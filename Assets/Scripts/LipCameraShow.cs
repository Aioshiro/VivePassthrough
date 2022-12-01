using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;

/// <summary>
/// Debug script to show input of facial tracker camera data
/// </summary>
public class LipCameraShow : MonoBehaviour
{
    Texture2D lipTexture;

    void Start()
    {
        //lipTexture initialization, R8 texture format is necessary, as the data has only one channel
        lipTexture = new Texture2D(SRanipal_Lip_v2.ImageWidth,SRanipal_Lip_v2.ImageHeight,TextureFormat.R8,false);
        GetComponent<Renderer>().material.mainTexture = lipTexture;
    }

    void Update()
    {
        SRanipal_Lip_v2.GetLipImage(ref lipTexture);
    }
}
