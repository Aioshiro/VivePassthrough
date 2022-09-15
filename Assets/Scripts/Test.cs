using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    private new Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vive.Plugin.SR.ViveSR_DualCameraImageCapture.GetUndistortedTexture(out Texture2D imageLeft, out Texture2D imageRight, out _, out _, out _, out _);
        renderer.material.mainTexture = imageLeft;
    }
}
