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
	public Matrix4x4 leftPose;

	public GameObject cubeToMove;

	//To extract the HMD video frame from GPU to CPU
	AsyncGPUReadbackRequest request;
	private bool initialized;


	void Start()
	{

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
		Debug.Log("Left focal is " + ViveSR_DualCameraImageCapture.FocalLengthLeft.ToString());
		Debug.Log("Cx is " + ViveSR_DualCameraImageCapture.UndistortedCx_L.ToString());
		Debug.Log("Cy is " + ViveSR_DualCameraImageCapture.UndistortedCy_L.ToString());

		if (!initialized)
		{
			InitTextures();
			initialized = true;
		}

		if (initialized && request.done)
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