using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mirror class to test feed back of own avatar in solo
/// </summary>
public class Miror : MonoBehaviour
{
    private const float Distance = 0.6f;
    [SerializeField] private Camera cameraToMirror;

    private void Update()
    {
        SetMirroTransform();
    }

    private void SetMirroTransform()
    {
        transform.position = cameraToMirror.transform.position + cameraToMirror.transform.forward * Distance;
        transform.position = new Vector3(transform.position.x, cameraToMirror.transform.position.y, transform.position.z);
        transform.LookAt(cameraToMirror.transform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
