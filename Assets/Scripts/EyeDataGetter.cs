using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;
using Unity.Jobs;


/// <summary>
/// Gather eye data from local eye tracker and from server data (other player).
/// Data is updated with SyncGaze script
/// </summary>
public class EyeDataGetter : MonoBehaviour
{
    [Tooltip("Gaze sensibility. The bigger factor is, the more sensitive the gaze ray is.")]
    [Range(0, 1)]
    public float gazeSensibility;

    [Tooltip("Should the data be updated ?")]
    public bool NeededToGetData = true;

    [Tooltip("Own (local) player eye weightings data")]
    public static Dictionary<EyeShape_v2, float> ownEyeWeightings = new Dictionary<EyeShape_v2, float>();

    [Tooltip("Other player eye weightings data")]
    public static Dictionary<EyeShape_v2, float> otherEyeWeightings = new Dictionary<EyeShape_v2, float>();

    [Tooltip("Own (local) player local gaze direction")]
    public static Vector3 ownGazeDirectionLocal;

    [Tooltip("Other player local gaze direction")]
    public static Vector3 otherGazeDirectionLocal;

    /// <summary>
    /// Own raw eye data
    /// </summary>
    public static EyeData_v2 ownEyeData = new EyeData_v2();

    [Tooltip("True if player one is missing frames")]
    static public bool playerOneMissingFrames;

    [Tooltip("True if player two is missing frames")]
    static public bool playerTwoMissingFrames;

    /// <summary>
    /// Is the eye called been registered ?
    /// </summary>
    private static bool eye_callback_registered = false;

    private void Start()
    {
        //setting up gaze sensibility

        EyeParameter StartingEyeParameter = new EyeParameter
        {
            gaze_ray_parameter = new GazeRayParameter
            {
                sensitive_factor = gazeSensibility
            }
        };

        SRanipal_Eye_API.SetEyeParameter(StartingEyeParameter);

        //Initializing dictionnaries
        for (int i= (int)EyeShape_v2.None; i<=(int)EyeShape_v2.Max; i++)
        {
            ownEyeWeightings.Add((EyeShape_v2)i, 0);
            otherEyeWeightings.Add((EyeShape_v2)i, 0);

        }
        otherEyeWeightings[EyeShape_v2.Eye_Left_Left] = 1;
    }



    private void Update()
    {
        //just making sure the callback is there
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback && !eye_callback_registered)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out _, out ownGazeDirectionLocal, ownEyeData)) { }
        else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out _, out ownGazeDirectionLocal, ownEyeData)) { }
        else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out _, out ownGazeDirectionLocal, ownEyeData)) { }

        //SRanipal_Eye_API.GetEyeData_v2(ref ownEyeData);
        SRanipal_Eye_v2.GetEyeWeightings(out ownEyeWeightings, ownEyeData);

    }

    /// <summary>
    /// Eye callback, called at 90Hz (I believe)
    /// </summary>
    /// <param name="eye_data"> Raw eye data from tracker</param>
    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        //getting eyedata and updating weighting (blendshapes) values
        ownEyeData = eye_data;
    }

}
