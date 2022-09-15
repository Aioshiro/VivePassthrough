using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using UnityEngine.Rendering;
using Vive.Plugin.SR;

public class DetectionMarkers : MonoBehaviour
{

	private DetectorParameters detectorParameters;
	private Dictionary dictionary;
	public Point2f[][] corners;
	private int[] ids;
	private Point2f[][] rejectedImgPoints;
	public Renderer initialImage;
	public Renderer convertedImage;
	public Renderer finalImage;

	Texture2D left;
	Texture2D leftCPU;
	Texture2D gray;

	Point3f[] markerPoints;
	private float markerLength=0.06f;
	private double[,] cameraMatrix;
	double[] distCoeffs = new double[4] { 0d, 0d, 0d, 0d };
	double[,] rotMat = new double[3, 3] { { 0d, 0d, 0d }, { 0d, 0d, 0d }, { 0d, 0d, 0d } };
	private bool isCameraInitialized;
	private Matrix4x4 leftPose;


	public GameObject cubeToMove;

	//To extract the HMD video frame from GPU to CPU
	AsyncGPUReadbackRequest request;
	private bool initialized;


	void Start()
	{
		 markerPoints= new Point3f[] {
				new Point3f(-markerLength / 2f,  markerLength / 2f, 0f),
				new Point3f( markerLength / 2f,  markerLength / 2f, 0f),
				new Point3f( markerLength / 2f, -markerLength / 2f, 0f),
				new Point3f(-markerLength / 2f, -markerLength / 2f, 0f)
			};


		// Create default parameres for detection
		detectorParameters = DetectorParameters.Create();

		// Dictionary holds set of all available markers
		dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);

	}

	void InitTextures()
	{
		left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
		gray = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
	}

	void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        var tex = new Texture2D(Vive.Plugin.SR.ViveSR_DualCameraImageCapture.UndistortedImageWidth, Vive.Plugin.SR.ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        tex.LoadRawTextureData(request.GetData<uint>());
        tex.Apply();
        Graphics.CopyTexture(tex, leftCPU);
        Destroy(tex);
    }

    private void Update()
	{

		if (!initialized)
		{
			InitTextures();
			initialized = true;
		}
		if (!isCameraInitialized  && ViveSR_DualCameraImageCapture.FocalLengthLeft > 0)
        {
			cameraMatrix = new double[3, 3] {
				{ViveSR_DualCameraImageCapture.FocalLengthLeft, 0d, ViveSR_DualCameraImageCapture.UndistortedCxLeft},
				{0d, ViveSR_DualCameraImageCapture.FocalLengthLeft, ViveSR_DualCameraImageCapture.UndistortedCyLeft},
				{0d, 0d, 1d}
			};

			isCameraInitialized = true;
		}
		

		if (initialized&& isCameraInitialized && request.done)
		{
			initialImage.material.mainTexture = leftCPU;
			Mat mat = OpenCvSharp.Unity.TextureToMat(leftCPU);
			Mat flippedMat = new Mat();
			Cv2.Flip(mat, flippedMat, FlipMode.Y);
			Mat grayMat = new Mat();
			Cv2.CvtColor(flippedMat, grayMat, ColorConversionCodes.BGRA2GRAY);
			gray = OpenCvSharp.Unity.MatToTexture(grayMat);

			convertedImage.material.mainTexture = gray;

			// Detect and draw markers
			CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
			CvAruco.DrawDetectedMarkers(flippedMat, corners, ids);

			if (ids.Length > 0)
            {
				for (int i = 0; i < ids.Length; i++)
				{
					if (ids[i] == 0)
					{
						Cv2.SolvePnP(markerPoints, corners[i], cameraMatrix, distCoeffs, out double[] rvec, out double[] tvec, false, SolvePnPFlags.Iterative);
						Cv2.Rodrigues(rvec, out rotMat);

						Vector3 cameraSpacePos = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); //x is negative due to flipping of camera
						Quaternion cameraSpaceRot = Quaternion.Euler((float) rvec[0], (float) rvec[1], (float) rvec[2]);

						Vector3 worldPos = leftPose.MultiplyPoint(cameraSpacePos);

						cubeToMove.transform.position = worldPos;
						cubeToMove.transform.rotation = cameraSpaceRot;
					}
				}



			}

			Texture2D outputTexture = OpenCvSharp.Unity.MatToTexture(flippedMat);
			finalImage.material.mainTexture = outputTexture;



			grayMat.Dispose();
			mat.Dispose();
			flippedMat.Dispose();

			ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out _, out _, out _, out leftPose, out _);
			request = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadback);
		}



	}

}