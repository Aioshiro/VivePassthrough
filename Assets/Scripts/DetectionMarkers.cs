using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using UnityEngine.Rendering;
using Vive.Plugin.SR;
using System.Collections.Generic;

public class DetectionMarkers : MonoBehaviour
{

	//Aruco variables
	private DetectorParameters detectorParameters;
	private Dictionary dictionary;
	Texture2D left;
	Texture2D leftCPU;
	Texture2D right;
	Texture2D rightCPU;
	[Tooltip("Check if you want to do AruCo corner refinement (advised)")]
	public bool useCornerRefinement;
	Point3f[] markerPoints; //Marker points in object space
	[Tooltip("Marker length in meters")]
	[SerializeField] private float markerLength = 0.06f;
	[Tooltip("Aruco dictionnary to use")]
	[SerializeField] private PredefinedDictionaryName dictionaryName = PredefinedDictionaryName.Dict4X4_50;
	const int numberOfMarkers = 4;
	MarkersManager markersManager; //Manager for markers position and smoothering
	//MarkersManagerMulti markersManager;
	bool[] markersToUpdateRight;
	bool[] markersToUpdateLeft;

	//Aruco results
	private Point2f[][] corners;
	private int[] ids;
	double[][,] rotMatLeft;
	double[][,] rotMatRight;
	double[][] rvecLeft;
	private double[][] rvecRight;
	double[][] tvecLeft;
	private double[][] tvecRight;

	//To extract the HMD video frame from GPU to CPU
	private bool initialized=false;
	AsyncGPUReadbackRequest requestLeft;
	AsyncGPUReadbackRequest requestRight;
	[Tooltip("Check if you want to wait at each frame for the GPU request to finish")]
	public bool waitForCompletion;

	//Camera parameters
	private double[,] cameraLeftMatrix;
	private double[,] cameraRightMatrix; //camera intrinsic parameters
    readonly double[] distCoeffs = new double[4] { 0d, 0d, 0d, 0d }; //no distortion with the Vive Pro 2
	private bool isCameraRightInitialized = false;
	private bool isCameraLeftInitialized = false;
	[Tooltip("Right camera transform")]
	[SerializeField] Transform cameraRight;
	[Tooltip("Left camera transform")]
	[SerializeField] Transform cameraLeft;
	public bool useLeftCamera;
	public bool useRightCamera;



	void Start()
	{
		//markersManager = GetComponent<MarkersManagerMulti>();
		markersManager = GetComponent<MarkersManager>();
		markersToUpdateLeft = new bool[numberOfMarkers];
		markersToUpdateRight = new bool[numberOfMarkers];
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
		detectorParameters.DoCornerRefinement = useCornerRefinement;
		detectorParameters.CornerRefinementWinSize = 2;
		detectorParameters.CornerRefinementMinAccuracy = 0.00001;
		detectorParameters.CornerRefinementMaxIterations = 100000;

		// Dictionary holds set of all available markers
		dictionary = CvAruco.GetPredefinedDictionary(dictionaryName);

		rvecLeft = new double[numberOfMarkers][];
		tvecLeft = new double[numberOfMarkers][];

		rotMatLeft = new double[numberOfMarkers][,];

		rvecRight = new double[numberOfMarkers][];
		tvecRight = new double[numberOfMarkers][];

		rotMatRight = new double[numberOfMarkers][,];
	}

	void InitTextures()
	{
		left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		rightCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
	}

	//Called when request is finished
	void OnCompleteReadbackLeft(AsyncGPUReadbackRequest request)
	{
		//Loading data into texture
		leftCPU.LoadRawTextureData(request.GetData<uint>());
	}

	//Called when request is finished
	void OnCompleteReadbackRight(AsyncGPUReadbackRequest request)
	{
		//Loading data into texture
		rightCPU.LoadRawTextureData(request.GetData<uint>());
	}

