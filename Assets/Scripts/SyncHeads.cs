using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncHeads : NetworkBehaviour
{
    readonly SyncList<Vector3> playersHeadsLocalPositions = new SyncList<Vector3>();
    [SerializeField] List<GameObject> playersHeads;
    [SerializeField] Transform markerWorldOrigin;
    [SerializeField] Transform localRigTrackedLeftCamera;

    public override void OnStartServer()
    {
        base.OnStartServer();
        playersHeadsLocalPositions.Add(Vector3.zero);
        playersHeadsLocalPositions.Add(Vector3.zero);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        playersHeadsLocalPositions.Callback += OnListUpdated;
    }

    // Update is called once per frame
    void Update()
    {
        if (isClientOnly)
        {
            int playerNumber = GameManager.Instance.playerNumber;
            //First, upload own headPos
            UpdateListValue(playerNumber, markerWorldOrigin.transform.InverseTransformPoint(localRigTrackedLeftCamera.position));
            //Then, download other headPos
            playersHeads[(playerNumber + 1) % 2].transform.position = markerWorldOrigin.transform.TransformPoint(playersHeadsLocalPositions[(playerNumber + 1) % 2]);
        }
    }

    [Command]
    void UpdateListValue(int index, Vector3 newValue)
    {
        playersHeadsLocalPositions[index] = newValue;
    }

    void OnListUpdated(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem)
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
    
}
