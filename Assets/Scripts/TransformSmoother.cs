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
    [SerializeField] private float rotMaxDegrees=1;

    [Tooltip("Uncheck to freeze object")]
    [SerializeField] private bool allowMovement;

    [SerializeField] private bool allowRotation;

    [SerializeField] Transform leftCamera;

    public bool wasPointingDown = false;
    //public bool wasPointingRight = false;
    Quaternion rotDerivate;
    public float dangerZoneTolerance;

    public bool stopOnAverageObtained=false;

    //public Material cubeColor;

    public bool xRotationAllowed = true;
    public bool yRotationAllowed = true;
    public bool zRotationAllowed = true;

    public void SetNewTransform(Vector3 pos, Quaternion rot)
    {
        if (!allowMovement) { return; }
        count++;
        if (count> movingAverageLengthPos) //If we have enough samples, we update the position
        {
            movingAveragePos += (pos - movingAveragePos) / (movingAverageLengthPos + 1); //new theoretical position
            transform.position = Vector3.MoveTowards(transform.position,movingAveragePos,posMaxDistance); //Go to new position with some dampning
            //Debug.Log("new pos is " + transform.position.ToString());
            if (stopOnAverageObtained)
            {
                allowMovement = false;
                Debug.Log("stoppping");
            }
        }
        else 
        {
            transform.position = pos;
            movingAveragePos += pos; //we register the new sample
            if (count == movingAverageLengthPos)
            {
                movingAveragePos /= count;
                transform.position = Vector3.MoveTowards(transform.position, movingAveragePos, posMaxDistance);
            }
            //Debug.Log("new pos is " + transform.position.ToString());
        }

        //For the rotation, we do the average of the Up and Forward vectors of the rotation, as it's a bit complicated to do averages with quaternions
        // Though, as we move from and to very close rotations, average are possible

        Vector3 markerToCamera = (leftCamera.position - pos).normalized;
        Vector3 crossForward = Vector3.Cross(markerToCamera, rot * Vector3.forward);
        if (Vector3.Dot(markerToCamera,rot*Vector3.forward)>dangerZoneTolerance)
        {
            //cubeColor.color = Color.red;
            bool isCurrentlyPointingDown = Vector3.Dot(crossForward, rot * Vector3.right) > 0;

            if ((isCurrentlyPointingDown&& !wasPointingDown) || (!isCurrentlyPointingDown&&wasPointingDown))
            {
                
                //Debug.Log("problème");
                //Vector3 newForward = rot * Vector3.forward;
                //newForward.y = -newForward.y;
                //rot = Quaternion.LookRotation(newForward, rot * Vector3.up);
                rot = QuaternionUtil.EstimateNewRot(transform.rotation, rotDerivate, Time.deltaTime);
                //return;
            }
        }
        else
        {
            //cubeColor.color = Color.green;
            wasPointingDown = Vector3.Dot(crossForward, rot * Vector3.right) > 0;
            // wasPointingRight = Vector3.Dot(crossForward, rot * Vector3.up) > 0;
            rotDerivate = QuaternionUtil.DerivateQuaternion(transform.rotation, rot, Time.deltaTime);
        }

        if (count > movingAverageLengthRot) //If we have enough samples, we update the rotation
        {
            movingAverageUp += (rot * Vector3.up - movingAverageUp) / (movingAverageLengthRot + 1);//new theoretical up vector
            movingAverageForward += (rot * Vector3.forward - movingAverageForward) / (movingAverageLengthRot + 1); //new theoretical forward vector
            transform.rotation = Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(movingAverageForward, movingAverageUp),rotMaxDegrees); //Go to new rotation with some dampning
        }
        else
        {
            movingAverageForward += rot * Vector3.forward;
            movingAverageUp += rot * Vector3.up;
            transform.rotation = rot;
            if (count == movingAverageLengthRot)
            {
                movingAverageUp /= count;
                movingAverageForward /= count;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(movingAverageForward, movingAverageUp),rotMaxDegrees);
            }
        }
        ApplyFreezes();
    }

    private void ApplyFreezes()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        if (!xRotationAllowed)
        {
            euler.x = 0;
        }
        if (!yRotationAllowed)
        {
            euler.y = 0;
        }
        if (!zRotationAllowed)
        {
            euler.z = 0;
        }

        transform.rotation = Quaternion.Euler(euler);
    }

}
