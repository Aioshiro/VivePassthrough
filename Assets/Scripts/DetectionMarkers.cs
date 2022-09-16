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
	Texture2D left;
	Texture2D leftCPU;
	Texture2D gray;

	Point3f[] markerPoints;
	private float markerLength = 0.06f;
	private double[,] cameraMatrix;
	double[] distCoeffs = new double[4] { 0d, 0d, 0d, 0d };
	double[,] rotMat = new double[3, 3] { { 0d, 0d, 0d }, { 0d, 0d, 0d }, { 0d, 0d, 0d } };
	private bool isCameraInitialized;
	public Matrix4x4 leftPose;

	public double[] rvec;
	public double[] tvec;

	public GameObject cubeToMove;

	//To extract the HMD video frame from GPU to CPU
	AsyncGPUReadbackRequest request;
	private bool initialized;


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
		if (!isCameraInitialized && ViveSR_DualCameraImageCapture.FocalLengthLeft > 0)
		{
			cameraMatrix = new double[3, 3] {
				{ViveSR_DualCameraImageCapture.FocalLengthLeft, 0d, ViveSR_DualCameraImageCapture.UndistortedCxLeft},
				{0d, ViveSR_DualCameraImageCapture.FocalLengthLeft, ViveSR_DualCameraImageCapture.UndistortedCyLeft},
				{0d, 0d, 1d}
			};

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
			CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
			//CvAruco.DrawDetectedMarkers(flippedMat, corners, ids);

			if (ids.Length > 0)
			{
				for (int i = 0; i < ids.Length; i++)
				{
					if (ids[i] == 0)
					{
						Cv2.SolvePnP(markerPoints, corners[i], cameraMatrix, distCoeffs, out rvec, out tvec, false, SolvePnPFlags.Iterative);
						Cv2.Rodrigues(rvec, out rotMat);
						//rotMat = LHMatrixFromRHMatrix(rotMat);

						Vector3 forward = new Vector3((float) rotMat[2, 0], (float) rotMat[2, 1],(float) rotMat[2, 2]);
						Vector3 up = new Vector3((float)rotMat[1, 0], (float)rotMat[1, 1], (float)rotMat[1, 2]);

						//forward = leftPose.MultiplyVector(forward);
						//up = leftPose.MultiplyVector(up);

						Quaternion finalRot = Quaternion.LookRotation(forward, up);

						Vector3 cameraSpacePos = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); //x is negative due to flipping of camera

						Vector3 worldPos = leftPose.MultiplyPoint(cameraSpacePos);

						Quaternion cameraRot = QuaternionFromMatrix(rotMat);

						cameraRot.y = -cameraRot.y;
						//cameraRot.x = -cameraRot.x;
						cameraRot.z = -cameraRot.z;

						//float temp = cameraRot.x;
						//cameraRot.x = cameraRot.z;
						//cameraRot.z = temp;

						//cubeToMove.SetNewTransform(worldPos, finalRot);
						cubeToMove.transform.localPosition = cameraSpacePos;
						cubeToMove.transform.localRotation = cameraRot;
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

	private double[,] MultiplyMatrix(double[,] m1,double[,] m2)
    {
		double[,] res = new double[3, 3];
		for (int i = 0; i < 3; i++)
        {
			for (int j = 0; j < 3; j++)
            {
				double sum = 0;
				for (int k = 0; k < 3; k++)
                {
					sum += m1[i, k] * m2[k, j];
                }
				res[i, j] = sum;
            }
        }
		return res;
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

	public double[,] LHMatrixFromRHMatrix(double[,] rhm)
	{
		double[,] lhm = new double[3,3] ;

		// Column 0.
		lhm[0, 0] = rhm[0, 0];
		lhm[1, 0] = rhm[1, 0];
		lhm[2, 0] = -rhm[2, 0];

		// Column 1.
		lhm[0, 1] = rhm[0, 1];
		lhm[1, 1] = rhm[1, 1];
		lhm[2, 1] = -rhm[2, 1];

		// Column 2.
		lhm[0, 2] = -rhm[0, 2];
		lhm[1, 2] = -rhm[1, 2];
		lhm[2, 2] = rhm[2, 2];

		return lhm;
	}
}