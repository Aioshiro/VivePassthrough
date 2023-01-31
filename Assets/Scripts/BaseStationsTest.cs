using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStationsTest : MonoBehaviour
{

    /// <summary>
    /// List of stored positions of tracked devices
    /// </summary>
    Valve.VR.TrackedDevicePose_t[] stationsPose = new Valve.VR.TrackedDevicePose_t[10];

    /// <summary>
    /// GameObject (cube) representing the first station
    /// </summary>
    GameObject station1;

    /// <summary>
    /// GameObject (cube) representing the second station
    /// </summary>
    GameObject station2;

    //setting up variables
     void Start()
    {
        //creating cube of 10cm side to represent stations
        station1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        station2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        station1.transform.localScale /= 10;
        station2.transform.localScale /= 10;

    }

    // Update is called once per frame
    void Update()
    {
        bool firstStationChanged = false;
        // Updating the poses of all tracked devices
        Valve.VR.OpenVR.System.GetDeviceToAbsoluteTrackingPose(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding, 0, stationsPose);
        //For every tracked device
        for (uint i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            // If it's a base station
            if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(i) == Valve.VR.ETrackedDeviceClass.TrackingReference) 
            {
                //we get its position and rotation
                Vector3 baseStationPos = stationsPose[i].mDeviceToAbsoluteTracking.GetPosition();
                Quaternion baseStationRot = stationsPose[i].mDeviceToAbsoluteTracking.GetRotation();
                //we update the first station transform if it has not been done, otherwise we update the second one
                if (!firstStationChanged)
                {
                    station1.transform.SetPositionAndRotation(baseStationPos, baseStationRot);
                    firstStationChanged = true;

                }
                else
                {
                    station2.transform.SetPositionAndRotation(baseStationPos, baseStationRot);
                }
            }
        }
    }
}
