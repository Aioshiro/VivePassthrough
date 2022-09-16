using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSmoother : MonoBehaviour
{
    [Range(0,1)] [SerializeField]
    private float smootherCoefPos=1;

    [Range(0, 1)]
    [SerializeField]
    private float smootherCoefRot = 1;

    public void SetNewTransform(Vector3 pos, Quaternion rot)
    {
        transform.position = Vector3.Lerp(transform.position, pos, smootherCoefPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, smootherCoefRot);
    }
}
