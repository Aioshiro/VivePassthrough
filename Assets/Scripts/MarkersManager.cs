using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MarkersManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] TransformSmoother[] markers;

    public void UpdateIthMarkerPos(int i,Vector3 pos, Quaternion rot)
    {
        if (i >= 0 && i < markers.Length)
        {
            markers[i].SetNewTransform(pos, rot);
        }
        else
        {
            Debug.LogWarning("Out of range marker");
        }
    }

}
