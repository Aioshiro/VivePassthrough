using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FollowObject : NetworkBehaviour
{

    [SerializeField] Transform objectToFollow;

    private void Start()
    {
        objectToFollow = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0);
    }

    private void Update()
    {
        if (netIdentity.isLocalPlayer&& objectToFollow == null)
        {
            Start();
        }
        transform.position = objectToFollow.position;
        transform.rotation = objectToFollow.rotation;
    }
}