	void InitLeftCamera()
	{
		cameraLeftMatrix = new double[3, 3] {
				{ViveSR_DualCameraImageCapture.FocalLengthLeft, 0d, ViveSR_DualCameraImageCapture.UndistortedCxLeft},
				{0d, ViveSR_DualCameraImageCapture.FocalLengthLeft, ViveSR_DualCameraImageCapture.UndistortedCyLeft},
				{0d, 0d, 1d}
			};

		ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out _, out _);
		if (left != null) //Sometimes, camera is not active on first frame
		{
			requestLeft = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadbackLeft);
			isCameraLeftInitialized = true;
		}
	}

	void InitRightCamera()
	{
		cameraRightMatrix = new double[3, 3] {
				{ViveSR_DualCameraImageCapture.FocalLengthRight, 0d, ViveSR_DualCameraImageCapture.UndistortedCxRight},
				{0d, ViveSR_DualCameraImageCapture.FocalLengthRight, ViveSR_DualCameraImageCapture.UndistortedCyRight},
				{0d, 0d, 1d}
			};

		ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out _);
		if (right != null) //Sometimes, camera is not active on first frame
		{
			requestRight = AsyncGPUReadback.Request(right, 0, TextureFormat.RGBA32, OnCompleteReadbackRight);
			isCameraRightInitialized = true;
		}
	}

	private void Update()
	{
		if (!initialized && ViveSR_DualCameraImageCapture.UndistortedImageWidth>0) //Sometimes, capture is not initialized on first frame
		{
			InitTextures();
			initialized = true;
		}
		if (!initialized) { return; }

		if (!isCameraRightInitialized && ViveSR_DualCameraImageCapture.FocalLengthRight > 0) //Sometimes, capture is not initialized on first frame
		{
			InitRightCamera();
		}

		if (!isCameraLeftInitialized && ViveSR_DualCameraImageCapture.FocalLengthLeft > 0) //Sometimes, capture is not initialized on first frame
		{
			InitLeftCamera();
		}

		if (useRightCamera && isCameraRightInitialized&&requestRight.done) //if the camera is initialized and we got the gpu texture
		{
			DetectMarkers(rightCPU, out corners, out ids); //Detect every marker on the image
			for (int i = 0; i < ids.Length; i++) //Updating position and rotation of every marker
			{
				if (ids[i] <numberOfMarkers)
				{
					markersToUpdateRight[ids[i]] = true;
					Cv2.SolvePnP(markerPoints, corners[i], cameraRightMatrix, distCoeffs, out rvecRight[ids[i]], out tvecRight[ids[i]], false, SolvePnPFlags.Iterative); //Pose estimation, to go from 2d pixels to 3d position in CAMERA space
					Cv2.Rodrigues(rvecRight[ids[i]], out rotMatRight[ids[i]]);
				}
			}

			ViveSR_DualCameraImageCapture.GetUndistortedTexture(out _, out right, out _, out _, out _, out _);
			requestRight = AsyncGPUReadback.Request(right, 0, TextureFormat.RGBA32, OnCompleteReadbackRight);
			if (waitForCompletion) { requestRight.WaitForCompletion(); }
		}

		if (useLeftCamera && isCameraLeftInitialized && requestLeft.done) //if the camera is initialized and we got the gpu texture
		{
			DetectMarkers(leftCPU, out corners, out ids); //Detect every marker on the image
			for (int i = 0; i < ids.Length; i++) //Updating position and rotation of every marker
			{
				if (ids[i] < numberOfMarkers)
				{
					markersToUpdateLeft[ids[i]] = true;
					Cv2.SolvePnP(markerPoints, corners[i], cameraLeftMatrix, distCoeffs, out rvecLeft[ids[i]], out tvecLeft[ids[i]], false, SolvePnPFlags.Iterative); //Pose estimation, to go from 2d pixels to 3d position in CAMERA space
					Cv2.Rodrigues(rvecLeft[ids[i]], out rotMatLeft[ids[i]]);
				}
			}

			ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out _, out _);
			requestLeft = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadbackLeft);
			if (waitForCompletion) { requestLeft.WaitForCompletion(); }
		}

		UpdatingPoses(); //Updating markers 3d pos to manager
	}

	//Updating markers 3d pos to manager
	private void UpdatingPoses()
	{
		for (int i = 0; i < markersToUpdateLeft.Length; i++)
		{
			if (markersToUpdateLeft[i] && markersToUpdateRight[i]) //foreach marker detected and so needed to be updated
			{
				markersToUpdateLeft[i] = false;
				markersToUpdateRight[i] = false;
				GetObjectNewTransform(tvecRight[i], rotMatRight[i], false, out Vector3 worldPosRight, out Quaternion worldRotRight); //estimating worldspace position
				GetObjectNewTransform(tvecLeft[i], rotMatLeft[i],true, out Vector3 worldPosLeft, out Quaternion worldRotLeft); //estimating worldspace position
				markersManager.UpdateIthMarkerPos(i, (worldPosLeft + worldPosRight) / 2, Quaternion.Slerp(worldRotRight,worldRotLeft,0.5f));
			}
            else if(markersToUpdateLeft[i])
			{
				markersToUpdateLeft[i] = false;
				GetObjectNewTransform(tvecLeft[i],rotMatLeft[i],true, out Vector3 worldPos, out Quaternion worldRot); //estimating worldspace position
				markersManager.UpdateIthMarkerPos(i, worldPos, worldRot);
			}
			else if (markersToUpdateRight[i])
            {
				markersToUpdateRight[i] = false;
				GetObjectNewTransform(tvecRight[i], rotMatRight[i],false, out Vector3 worldPos, out Quaternion worldRot); //estimating worldspace position
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

	//Go from camera space to world space
	private void GetObjectNewTransform(double[] tvec,double[,] rotMat, bool isLeft,out Vector3 worldPos, out Quaternion worldRot)
    {
		Vector3 cameraSpacePos = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); //x is negative due to flipping of camera image

		if (isLeft)
		{
			worldPos = cameraLeft.TransformPoint(cameraSpacePos); //straightforward transformation for position
		}
		else
		{
			worldPos = cameraRight.TransformPoint(cameraSpacePos); //straightforward transformation for position
		}

		Quaternion cameraRot = QuaternionFromMatrix(rotMat); //get the rotation in camera space

		cameraRot.y = -cameraRot.y;   //have to invert y and z due to right hand convention in OpenCV
		cameraRot.z = -cameraRot.z;   //and left hand convention in Unity

		Quaternion cameraPos;
		if (isLeft)
        {
			cameraPos = cameraLeft.rotation; //get the camera rotation
		}
        else
        {
			cameraPos = cameraRight.rotation; //get the camera rotation
		}

		worldRot = cameraPos * cameraRot; //composition of rotations by multiplying quaternions

	}

	//Detect markers in the image
    private void DetectMarkers(Texture2D image, out Point2f[][] corners,out int[] ids)
    {
		Mat flippedMat = new Mat();
		Cv2.Flip(OpenCvSharp.Unity.TextureToMat(image), flippedMat, FlipMode.Y); //flipping input cameras, as vive feed is reversed
		CvAruco.DetectMarkers(flippedMat, dictionary, out corners, out ids, detectorParameters, out _);
		flippedMat.Dispose();
	}

	//Waiting for end of request when destroying, to fix errors when closing the app
    private void OnDestroy()
    {
		requestLeft.WaitForCompletion();
		requestRight.WaitForCompletion();
    }
}