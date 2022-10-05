using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkersManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] TransformSmoother[] markers;

    private void Start()
    {
        markers = GetComponentsInChildren<TransformSmoother>();
    }  

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
