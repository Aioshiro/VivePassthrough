using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSmoother : MonoBehaviour
{
    private Vector3 movingAveragePos;
    private Vector3 movingAverageUp;
    private Vector3 movingAverageForward;
    private int count=0;
    [Tooltip("Number of samples to take for the average of the position")]
    [SerializeField] private int movingAverageLengthPos = 6;
    [Tooltip("Number of samples to take for the average of the rotation")]
    [SerializeField] private int movingAverageLengthRot = 3;

    [Tooltip("Max distance between frames to go to new position")]
    [SerializeField] private float posMaxDistance=0.01f;
    [Tooltip("Time factor for the max time to go to new rotation")]
    [SerializeField] private float rotStrength=1;

    Vector3 currentVelocity;
    Quaternion currentRotVelocity;

    [Tooltip("Uncheck to freeze object")]
    [SerializeField] private bool allowMovement;

    private void Start()
    {
        currentVelocity = new Vector3();
    }
    public void SetNewTransform(Vector3 pos, Quaternion rot)
    {
        if (!allowMovement) { return; }
        count++;
        if (count> movingAverageLengthPos) //If we have enough samples, we update the position
        {
            movingAveragePos += (pos - movingAveragePos) / (movingAverageLengthPos + 1); //new theoretical position
            transform.position = Vector3.MoveTowards(transform.position,movingAveragePos,posMaxDistance); //Go to new position with some dampning
        }
        else 
        {
            movingAveragePos += pos; //we register the new sample
            if (count == movingAverageLengthPos) // and update immediatlly if we have enough
            {
                movingAveragePos /= count;
                transform.position = Vector3.SmoothDamp(transform.position, movingAveragePos, ref currentVelocity, Time.deltaTime * posMaxDistance);
            }
        }

        //For the rotation, we do the average of the Up and Forward vectors of the rotation, as it's a bit complicated to do averages with quaternions
        // Though, as we move from and to very close rotations, average are possible

        if (count > movingAverageLengthRot) //If we have enough samples, we update the rotation
        {
            movingAverageUp += (rot * Vector3.up - movingAverageUp) / (movingAverageLengthRot + 1);//new theoretical up vector
            movingAverageForward += (rot * Vector3.forward - movingAverageForward) / (movingAverageLengthRot + 1); //new theoretical forward vector
            transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation,Quaternion.LookRotation(movingAverageForward, movingAverageUp),ref currentRotVelocity,rotStrength); //Go to new rotation with some dampning
        }
        else
        {
            movingAverageForward += rot * Vector3.forward;
            movingAverageUp += rot * Vector3.up;
            if (count == movingAverageLengthRot)
            {
                movingAverageUp /= count;
                movingAverageForward /= count;
                transform.rotation =QuaternionUtil.SmoothDamp(transform.rotation, Quaternion.LookRotation(movingAverageForward, movingAverageUp),ref currentRotVelocity,rotStrength);
            }
        }
    }

}
