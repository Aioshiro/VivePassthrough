using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveHandTracking;

public class SphereOccluder : MonoBehaviour
{
    [Tooltip("Draw left hand if true, right hand otherwise")]
    public bool IsLeft = false;
    [Tooltip("Root object of skinned mesh")]
    public GameObject Hand = null;


    void Awake()
    {
        Hand.SetActive(false);
    }

    IEnumerator Start()
    {
        while (GestureProvider.Status == GestureStatus.NotStarted) yield return null;
        if (!GestureProvider.HaveSkeleton)
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        GestureResult result = IsLeft ? GestureProvider.LeftHand : GestureProvider.RightHand;
        if (result == null)
        {
            Hand.SetActive(false);
            return;
        }
        Hand.SetActive(true);

        transform.localPosition = result.position;
        transform.localRotation = result.rotation;
    }
}

