using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

public class SetUpNext : MonoBehaviour
{

    private const float Distance = 0.6f;

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;
        SetMirroTransform();
    }

    private void SetMirroTransform()
    {
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * Distance;
        transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z);
        transform.LookAt(Camera.main.transform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
