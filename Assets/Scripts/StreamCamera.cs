using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamCamera : MonoBehaviour
{
    // Start is called before the first frame update
    private WebCamTexture webCamTexture;
    void Start()
    {
        webCamTexture = new WebCamTexture();
        GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
