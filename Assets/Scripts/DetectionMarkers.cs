using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using UnityEngine.Rendering;
using Vive.Plugin.SR;

public class DetectionMarkers : MonoBehaviour
{

	//Aruco variables
	private DetectorParameters detectorParameters;
	private Dictionary dictionary;
	Texture2D left;
	Texture2D leftCPU;
	Point3f[] markerPoints;
	[Tooltip("Marker length in meters")]
	[SerializeField] private float markerLength = 0.06f;
	[SerializeField] private PredefinedDictionaryName dictionaryName = PredefinedDictionaryName.Dict4X4_50;


	//Aruco results
	private Point2f[][] corners;
	private int[] ids;
	//private Point2f[][] rejectedImgPoints;
	double[,] rotMat = new double[3, 3] { { 0d, 0d, 0d }, { 0d, 0d, 0d }, { 0d, 0d, 0d } };
	private double[] rvec;
	private double[] tvec;

	//To extract the HMD video frame from GPU to CPU
	AsyncGPUReadbackRequest request;
	private bool initialized=false;

	//Camera parameters
	private double[,] cameraMatrix;
	double[] distCoeffs = new double[4] { 0d, 0d, 0d, 0d }; //no distortion with the Vive Pro 2
 	private bool isCameraInitialized = false;
	private Matrix4x4 leftPose;



	//Object to make appear on tracker
	public TransformSmoother cubeToMove;



	void Start()
	{
		markerPoints = new Point3f[] {
				new Point3f(-markerLength / 2f,  markerLength / 2f, 0f),
				new Point3f( markerLength / 2f,  markerLength / 2f, 0f),
				new Point3f( markerLength / 2f, -markerLength / 2f, 0f),
				new Point3f(-markerLength / 2f, -markerLength / 2f, 0f)
			};


		// Create default parameres for detection
		detectorParameters = DetectorParameters.Create();

		// Dictionary holds set of all available markers
		dictionary = CvAruco.GetPredefinedDictionary(dictionaryName);

	}

	void InitTextures()
	{
		left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
	}

	void OnCompleteReadback(AsyncGPUReadbackRequest request)
	{
		var tex = new Texture2D(Vive.Plugin.SR.ViveSR_DualCameraImageCapture.UndistortedImageWidth, Vive.Plugin.SR.ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		tex.LoadRawTextureData(request.GetData<uint>());
		tex.Apply();
		Graphics.CopyTexture(tex, leftCPU);
		Destroy(tex);
	}

	void InitCamera()
    {
		cameraMatrix = new double[3, 3] {
				{ViveSR_DualCameraImageCapture.FocalLengthLeft, 0d, ViveSR_DualCameraImageCapture.UndistortedCxLeft},
				{0d, ViveSR_DualCameraImageCapture.FocalLengthLeft, ViveSR_DualCameraImageCapture.UndistortedCyLeft},
				{0d, 0d, 1d}
			};

	}

	private void Update()
	{

		if (!initialized)
		{
			InitTextures();
			initialized = true;
		}
		if (!isCameraInitialized && ViveSR_DualCameraImageCapture.FocalLengthLeft > 0)
		{
			InitCamera();
			isCameraInitialized = true;
		}


		if (initialized && isCameraInitialized && request.done)
		{
			Mat mat = OpenCvSharp.Unity.TextureToMat(leftCPU);
			Mat flippedMat = new Mat();
			Cv2.Flip(mat, flippedMat, FlipMode.Y);
			Mat grayMat = new Mat();
			Cv2.CvtColor(flippedMat, grayMat, ColorConversionCodes.BGRA2GRAY);

			// Detect and draw markers
			CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out _);
			//CvAruco.DrawDetectedMarkers(flippedMat, corners, ids);

			if (ids.Length > 0)
			{
				for (int i = 0; i < ids.Length; i++)
				{
					if (ids[i] == 0)
					{
						Cv2.SolvePnP(markerPoints, corners[i], cameraMatrix, distCoeffs, out rvec, out tvec, false, SolvePnPFlags.Iterative);
						Cv2.Rodrigues(rvec, out rotMat);
						UpdateObjectPosition();
					}
				}

			}

			grayMat.Dispose();
			mat.Dispose();
			flippedMat.Dispose();

			ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out leftPose, out _);
			request = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadback);
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

	private void UpdateObjectPosition()
    {
		Vector3 cameraSpacePos = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); //x is negative due to flipping of camera image

		Vector3 worldPos = leftPose.MultiplyPoint(cameraSpacePos);

		Quaternion cameraRot = QuaternionFromMatrix(rotMat);

		cameraRot.y = -cameraRot.y;   //have to invert y and z due to right hand convention in OpenCV
		cameraRot.z = -cameraRot.z;   //and left hand convention in Unity

		Quaternion cameraPos = leftPose.rotation;

		cubeToMove.SetNewTransform(worldPos, cameraPos * cameraRot);
	}
}