using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Marker manager to gather data of markers positions, multi version to sync transform on server
/// </summary>
public class MarkersManagerMulti : NetworkBehaviour
{

    [SerializeField] TransformSmoother[] markers;

    /// <summary>
    /// Send data to the right's marker TransformSmoother on server
    /// </summary>
    /// <param name="i"> Marker's id</param>
    /// <param name="pos"> World position input </param>
    /// <param name="rot"> World rotation input</param>
    [Command(requiresAuthority =false)]
    public void UpdateIthMarkerPos(int i, Vector3 pos, Quaternion rot)
    {
        if (i >= 0 && i < markers.Length) //Ensuring that we care about the marker, in case a wrong marker is detected
        {
            markers[i].SetNewTransform(pos, rot);
        }
        else
        {
            Debug.LogWarning("Out of range marker");
        }
    }

}
