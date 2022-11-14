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

    Vector3 initialScale;
    bool rescaleOccluder = true;

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
        initialScale = Hand.transform.localScale;
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

        if (!rescaleOccluder) { return; }
        float handLength = (result.points[12] - result.position).magnitude; //distance between palm and end of major
        Hand.transform.localScale = new Vector3(initialScale.x,Mathf.Clamp(initialScale.y*handLength/0.13f,0.1f,initialScale.y),initialScale.z);
    }
}

