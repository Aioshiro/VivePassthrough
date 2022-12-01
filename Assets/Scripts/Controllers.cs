using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to update controllers position
/// </summary>
[System.Obsolete("We do not use controllers",false)]
public class Controllers : MonoBehaviour
{
    /// <summary>
    /// Left controller transform
    /// </summary>
    private Transform left;
    /// <summary>
    /// Right controller transform
    /// </summary>
    private Transform right;

    void Start()
    {
        //Setting up variables
        left = transform.GetChild(0);
        right = transform.GetChild(1);
    }

    void Update()
    {
        //Manually updating controller poses
        Vive.Plugin.SR.ControllPose.SRWork_Controll_Pose.UpdateData();

        //Updating the transform of each controller
        float[] posLeft = Vive.Plugin.SR.ControllPose.SRWork_Controll_Pose.pos_left;
        left.position = new Vector3(posLeft[0], posLeft[1], posLeft[2]);
        float[] posRight = Vive.Plugin.SR.ControllPose.SRWork_Controll_Pose.pos_right;
        right.position = new Vector3(posRight[0], posRight[1], posRight[2]);
        float[] rotLeft = Vive.Plugin.SR.ControllPose.SRWork_Controll_Pose.rot_left;
        left.rotation = new Quaternion(rotLeft[0], rotLeft[1], rotLeft[2], rotLeft[3]);
        float[] rotRight = Vive.Plugin.SR.ControllPose.SRWork_Controll_Pose.rot_right;
        right.rotation = new Quaternion(rotRight[0], rotRight[1], rotRight[2], rotRight[3]);
    }
}
