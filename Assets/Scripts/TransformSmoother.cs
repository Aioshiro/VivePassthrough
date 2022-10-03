using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSmoother : MonoBehaviour
{
    private Vector3 movingAveragePos;
    private Vector3 movingAverageUp;
    private Vector3 movingAverageForward;
    private int count=0;
    [SerializeField] private int movingAverageLengthPos = 6;
    [SerializeField] private int movingAverageLengthRot = 3;

    [SerializeField] private float posStrength=1;
    //[SerializeField] private float rotStrength;

    Vector3 currentVelocity;

    [SerializeField] private bool allowMovement;

    private void Start()
    {
        currentVelocity = new Vector3();
    }

    public void SetNewTransform(Vector3 pos, Quaternion rot)
    {
        if (!allowMovement) { return; }
        count++;
        if (count> movingAverageLengthPos)
        {
            movingAveragePos += (pos - movingAveragePos) / (movingAverageLengthPos + 1);
            transform.position = Vector3.SmoothDamp(transform.position,movingAveragePos,ref currentVelocity,Time.deltaTime*posStrength);
        }
        else
        {
            movingAveragePos += pos;
            if (count == movingAverageLengthPos)
            {
                movingAveragePos /= count;
                transform.position = Vector3.SmoothDamp(transform.position, movingAveragePos, ref currentVelocity, Time.deltaTime * posStrength);
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
