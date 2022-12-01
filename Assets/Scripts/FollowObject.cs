using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Used for an object to follow the player's head
/// </summary>

[System.Obsolete("Not used since the two local clients don't have the same origin, see SyncHeads.cs",false)]
public class FollowObject : NetworkBehaviour
{

    [SerializeField] Transform objectToFollow = null;

    private void Start()
    {
        if (netIdentity.isLocalPlayer) { objectToFollow = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(2); }
    }

    private void Update()
    {
        if (netIdentity.isLocalPlayer)
        {
            UpdateTransformLocal();
        }
    }

    void UpdateTransformLocal()
    {
        transform.position = objectToFollow.position;
        transform.rotation = objectToFollow.rotation;
        //UpdateTransformServer(objectToFollow.position, objectToFollow.rotation);
    }

    [Command]
    void UpdateTransformServer(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        Debug.Log(pos);
    }
}
