using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStationsTest : MonoBehaviour
{

    Valve.VR.TrackedDevicePose_t[] stationsPose = new Valve.VR.TrackedDevicePose_t[10]; //Storing the poses
    GameObject station1;
    GameObject station2;

    // Start is called before the first frame update
    void Start()
    {
        station1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        station2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        station1.transform.localScale /= 10;
        station2.transform.localScale /= 10;

    }

    // Update is called once per frame
    void Update()
    {
        bool firstStationChanged = false;
        Valve.VR.OpenVR.System.GetDeviceToAbsoluteTrackingPose(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding, 0, stationsPose); //Updating the poses
        for (uint i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++) //For every tracked device
        {
            if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(i) == Valve.VR.ETrackedDeviceClass.TrackingReference) //If it's a base station
            {
                Vector3 baseStationPos = stationsPose[i].mDeviceToAbsoluteTracking.GetPosition();
                Quaternion baseStationRot = stationsPose[i].mDeviceToAbsoluteTracking.GetRotation();
                if (firstStationChanged)
                {
                    station2.transform.SetPositionAndRotation(baseStationPos, baseStationRot);
                }
                else
                {
                    firstStationChanged = true;
                    station1.transform.SetPositionAndRotation(baseStationPos, baseStationRot);
                }
            }
            //Get position and rotation of the station

        }
    }
}
