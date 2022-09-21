using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSmoother : MonoBehaviour
{
    private Vector3 movingAveragePos;
    private Vector3 movingAverageUp;
    private Vector3 movingAverageForward;
    private int count=0;
    [SerializeField] private int movingAverageLengthPos = 10;
    [SerializeField] private int movingAverageLengthRot = 10;


    public void SetNewTransform(Vector3 pos, Quaternion rot)
    {
        count++;
        //transform.rotation = Quaternion.Slerp(transform.rotation, rot, smootherCoefRot);
        if (count> movingAverageLengthPos)
        {
            movingAveragePos += (pos - movingAveragePos) / (movingAverageLengthPos + 1);
            transform.position = movingAveragePos;
        }
        else
        {
            movingAveragePos += pos;
            if (count == movingAverageLengthPos)
            {
                movingAveragePos /= count;
                transform.position = movingAveragePos;
            }
        }
        if (count > movingAverageLengthRot)
        {
            movingAverageUp += (rot * Vector3.up - movingAverageUp) / (movingAverageLengthRot + 1);
            movingAverageForward += (rot * Vector3.forward - movingAverageForward) / (movingAverageLengthRot + 1);
            transform.rotation = Quaternion.LookRotation(movingAverageForward, movingAverageUp);
        }
        else
        {
            movingAverageForward += rot * Vector3.forward;
            movingAverageUp += rot * Vector3.up;
            if (count == movingAverageLengthRot)
            {
                movingAverageUp /= count;
                movingAverageForward /= count;
                transform.rotation = Quaternion.LookRotation(movingAverageForward, movingAverageUp);
            }
        }
    }

}
