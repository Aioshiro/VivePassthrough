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
	Point3f[][] markerPoints; //Marker points in object space
	[Tooltip("Marker length in meters")]
	[SerializeField] private List<float> markerLength;
	[Tooltip("Aruco dictionnary to use")]
	[SerializeField] private PredefinedDictionaryName dictionaryName = PredefinedDictionaryName.Dict4X4_50;
	[SerializeField] int numberOfMarkers = 10;
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

	[SerializeField] Material planeMaterial;

	void Start()
	{
		//markersManager = GetComponent<MarkersManagerMulti>();
		markersManager = GetComponent<MarkersManager>();
		markersToUpdateLeft = new bool[numberOfMarkers];
		markersToUpdateRight = new bool[numberOfMarkers];
		InitArucoParameters();
	}
	/// <summary>
	/// Initializing aruco's parameters
	/// </summary>
	void InitArucoParameters()
    {
		markerPoints = new Point3f[numberOfMarkers][];
		for (int i = 0; i < numberOfMarkers; i++)
		{
			markerPoints[i] = new Point3f[] {
				new Point3f(-markerLength[i] / 2f,  markerLength[i] / 2f, 0f),
				new Point3f( markerLength[i] / 2f,  markerLength[i] / 2f, 0f),
				new Point3f( markerLength[i] / 2f, -markerLength[i] / 2f, 0f),
				new Point3f(-markerLength[i] / 2f, -markerLength[i] / 2f, 0f)
			};
		}

		// Create default parameres for detection
		detectorParameters = DetectorParameters.Create();
		detectorParameters.DoCornerRefinement = useCornerRefinement;

		//detectorParameters.AdaptiveThreshConstant = 7; // found no effect
		//detectorParameters.AdaptiveThreshWinSizeMax = 23; //found no effect
		//detectorParameters.AdaptiveThreshWinSizeMin = 3; //found no effect
		//detectorParameters.AdaptiveThreshWinSizeStep = 10; //found no effect
		detectorParameters.CornerRefinementWinSize = 9;
		//detectorParameters.CornerRefinementMinAccuracy = 0.1; //found no effect
		//detectorParameters.CornerRefinementMaxIterations = int.MaxValue; //found no effect
		//detectorParameters.MarkerBorderBits =1 //always one for us
		//detectorParameters.MaxErroneousBitsInBorderRate = 0.35 //no issues or erroneous bits
		//detectorParameters.MaxMarkerPerimeterRate = 4;
		//detectorParameters.MinCornerDistanceRate = 0.05;
		detectorParameters.MinDistanceToBorder = 100; //increased quite a bit, as the other person head tend to be in the center
		//detectorParameters.MinMarkerDistanceRate = 0.05;
		detectorParameters.MinMarkerPerimeterRate = 0.04; //Increase a little bit to filter small wrong markers
		 //detectorParameters.MinOtsuStdDev = 5; no effect
		//detectorParameters.PolygonalApproxAccuracyRate = 0.03; no effect

		// Dictionary holds set of all available markers
		dictionary = CvAruco.GetPredefinedDictionary(dictionaryName);

		rvecLeft = new double[numberOfMarkers][];
		tvecLeft = new double[numberOfMarkers][];

		rotMatLeft = new double[numberOfMarkers][,];

		rvecRight = new double[numberOfMarkers][];
		tvecRight = new double[numberOfMarkers][];

		rotMatRight = new double[numberOfMarkers][,];
	}

	public void IncreaseWindow(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
		if (context.started)
		{
			detectorParameters.CornerRefinementWinSize += 1;
			Debug.Log("Window size is " + detectorParameters.CornerRefinementWinSize.ToString());
		}
    }
	public void DecreaseWindow(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
		if (context.started)
		{
			detectorParameters.CornerRefinementWinSize -= 1;
			if (detectorParameters.CornerRefinementWinSize <= 0)
			{
				detectorParameters.CornerRefinementWinSize = 1;
			}
			Debug.Log("Window size is " + detectorParameters.CornerRefinementWinSize.ToString());
		}
	}

	public void IncreaseCornerMinAccuracy(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		if (context.started)
        {
			detectorParameters.CornerRefinementMinAccuracy *= 10;
			Debug.Log("Min accuracy is " + detectorParameters.CornerRefinementMinAccuracy.ToString());
        }
	}

	public void DecreaseCornerMinAccuracy(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		if (context.started)
		{
			detectorParameters.CornerRefinementMinAccuracy /= 10;
			Debug.Log("Min accuracy is " + detectorParameters.CornerRefinementMinAccuracy.ToString());
		}
	}

	public void DecreasePolygonalApproxAccuracyRate(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		if (context.started)
		{
			detectorParameters.PolygonalApproxAccuracyRate -= 0.01;
			if (detectorParameters.PolygonalApproxAccuracyRate <= 0)
            {
				detectorParameters.PolygonalApproxAccuracyRate = 0.01;
            }
			Debug.Log("Adaptive PolygonalApproxAccuracyRate is " + detectorParameters.PolygonalApproxAccuracyRate.ToString());
		}
	}

	public void IncreasePolygonalApproxAccuracyRate(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		if (context.started)
		{
			detectorParameters.PolygonalApproxAccuracyRate += 0.01;
			Debug.Log("Adaptive PolygonalApproxAccuracyRate is " + detectorParameters.PolygonalApproxAccuracyRate.ToString());
		}
	}


	/// <summary>
	/// Initializing the textures for the camera's outputs
	/// </summary>
	void InitTextures()
	{
		left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		rightCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
	}

	/// <summary>
	/// Called when left camera request is finished
	/// </summary>
	void OnCompleteReadbackLeft(AsyncGPUReadbackRequest request)
	{
		//Loading data into texture
		leftCPU.LoadRawTextureData(request.GetData<uint>());
	}

	/// <summary>
	/// Called when right camera request is finished
	/// </summary>
	/// <param name="request"></param>
	void OnCompleteReadbackRight(AsyncGPUReadbackRequest request)
	{
		//Loading data into texture
		rightCPU.LoadRawTextureData(request.GetData<uint>());
	}

	/// <summary>
	/// Initialize the left camera
	/// </summary>
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

	/// <summary>
	/// Initialize the right camera
	/// </summary>
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
		if(cameraLeft == null)
        {
			cameraLeft = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0);
        }
		if (cameraRight == null)
        {
			cameraRight = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(1);
		}
		if (useRightCamera && isCameraRightInitialized&&requestRight.done) //if the camera is initialized and we got the gpu texture
		{
			DetectMarkers(rightCPU, out corners, out ids); //Detect every marker on the image
			for (int i = 0; i < ids.Length; i++) //Updating position and rotation of every marker
			{
				if (ids[i] <numberOfMarkers)
				{
					markersToUpdateRight[ids[i]] = true;
					Cv2.SolvePnP(markerPoints[ids[i]], corners[i], cameraRightMatrix, distCoeffs, out rvecRight[ids[i]], out tvecRight[ids[i]], false, SolvePnPFlags.Iterative); //Pose estimation, to go from 2d pixels to 3d position in CAMERA space
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
					Cv2.SolvePnP(markerPoints[ids[i]], corners[i], cameraLeftMatrix, distCoeffs, out rvecLeft[ids[i]], out tvecLeft[ids[i]], false, SolvePnPFlags.Iterative); //Pose estimation, to go from 2d pixels to 3d position in CAMERA space
					Cv2.Rodrigues(rvecLeft[ids[i]], out rotMatLeft[ids[i]]);
				}
			}

			ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out _, out _);
			requestLeft = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadbackLeft);
			if (waitForCompletion) { requestLeft.WaitForCompletion(); }
		}

		UpdatingPoses(); //Updating markers 3d pos to manager
	}

	/// <summary>
	/// Sending calculated poses to Marker Manager
	/// </summary>
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
				//markersManager.SetActiveIthMarker(i, true);
			}
            else if(markersToUpdateLeft[i])
			{
				markersToUpdateLeft[i] = false;
				GetObjectNewTransform(tvecLeft[i],rotMatLeft[i],true, out Vector3 worldPos, out Quaternion worldRot); //estimating worldspace position
				markersManager.UpdateIthMarkerPos(i, worldPos, worldRot);
				//markersManager.SetActiveIthMarker(i, true);
			}
			else if (markersToUpdateRight[i])
            {
				markersToUpdateRight[i] = false;
				GetObjectNewTransform(tvecRight[i], rotMatRight[i],false, out Vector3 worldPos, out Quaternion worldRot); //estimating worldspace position
				markersManager.UpdateIthMarkerPos(i, worldPos, worldRot);
				//markersManager.SetActiveIthMarker(i, true);
			}
   //         else
   //         {
			//	//markersManager.SetActiveIthMarker(i, false);
			//}
		}
	}

	/// <summary>
	/// Go from camera space to world space
	/// </summary>
	/// <param name="tvec"> Translation vector of object</param>
	/// <param name="rotMat"> Rotation matrix of object</param>
	/// <param name="isLeft"> True if this is left camera, right otherwise</param>
	/// <param name="worldPos"> Object position in world space</param>
	/// <param name="worldRot"> Object rotation in world space</param>
	private void GetObjectNewTransform(double[] tvec,double[,] rotMat, bool isLeft,out Vector3 worldPos, out Quaternion worldRot)
    {
		worldPos = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); //initial pos in cameraSpace,  x is negative due to flipping of camera image

		//now going from camera space to worldspace
		if (isLeft)
		{
			worldPos = cameraLeft.TransformPoint(worldPos); //straightforward transformation for position
		}
		else
		{
			worldPos = cameraRight.TransformPoint(worldPos); //straightforward transformation for position
		}

		Quaternion cameraRot = QuaternionUtil.QuaternionFromMatrix(rotMat); //get the rotation in camera space

		cameraRot.y = -cameraRot.y;   //have to invert y and z due to right hand convention in OpenCV
		cameraRot.z = -cameraRot.z;   //and left hand convention in Unity

		//Quaternion cameraPos;
		if (isLeft)
        {
			worldRot = cameraLeft.rotation * cameraRot;
			//cameraPos = cameraLeft.rotation; //get the camera rotation
		}
        else
        {
			worldRot = cameraRight.rotation * cameraRot;
			//cameraPos = cameraRight.rotation; //get the camera rotation
		}

		//worldRot = cameraPos * cameraRot; //composition of rotations by multiplying quaternions

	}

	/// <summary>
	/// Detects the markers on an image
	/// </summary>
	/// <param name="image"> The Texture2D to analyze</param>
	/// <param name="corners"> Output of the corners of all detected markers, size of [numberOfMarkersDetected,4] </param>
	/// <param name="ids"> Output of the ids associated with each markers, corners[i] corresponds to the marker with id ids[i] </param>
    private void DetectMarkers(Texture2D image, out Point2f[][] corners,out int[] ids)
    {
		using (Mat flippedMat = new Mat())
		{
			Cv2.Flip(OpenCvSharp.Unity.TextureToMat(image), flippedMat, FlipMode.Y); //flipping input cameras, as vive feed is reversed
			CvAruco.DetectMarkers(flippedMat, dictionary, out corners, out ids, detectorParameters, out _);
			//CvAruco.DrawDetectedMarkers(flippedMat, corners, ids);
			//planeMaterial.mainTexture = OpenCvSharp.Unity.MatToTexture(flippedMat);
		}
	}

	/// <summary>
	/// Waiting for end of request when destroying, to fix errors when closing the app
	/// </summary>
	private void OnDestroy()
    {
		requestLeft.WaitForCompletion();
		requestRight.WaitForCompletion();
    }
}