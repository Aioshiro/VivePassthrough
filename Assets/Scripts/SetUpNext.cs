using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

/// <summary>
/// Sets up and object in front of the main camera
/// </summary>
public class SetUpNext : MonoBehaviour
{
    Camera mainCamera;
    private const float Distance = 0.6f;

    private void Update()
    {
        SetMirrorTransform();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Sets and object in front of the main camera
    /// </summary>
    private void SetMirrorTransform()
    {
        transform.position = mainCamera.transform.position + mainCamera.transform.forward * Distance;
        transform.position = new Vector3(transform.position.x, mainCamera.transform.position.y, transform.position.z);
        transform.LookAt(mainCamera.transform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
