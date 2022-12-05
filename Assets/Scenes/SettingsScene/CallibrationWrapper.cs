using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using System;



/// <summary>
/// Wrapper script to start eye callibration
/// </summary>
public class CallibrationWrapper : MonoBehaviour
{

    public bool skipCallibration = false;

    public void ToggleSkipCallibration(bool skip)
    {
        skipCallibration = skip;
    }

    /// <summary>
    /// Function to call to start eye callibration
    /// </summary>
    public void StartCallibration()
    {
        if (skipCallibration) { return; }
        SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
    }
}
