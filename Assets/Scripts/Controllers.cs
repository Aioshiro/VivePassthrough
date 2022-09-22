using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllers : MonoBehaviour
{
    private Transform left;
    private Transform right;
    // Start is called before the first frame update
    void Start()
    {
        left = transform.GetChild(0);
        right = transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        Vive.Plugin.SR.ControllPose.SRWork_Controll_Pose.UpdateData();
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
