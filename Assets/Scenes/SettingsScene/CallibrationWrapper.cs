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
    /// <summary>
    /// Function to call to start eye callibration
    /// </summary>
    public void StartCallibration()
    {
        SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
    }
}
