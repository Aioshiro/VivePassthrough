using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncHeads : NetworkBehaviour
{
    readonly SyncList<Vector3> playersHeadsLocalPositions = new SyncList<Vector3>();
    readonly SyncList<Quaternion> playersHeadsLocalRotations = new SyncList<Quaternion>();
    [SerializeField] List<GameObject> playersHeads;
    [SerializeField] Transform markerWorldOrigin;
    [SerializeField] Transform localRigTrackedCamera;

    public override void OnStartServer()
    {
        base.OnStartServer();
        playersHeadsLocalPositions.Add(Vector3.zero);
        playersHeadsLocalPositions.Add(Vector3.zero);
        playersHeadsLocalRotations.Add(Quaternion.identity);
        playersHeadsLocalRotations.Add(Quaternion.identity);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        playersHeadsLocalPositions.Callback += OnPosUpdated;
        playersHeadsLocalRotations.Callback += OnRotUpdated;
    }

    // Update is called once per frame
    void Update()
    {
        if (isClientOnly)
        {
            int playerNumber = GameManager.Instance.playerNumber;
            //First, upload own headPos and headRot
            UpdatePosValue(playerNumber, markerWorldOrigin.transform.InverseTransformPoint(localRigTrackedCamera.position));
            UpdateRotValue(playerNumber, localRigTrackedCamera.transform.rotation * markerWorldOrigin.transform.rotation);
            //Then, download other headPos and headRot
            playersHeads[(playerNumber + 1) % 2].transform.position = markerWorldOrigin.transform.TransformPoint(playersHeadsLocalPositions[(playerNumber + 1) % 2]);
            playersHeads[(playerNumber + 1) % 2].transform.rotation = markerWorldOrigin.transform.rotation*playersHeadsLocalRotations[(playerNumber + 1) % 2];
        }
    }

    [Command(requiresAuthority =false)]
    void UpdatePosValue(int index, Vector3 newValue)
    {
        playersHeadsLocalPositions[index] = newValue;
    }

    [Command(requiresAuthority = false)]
    void UpdateRotValue(int index, Quaternion newValue)
    {
        playersHeadsLocalRotations[index] = newValue;
    }

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
