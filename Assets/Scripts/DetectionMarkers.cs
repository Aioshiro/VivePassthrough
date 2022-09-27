using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using UnityEngine.Rendering;
using Vive.Plugin.SR;

public class DetectionMarkers : MonoBehaviour
{

	//Aruco variables
	private DetectorParameters detectorParameters;
	private Dictionary dictionary;
	//Texture2D left;
	Texture2D right;
	//Texture2D leftCPU;
	Texture2D rightCPU;
	Point3f[] markerPoints;
	[Tooltip("Marker length in meters")] [SerializeField] private float markerLength = 0.06f;
	[SerializeField] private PredefinedDictionaryName dictionaryName = PredefinedDictionaryName.Dict4X4_50;
	const int numberOfMarkers = 4;


	//Aruco results
	private Point2f[][] corners;
	private int[] ids;
	//private Point2f[][] rejectedImgPoints;
	//double[][,] rotMatLeft;
	double[][,] rotMatRight;
	//private double[][] rvecLeft;
	//private double[][] tvecLeft;
	private double[][] rvecRight;
	private double[][] tvecRight;

	//To extract the HMD video frame from GPU to CPU
	//AsyncGPUReadbackRequest requestLeft;
	private bool initialized=false;
	AsyncGPUReadbackRequest requestRight;

	//Camera parameters
	//private double[,] cameraLeftMatrix;
	private double[,] cameraRightMatrix;
	readonly double[] distCoeffs = new double[4] { 0d, 0d, 0d, 0d }; //no distortion with the Vive Pro 2
 	//private bool isCameraLeftInitialized = false;
	private bool isCameraRightInitialized = false;
	//private Matrix4x4 leftPose;
	private Matrix4x4 rightPose;

	//bool updatedLeftPose = false;
	//bool updatedRightPose = false;

	//Object to make appear on tracker
	public TransformSmoother cubeToMove;

	[SerializeField] private bool waitForRequestCompletion = false;

	void Start()
	{
		InitArucoParameters();
	}

	void InitArucoParameters()
    {
		markerPoints = new Point3f[] {
				new Point3f(-markerLength / 2f,  markerLength / 2f, 0f),
				new Point3f( markerLength / 2f,  markerLength / 2f, 0f),
				new Point3f( markerLength / 2f, -markerLength / 2f, 0f),
				new Point3f(-markerLength / 2f, -markerLength / 2f, 0f)
			};

		// Create default parameres for detection
		detectorParameters = DetectorParameters.Create();
		detectorParameters.DoCornerRefinement = true;
		detectorParameters.CornerRefinementMinAccuracy = 0.0001;
		detectorParameters.CornerRefinementMaxIterations = 100;

		// Dictionary holds set of all available markers
		dictionary = CvAruco.GetPredefinedDictionary(dictionaryName);

		//rvecLeft = new double[numberOfMarkers][];
		//tvecLeft = new double[numberOfMarkers][];
		rvecRight = new double[numberOfMarkers][];
		tvecRight = new double[numberOfMarkers][];

		//rotMatLeft = new double[numberOfMarkers][,];
		rotMatRight = new double[numberOfMarkers][,];

		for (int i = 0; i < numberOfMarkers; i++)
        {
			rvecRight[i] = null;
			tvecRight[i] = null;
			rotMatRight[i] = null;
        }

	}

	void InitTextures()
	{
		//left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		//leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		rightCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
	}

	void OnCompleteReadbackRight(AsyncGPUReadbackRequest request)
	{
		var tex = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		tex.LoadRawTextureData(request.GetData<uint>());
		tex.Apply();
		Graphics.CopyTexture(tex, rightCPU);
		Destroy(tex);
	}

