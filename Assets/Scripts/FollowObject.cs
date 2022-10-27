using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FollowObject : NetworkBehaviour
{

    [SerializeField] Transform objectToFollow = null;

    private void Start()
    {
        if (netIdentity.isLocalPlayer) { objectToFollow = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0); }
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
        UpdateTransformServer(objectToFollow.position, objectToFollow.rotation);
    }

    [Command]
    void UpdateTransformServer(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        Debug.Log(pos);
    }
}
