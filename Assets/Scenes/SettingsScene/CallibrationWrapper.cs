using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using System;

public class CallibrationWrapper : MonoBehaviour
{
    public void StartCallibration()
    {
        SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
    }
}
