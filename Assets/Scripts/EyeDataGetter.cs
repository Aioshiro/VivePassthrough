using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;
using Unity.Jobs;

public class EyeDataGetter : MonoBehaviour
{
    [Range(0, 1)]
    public float gazeSensibility;

    public bool NeededToGetData = true;
    public static Dictionary<EyeShape_v2, float> ownEyeWeightings = new Dictionary<EyeShape_v2, float>();
    public static Dictionary<EyeShape_v2, float> otherEyeWeightings = new Dictionary<EyeShape_v2, float>();

    public static EyeData_v2 ownEyeData = new EyeData_v2();

    static public bool playerOneMissingFrames;
    static public bool playerTwoMissingFrames;
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
        //SRanipal_Eye_API.GetEyeData_v2(ref ownEyeData);
        SRanipal_Eye_v2.GetEyeWeightings(out ownEyeWeightings, ownEyeData);
        foreach (var key in ownEyeWeightings.Keys)
        {
            Debug.Log(key);
            Debug.Log($"Own value of {key} is {ownEyeWeightings[key]}");
            Debug.Log($"other value of {key} is {otherEyeWeightings[key]}");

        }

    }
    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        //getting eyedata and updating weighting (blendshapes) values
        ownEyeData = eye_data;
    }

}
