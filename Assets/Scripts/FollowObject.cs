using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FollowObject : NetworkBehaviour
{

    [SerializeField] Transform objectToFollow;

    private void Update()
    {
        if (netIdentity.isLocalPlayer)
        {
            if (objectToFollow == null)
            {
                objectToFollow = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0);
            }
            else
            {
                UpdateTransform();
            }
        }
    }

    [Command]
    void UpdateTransform()
    {
        transform.position = objectToFollow.position;
        transform.rotation = objectToFollow.rotation;
    }
}
