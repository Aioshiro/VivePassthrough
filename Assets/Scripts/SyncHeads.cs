﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Sync the head positions and rotations of the players on the server.
/// <para>It uploads the local position and rotation in the marker 10 space, so both client have same origin</para>
/// </summary>
public class SyncHeads : NetworkBehaviour
{
    [Tooltip("List of players heads local position")]
    readonly SyncList<Vector3> playersHeadsLocalPositions = new SyncList<Vector3>();

    [Tooltip("List of players heads local rotation")]
    readonly SyncList<Quaternion> playersHeadsLocalRotations = new SyncList<Quaternion>();

    [Tooltip("Other player head")]
    [SerializeField] GameObject otherPlayerHead;

    //[Tooltip("Marker 10 transform")]
    //[SerializeField] TransformSmoother markerWorldOrigin;

    private GameObject baseStationOrigin;


    Valve.VR.TrackedDevicePose_t[] stationsPose = new Valve.VR.TrackedDevicePose_t[5]; //Storing the poses


    [Tooltip("Local rig tracked camera")]
    [SerializeField] Transform localRigTrackedCamera;

    string trackingReferenceSerialNumber=null;
    System.Text.StringBuilder stringB = new System.Text.StringBuilder();
    Valve.VR.ETrackedPropertyError perror = new Valve.VR.ETrackedPropertyError();

    /// <summary>
    /// On server start, initialize SyncLists
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        playersHeadsLocalPositions.Add(Vector3.zero);
        playersHeadsLocalPositions.Add(Vector3.zero);
        playersHeadsLocalRotations.Add(Quaternion.identity);
        playersHeadsLocalRotations.Add(Quaternion.identity);
    }

    /// <summary>
    /// On start client, register callbacks
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        playersHeadsLocalPositions.Callback += OnPosUpdated;
        playersHeadsLocalRotations.Callback += OnRotUpdated;
        baseStationOrigin = new GameObject("Base station");
    }

    // Update is called once per frame
    void Update()
    {
        if (localRigTrackedCamera == null)
        {
            localRigTrackedCamera = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(2);
        }
        if (isClientOnly)
        {
            int playerNumber = GameManager.Instance.playerNumber;
            if (trackingReferenceSerialNumber==null)
            {
                for (uint i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++) //For every tracked device
                {
                    if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(i) == Valve.VR.ETrackedDeviceClass.TrackingReference) //If it's a base station
                    {
                        System.Text.StringBuilder stringB = new System.Text.StringBuilder();
                        Valve.VR.ETrackedPropertyError perror = new Valve.VR.ETrackedPropertyError();
                        Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(i, Valve.VR.ETrackedDeviceProperty.Prop_SerialNumber_String, stringB, 100, ref perror);
                        SendSerialNumberTracking(stringB.ToString());
                        trackingReferenceSerialNumber = stringB.ToString();
                        return;
                    }
                }
            }

            Valve.VR.OpenVR.System.GetDeviceToAbsoluteTrackingPose(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding, 0, stationsPose); //Updating the poses
            uint chosenStation = 0;
            for (uint i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++) //For every tracked device (in case devices number change)
            {
                if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(i) == Valve.VR.ETrackedDeviceClass.TrackingReference) //If it's a base station
                {
                    Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(i, Valve.VR.ETrackedDeviceProperty.Prop_SerialNumber_String, stringB, 100, ref perror);
                    if (stringB.ToString() == trackingReferenceSerialNumber)
                    {
                        chosenStation = i;
                        break;
                    }
                }
            }
            //Get position and rotation of the station
            Vector3 baseStationPos = stationsPose[chosenStation].mDeviceToAbsoluteTracking.GetPosition();
            Quaternion baseStationRot = stationsPose[chosenStation].mDeviceToAbsoluteTracking.GetRotation();

            baseStationOrigin.transform.SetPositionAndRotation(baseStationPos, baseStationRot);

            //First, upload own headPos and headRot
            UpdatePosValue(playerNumber, baseStationOrigin.transform.InverseTransformPoint(localRigTrackedCamera.position));
            UpdateRotValue(playerNumber, Quaternion.Inverse(baseStationOrigin.transform.rotation) * localRigTrackedCamera.transform.rotation);
            //Then, download other headPos and headRot
            otherPlayerHead.transform.SetPositionAndRotation(baseStationOrigin.transform.TransformPoint(playersHeadsLocalPositions[(playerNumber + 1) % 2]), baseStationOrigin.transform.rotation * playersHeadsLocalRotations[(playerNumber + 1) % 2]);
        }
}

    [Command(requiresAuthority =false)]
    void SendSerialNumberTracking(string name)
    {
        trackingReferenceSerialNumber = name;
        Debug.Log($"Setting {name} as tracking station");
        SetSerialNumberTracking(name);
    }

    [ClientRpc]
    void SetSerialNumberTracking(string name)
    {
        trackingReferenceSerialNumber = name;
        Debug.Log($"Recieved {name} as tracking station");
    }

    /// <summary>
    /// Upload local position on server
    /// </summary>
    /// <param name="index"> Player number</param>
    /// <param name="newValue"> New local position value</param>
    [Command(requiresAuthority =false)]
    void UpdatePosValue(int index, Vector3 newValue)
    {
        playersHeadsLocalPositions[index] = newValue;
    }

    /// <summary>
    /// Upload local rotation on server
    /// </summary>
    /// <param name="index"> Player number</param>
    /// <param name="newValue"> New local rotation value</param>
    [Command(requiresAuthority = false)]
    void UpdateRotValue(int index, Quaternion newValue)
    {
        playersHeadsLocalRotations[index] = newValue;
    }


    /// <summary>
    /// Callback when position is updated
    /// </summary>
    /// <param name="op"></param>
    /// <param name="index"></param>
    /// <param name="oldItem"></param>
    /// <param name="newItem"></param>
    void OnPosUpdated(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem)
    {
        switch (op)
        {
            case SyncList<Vector3>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                break;
            case SyncList<Vector3>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<Vector3>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<Vector3>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<Vector3>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }

    /// <summary>
    /// Callback when rotation is updated
    /// </summary>
    /// <param name="op"></param>
    /// <param name="index"></param>
    /// <param name="oldItem"></param>
    /// <param name="newItem"></param>
    void OnRotUpdated(SyncList<Quaternion>.Operation op, int index, Quaternion oldItem, Quaternion newItem)
    {
        switch (op)
        {
            case SyncList<Quaternion>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                break;
            case SyncList<Quaternion>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<Quaternion>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<Quaternion>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<Quaternion>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }

}
