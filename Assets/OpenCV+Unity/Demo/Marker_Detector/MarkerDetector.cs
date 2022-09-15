namespace OpenCvSharp.Demo {

	using UnityEngine;
	using System.Collections;
	using UnityEngine.UI;
	using Aruco;

	public class MarkerDetector : MonoBehaviour {

		private DetectorParameters detectorParameters;
		private Dictionary dictionary;
		private Point2f[][] corners;
		private int[] ids;
		private Point2f[][] rejectedImgPoints;
		public Renderer initialImage;
		public Renderer finalImage;

		void Start () {


			// Create default parameres for detection
			detectorParameters = DetectorParameters.Create();

			// Dictionary holds set of all available markers
			dictionary = CvAruco.GetPredefinedDictionary (PredefinedDictionaryName.Dict4X4_50);

		}

        private void Update()
        {
			Vive.Plugin.SR.ViveSR_DualCameraImageCapture.GetUndistortedTexture(out Texture2D imageLeft, out Texture2D imageRight, out _, out _, out _, out _);
			initialImage.material.mainTexture = imageLeft;

            // Create Opencv image from unity texture
            Mat mat = Unity.TextureToMat(imageLeft);

            // Convert image to grasyscale
            Mat grayMat = new Mat();
            Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);

            // Detect and draw markers
            CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
            CvAruco.DrawDetectedMarkers(mat, corners, ids);

            // Create Unity output texture with detected markers
            Texture2D outputTexture = Unity.MatToTexture(mat);

            // Set texture to see the result
            finalImage.material.mainTexture = outputTexture;
        }

    }
}