	void InitRightCamera()
	{
		cameraRightMatrix = new double[3, 3] {
				{ViveSR_DualCameraImageCapture.FocalLengthRight, 0d, ViveSR_DualCameraImageCapture.UndistortedCxRight},
				{0d, ViveSR_DualCameraImageCapture.FocalLengthRight, ViveSR_DualCameraImageCapture.UndistortedCyRight},
				{0d, 0d, 1d}
			};

		ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out rightPose);
		if (right != null)
		{
			requestRight = AsyncGPUReadback.Request(right, 0, TextureFormat.RGBA32, OnCompleteReadbackRight);
			isCameraRightInitialized = true;
		}
	}

	private void Update()
	{
		//Can't do it in start, as some parameters are not updated on first frame
		if (!initialized && ViveSR_DualCameraImageCapture.UndistortedImageWidth>0)
		{
			InitTextures();
			initialized = true;
		}

		if (!isCameraRightInitialized && ViveSR_DualCameraImageCapture.FocalLengthRight > 0)
		{
			InitRightCamera();
		}
        if (!initialized || !isCameraRightInitialized) { return; }

		UpdateMarkerPoses();

		UpdateCameraImage();

		UpdateObjectTransform();
    }

	private void UpdateMarkerPoses()
    {
		DetectMarkers(rightCPU, out corners, out ids, out _);

		if (ids.Length > 0)
		{
			for (int i = 0; i < ids.Length; i++)
			{
				if (ids[i] < numberOfMarkers)
				{
					Cv2.SolvePnP(markerPoints, corners[i], cameraRightMatrix, distCoeffs, out rvecRight[ids[i]], out tvecRight[ids[i]], false, SolvePnPFlags.Iterative);
					Cv2.Rodrigues(rvecRight[ids[i]], out rotMatRight[ids[i]]);
				}
			}
		}

	}

	private void UpdateCameraImage()
    {
		ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out rightPose);
		requestRight = AsyncGPUReadback.Request(right, 0, TextureFormat.RGBA32, OnCompleteReadbackRight);
		if (waitForRequestCompletion)
		{
			requestRight.WaitForCompletion();
		}

	}

	private void UpdateObjectTransform()
    {
		if (tvecRight[0] == null || tvecRight[2] == null) { return; }
		double[] averagePosRight = new double[] { (tvecRight[0][0] + tvecRight[2][0]) / 2, (tvecRight[0][1] + tvecRight[2][1]) / 2, (tvecRight[0][2] + tvecRight[2][2]) / 2 };
		GetObjectNewTransform(averagePosRight, rotMatRight[0], rightPose, out Vector3 worldPos, out Quaternion worldRot);
		cubeToMove.SetNewTransform(worldPos, worldRot);
	}
	private Quaternion QuaternionFromMatrix(double[,] m)
	{
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion
		{
			w = Mathf.Sqrt(Mathf.Max(0, (float)(1 + m[0, 0] + m[1, 1] + m[2, 2]))) / 2,
			x = Mathf.Sqrt(Mathf.Max(0, (float)(1 + m[0, 0] - m[1, 1] - m[2, 2]))) / 2,
			y = Mathf.Sqrt(Mathf.Max(0, (float)(1 - m[0, 0] + m[1, 1] - m[2, 2]))) / 2,
			z = Mathf.Sqrt(Mathf.Max(0, (float)(1 - m[0, 0] - m[1, 1] + m[2, 2]))) / 2
		};
		q.x *= Mathf.Sign((float)(q.x * (m[2, 1] - m[1, 2])));
		q.y *= Mathf.Sign((float)(q.y * (m[0, 2] - m[2, 0])));
		q.z *= Mathf.Sign((float)(q.z * (m[1, 0] - m[0, 1])));
		return q;
	}

	private void GetObjectNewTransform(double[] tvec,double[,] rotMat,Matrix4x4 cameraPose,out Vector3 worldPos, out Quaternion worldRot)
    {
		Vector3 cameraSpacePos = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); //x is negative due to flipping of camera image

		worldPos = cameraPose.MultiplyPoint(cameraSpacePos);

		Quaternion cameraRot = QuaternionFromMatrix(rotMat);

		cameraRot.y = -cameraRot.y;   //have to invert y and z due to right hand convention in OpenCV
		cameraRot.z = -cameraRot.z;   //and left hand convention in Unity

		Quaternion cameraPos = cameraPose.rotation;

		worldRot = cameraPos * cameraRot;

	}

	private void DetectMarkers(Texture2D image, out Point2f[][] corners,out int[] ids, out Point2f[][] rejectedImgPoints)
    {
		Mat flippedMat = new Mat();
		Cv2.Flip(OpenCvSharp.Unity.TextureToMat(image), flippedMat, FlipMode.Y); //flipping input cameras, as vive feed is reversed
		CvAruco.DetectMarkers(flippedMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
		flippedMat.Dispose();
	}


}