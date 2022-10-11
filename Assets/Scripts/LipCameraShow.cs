using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;

public class LipCameraShow : MonoBehaviour
{
    Texture2D lipTexture;

    // Start is called before the first frame update
    void Start()
    {
        lipTexture = new Texture2D(SRanipal_Lip_v2.ImageWidth,SRanipal_Lip_v2.ImageHeight,TextureFormat.R8,false);
        GetComponent<Renderer>().material.mainTexture = lipTexture;
    }

    // Update is called once per frame
    void Update()
    {
        SRanipal_Lip_v2.GetLipImage(ref lipTexture);
    }
}
