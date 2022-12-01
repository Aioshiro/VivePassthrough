using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Smoothes new transform input
/// </summary>
public class TransformSmoother : MonoBehaviour
{
    /// <summary>
    /// Moving average of the position
    /// </summary>
    private Vector3 movingAveragePos;
    /// <summary>
    /// Moving average of the up direction vector
    /// </summary>
    private Vector3 movingAverageUp;
    /// <summary>
    /// Moving average of the forward direction vector
    /// </summary>
    private Vector3 movingAverageForward;
    /// <summary>
    /// Count of number of values updated
    /// </summary>
    public int count=0;
    [Tooltip("Number of samples to take for the average of the position")]
    [SerializeField] public int movingAverageLengthPos = 6;
    [Tooltip("Number of samples to take for the average of the rotation")]
    [SerializeField] private int movingAverageLengthRot = 3;

    [Tooltip("Max distance between frames to go to new position")]
    [SerializeField] private float posMaxDistance=0.01f;
    [Tooltip("Time factor for the max time to go to new rotation")]
    [SerializeField] private float rotMaxDegrees=1;

    [Tooltip("Uncheck to freeze position")]
    [SerializeField] private bool allowMovement;
    [Tooltip("Unceck to freeze object rotation")]
    [SerializeField] private bool allowRotation;

    [Tooltip("Left tracked camera")]
    [SerializeField] Transform leftCamera;

    [Tooltip("Was the marker pointing down ?")]
    private bool wasPointingDown = false;
    //public bool wasPointingRight = false;
    Quaternion rotDerivate;
    [Tooltip("Tolerance for danger zone")]
    [Range(0,1)]
    public float dangerZoneTolerance;

    [Tooltip("Should we stop updating position once we have finished gathering an average")]
    public bool stopOnAverageObtained=false;

    //public Material cubeColor;
    [Tooltip("Is X rotation allowed ?")]
    public bool xRotationAllowed = true;

    [Tooltip("Is Y rotation allowed ?")]
    public bool yRotationAllowed = true;

    [Tooltip("Is Z rotation allowed ?")]
    public bool zRotationAllowed = true;

    [Tooltip("Initial rotation values")]
    public Vector3 freezeRotValues;

    public void Update()
    {
        if (leftCamera == null)
        {
            leftCamera = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0);
        }
    }

    /// <summary>
    /// Set transform of object by smoothing inputs
    /// </summary>
    /// <param name="pos"> Position input</param>
    /// <param name="rot"> Rotation input </param>
    public void SetNewTransform(Vector3 pos, Quaternion rot)
    {
        if (!allowMovement || Vector3.SqrMagnitude(leftCamera.transform.position)<0.001f) { return; } // if the camera is at the origin, that means the head position is not tracked yet, and so it gives a false position which gives a false mean
        count++;
        if (count> movingAverageLengthPos) //If we have enough samples, we update the position
        {
            movingAveragePos += (pos - movingAveragePos) / (movingAverageLengthPos + 1); //new theoretical position
            transform.position = Vector3.MoveTowards(transform.position,movingAveragePos,posMaxDistance); //Go to new position with some dampning
            //Debug.Log("new pos is " + transform.position.ToString());
        }
        else 
        {
            transform.position = pos;
            movingAveragePos += pos; //we register the new sample
            if (count == movingAverageLengthPos)
            {
                if (stopOnAverageObtained)
                {
                    allowMovement = false;
                }
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

    /// <summary>
    /// Applying rotation freezes
    /// </summary>
    private void ApplyFreezes()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        if (!xRotationAllowed)
        {
            euler.x = freezeRotValues.x;
        }
        if (!yRotationAllowed)
        {
            euler.y = freezeRotValues.y;
        }
        if (!zRotationAllowed)
        {
            euler.z = freezeRotValues.z;
        }

        transform.rotation = Quaternion.Euler(euler);
    }

}
