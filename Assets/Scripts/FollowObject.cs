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
            UpdateTransform();
        }
    }

    [Command]
    void UpdateTransform()
    {
        transform.position = objectToFollow.position;
        transform.rotation = objectToFollow.rotation;
    }
}
