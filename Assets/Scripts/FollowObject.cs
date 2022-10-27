using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FollowObject : NetworkBehaviour
{

    [SerializeField] Transform objectToFollow;

    private void Update()
    {
        if (netIdentity.isLocalPlayer&& objectToFollow == null)
        {
            objectToFollow = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0);
        }
        transform.position = objectToFollow.position;
        transform.rotation = objectToFollow.rotation;
    }
}
