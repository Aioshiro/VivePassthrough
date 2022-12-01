using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Marker manager to gather data of markers positions
/// </summary>
public class MarkersManager : MonoBehaviour
{

    [SerializeField] TransformSmoother[] markers;

    /// <summary>
    /// Send data to the right's marker TransformSmoother
    /// </summary>
    /// <param name="i"> Marker's id</param>
    /// <param name="pos"> World position input </param>
    /// <param name="rot"> World rotation input</param>
    public void UpdateIthMarkerPos(int i,Vector3 pos, Quaternion rot)
    {
        if (i >= 0 && i < markers.Length) //Ensuring that we care about the marker, in case a wrong marker is detected
        {
            markers[i].SetNewTransform(pos, rot);
        }
        else
        {
            Debug.LogWarning("Out of range marker" + i.ToString());
        }
    }

    /// <summary>
    /// Set the ith marker as (un)active
    /// </summary>
    /// <param name="i"> Marker's id</param>
    /// <param name="setActive"> Should the marker be active or unactive ?</param>
    public void SetActiveIthMarker(int i,bool setActive)
    {
        markers[i].gameObject.SetActive(setActive);
    }

}
