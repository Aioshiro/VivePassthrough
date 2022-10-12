using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MarkersManager : MonoBehaviour
{

    [SerializeField] TransformSmoother[] markers;


    public void UpdateIthMarkerPos(int i,Vector3 pos, Quaternion rot)
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
