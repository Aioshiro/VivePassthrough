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
	[Tooltip("Marker length in meters")]
	[SerializeField] private float markerLength = 0.06f;
	[SerializeField] private PredefinedDictionaryName dictionaryName = PredefinedDictionaryName.Dict4X4_50;
	const int numberOfMarkers = 4;
	MarkersManager markersManager;
	bool[] markersToUpdate;

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

	//bool updatedLeftPose = false;
	//bool updatedRightPose = false;

	//Matrix4x4 rightPose;
	//Matrix4x4 leftPose;

	//[SerializeField] Transform cameraLeft;
	[SerializeField] Transform cameraRight;

	//Object to make appear on tracker
	//public TransformSmoother cubeToMove;

	//public bool useLeftCamera;
	//public bool useRightCamera;
	public bool useCornerRefinement;
	public bool waitForCompletion;


	//GameObject[] spheresTest;
	//private int imageNumber = 0;

	void Start()
	{
		markersManager = GetComponent<MarkersManager>();
		markersToUpdate = new bool[numberOfMarkers];
		InitArucoParameters();
		//spheresTest = new GameObject[4 * 4];
		//for (int i = 0; i < 16; i++)
  //      {
		//	spheresTest[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//	spheresTest[i].transform.localScale = Vector3.one * 0.01f;
  //      }
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
		detectorParameters.DoCornerRefinement = useCornerRefinement;
		detectorParameters.CornerRefinementMinAccuracy = 0.00001;
		detectorParameters.CornerRefinementMaxIterations = 100000;

		// Dictionary holds set of all available markers
		dictionary = CvAruco.GetPredefinedDictionary(dictionaryName);

		//rvecLeft = new double[numberOfMarkers][];
		//tvecLeft = new double[numberOfMarkers][];
		rvecRight = new double[numberOfMarkers][];
		tvecRight = new double[numberOfMarkers][];

		//rotMatLeft = new double[numberOfMarkers][,];
		rotMatRight = new double[numberOfMarkers][,];
	}

	void InitTextures()
	{
		//left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		//leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		rightCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
	}

	//void OnCompleteReadbackLeft(AsyncGPUReadbackRequest request)
	//{
	//	//texLeftTemp = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
	//	//texLeftTemp.LoadRawTextureData(request.GetData<uint>());
	//	//texLeftTemp.Apply();
	//	//Graphics.CopyTexture(texLeftTemp, leftCPU);
	//	leftCPU.LoadRawTextureData(request.GetData<uint>());
	//}
	void OnCompleteReadbackRight(AsyncGPUReadbackRequest request)
	{
		//texRightTemp = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		//texRightTemp.LoadRawTextureData(request.GetData<uint>());
		//texRightTemp.Apply();
		//Graphics.CopyTexture(texRightTemp, rightCPU);
		rightCPU.LoadRawTextureData(request.GetData<uint>());
	}

	//void InitLeftCamera()
 //   {
	//	cameraLeftMatrix = new double[3, 3] {
	//			{ViveSR_DualCameraImageCapture.FocalLengthLeft, 0d, ViveSR_DualCameraImageCapture.UndistortedCxLeft},
	//			{0d, ViveSR_DualCameraImageCapture.FocalLengthLeft, ViveSR_DualCameraImageCapture.UndistortedCyLeft},
	//			{0d, 0d, 1d}
	//		};
	//	//ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out leftPose, out _);
	//	ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out _, out _);
	//	if (left != null)
	//	{
	//		requestLeft = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadbackLeft);
	//		isCameraLeftInitialized = true;
	//	}
	//}
	void InitRightCamera()
	{
		cameraRightMatrix = new double[3, 3] {
				{ViveSR_DualCameraImageCapture.FocalLengthRight, 0d, ViveSR_DualCameraImageCapture.UndistortedCxRight},
				{0d, ViveSR_DualCameraImageCapture.FocalLengthRight, ViveSR_DualCameraImageCapture.UndistortedCyRight},
				{0d, 0d, 1d}
			};

		//ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out rightPose);
		ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out _);
		if (right != null)
		{
			requestRight = AsyncGPUReadback.Request(right, 0, TextureFormat.RGBA32, OnCompleteReadbackRight);
			isCameraRightInitialized = true;
		}
	}

	private void Update()
	{
		if (!initialized && ViveSR_DualCameraImageCapture.UndistortedImageWidth>0)
		{
			InitTextures();
			initialized = true;
		}
		if (!initialized) { return; }
		//if (!isCameraLeftInitialized && ViveSR_DualCameraImageCapture.FocalLengthLeft > 0)
		//{
		//	InitLeftCamera();
		//}
		if (!isCameraRightInitialized && ViveSR_DualCameraImageCapture.FocalLengthRight > 0)
		{
			InitRightCamera();
		}

		//if (isCameraLeftInitialized && useLeftCamera && requestLeft.done)
		//{
		//	DetectMarkers(leftCPU, out corners, out ids);

		//	for (int i = 0; i < ids.Length; i++)
		//	{
		//		if (ids[i] <numberOfMarkers)
		//		{
		//			Cv2.SolvePnP(markerPoints, corners[i], cameraLeftMatrix, distCoeffs, out rvecLeft[ids[i]], out tvecLeft[ids[i]], false, SolvePnPFlags.Iterative);
		//			Cv2.Rodrigues(rvecLeft[ids[i]], out rotMatLeft[ids[i]]);
		//			updatedLeftPose = true;
		//		}
		//	}

		//	//ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out leftPose, out _);
		//	ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out _, out _);
		//	requestLeft = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadbackLeft);
		//	if (waitForCompletion) { requestLeft.WaitForCompletion(); }
		//}

		//if (isCameraRightInitialized&& useRightCamera && requestRight.done)
		if (isCameraRightInitialized&&requestRight.done)
		{
			DetectMarkers(rightCPU, out corners, out ids);
			for (int i = 0; i < ids.Length; i++)
			{
				if (ids[i] <numberOfMarkers)
				{
					markersToUpdate[ids[i]] = true;
					Cv2.SolvePnP(markerPoints, corners[i], cameraRightMatrix, distCoeffs, out rvecRight[ids[i]], out tvecRight[ids[i]], false, SolvePnPFlags.Iterative);
					Cv2.Rodrigues(rvecRight[ids[i]], out rotMatRight[ids[i]]);
				}
			}

			//ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out rightPose);
			ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out _);
			requestRight = AsyncGPUReadback.Request(right, 0, TextureFormat.RGBA32, OnCompleteReadbackRight);
			if (waitForCompletion) { requestRight.WaitForCompletion(); }
		}

		UpdatingPoses();
		//DrawMarkers();
	}

	private void UpdatingPoses()
    {
		//just taking into account marker 0 for now
		//if (updatedLeftPose && updatedRightPose) //taking the average of the two values
		//{
		//	//double[] avgPosLeft = { (tvecLeft[0][0] + tvecLeft[1][0]) / 2, (tvecLeft[0][1] + tvecLeft[1][1]) / 2, (tvecLeft[0][2] + tvecLeft[1][2]) / 2 };
		//	//double[] avgPosRight = { (tvecRight[0][0] + tvecRight[1][0]) / 2, (tvecRight[0][1] + tvecRight[1][1]) / 2, (tvecRight[0][2] + tvecRight[1][2]) / 2 };

		//	GetObjectNewTransform(tvecRight[0], rotMatRight[0], false, out Vector3 worldPosRight, out Quaternion worldRotRight);
		//	GetObjectNewTransform(tvecLeft[0], rotMatLeft[0], true, out Vector3 worldPosLeft, out Quaternion worldRotLeft);
		//	cubeToMove.SetNewTransform(Vector3.Lerp(worldPosLeft, worldPosRight, 0.5f), Quaternion.Slerp(worldRotLeft, worldRotRight, 0.5f));
		//	updatedLeftPose = false;
		//	updatedRightPose = false;
		//	//cubeToMove.gameObject.SetActive(true);

		//}
		//else if (updatedRightPose) //taking the right result
		//{
		//	//double[] avgPosRight = { (tvecRight[0][0] + tvecRight[1][0]) / 2, (tvecRight[0][1] + tvecRight[1][1]) / 2, (tvecRight[0][2] + tvecRight[1][2]) / 2 };

		//	GetObjectNewTransform(tvecRight[0], rotMatRight[0], false, out Vector3 worldPos, out Quaternion worldRot);
		//	cubeToMove.SetNewTransform(worldPos, worldRot);
		//	updatedRightPose = false;
		//	//cubeToMove.gameObject.SetActive(true);


		//}
		//else if (updatedLeftPose) //taking the left result
		//{
		//	//double[] avgPosLeft = { (tvecLeft[0][0] + tvecLeft[1][0]) / 2, (tvecLeft[0][1] + tvecLeft[1][1]) / 2, (tvecLeft[0][2] + tvecLeft[1][2]) / 2 };
		//	GetObjectNewTransform(tvecLeft[0], rotMatLeft[0], true, out Vector3 worldPos, out Quaternion worldRot);
		//	cubeToMove.SetNewTransform(worldPos, worldRot);
		//	updatedLeftPose = false;
		//	//cubeToMove.gameObject.SetActive(true);

		//}
		//else
  //      {
		//	//cubeToMove.gameObject.SetActive(false);
  //      }

		for (int i = 0; i < markersToUpdate.Length; i++)
        {
			if (markersToUpdate[i])
            {
				markersToUpdate[i] = false;
				GetObjectNewTransform(tvecRight[i], rotMatRight[i], out Vector3 worldPos, out Quaternion worldRot);
				markersManager.UpdateIthMarkerPos(i, worldPos, worldRot);
            }
        }
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

	private void GetObjectNewTransform(double[] tvec,double[,] rotMat,out Vector3 worldPos, out Quaternion worldRot)
    {
		Vector3 cameraSpacePos = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); //x is negative due to flipping of camera image

		//worldPos = rightPose.MultiplyPoint(cameraSpacePos);
		worldPos = cameraRight.TransformPoint(cameraSpacePos);

		Quaternion cameraRot = QuaternionFromMatrix(rotMat);

		cameraRot.y = -cameraRot.y;   //have to invert y and z due to right hand convention in OpenCV
		cameraRot.z = -cameraRot.z;   //and left hand convention in Unity

		Quaternion cameraPos = cameraRight.rotation;

		worldRot = cameraPos * cameraRot;

	}

	//private void DrawMarkers()
 //   {
	//	for (int i = 0; i < tvecRight.Length; i++)
 //       {
	//		Vector3 cameraSpacePos = new Vector3(-(float)tvecRight[i][0], (float)tvecRight[i][1], (float)tvecRight[i][2]); //x is negative due to flipping of camera image
	//		cameraSpacePos = rightPose.MultiplyPoint(cameraSpacePos);
	//		spheresTest[i].transform.position = cameraSpacePos;
	//	}
	//}

	private void DetectMarkers(Texture2D image, out Point2f[][] corners,out int[] ids)
    {
		Mat flippedMat = new Mat();
		Cv2.Flip(OpenCvSharp.Unity.TextureToMat(image), flippedMat, FlipMode.Y); //flipping input cameras, as vive feed is reversed
		CvAruco.DetectMarkers(flippedMat, dictionary, out corners, out ids, detectorParameters, out _);
		flippedMat.Dispose();
	}

    private void OnDestroy()
    {
		requestRight.WaitForCompletion();
		//requestLeft.WaitForCompletion();
    }
}