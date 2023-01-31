using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activating the canvas on the right canvas relative to the player
/// </summary>
public class ActivateRightCanvas : MonoBehaviour
{
    /// <summary>
    /// The player's transform
    /// </summary>
    [SerializeField]
    Transform playerRig;

    // Start is called before the first frame update
    void Start()
    {
        //We calculate on which side of the marker is situated the player
        Vector3 cross = Vector3.Cross(transform.forward, playerRig.forward); //cross is the cross product between the canvas and the player
        float dot = Vector3.Dot(cross, transform.up);
        if (dot < 0)
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

}
