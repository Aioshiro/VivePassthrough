using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeGazingRocketbox : MonoBehaviour
{
    [Tooltip("Object to look at")]
    [SerializeField] private Transform objectToTrack;

    private SkinnedMeshRenderer meshRenderer;
    Mesh skinnedMesh;

    [Tooltip("Model left eye transform")]
    [SerializeField] Transform leftEye;
    [Tooltip("Model right eye transform")]
    [SerializeField] Transform rightEye;

    //Bunch of int corresponding to the good blendshapes
    int eyeLookDownLeft;
    int eyeLookInLeft;
    int eyeLookOutLeft;
    int eyeLookUpLeft;

    int eyeLookDownRight;
    int eyeLookInRight;
    int eyeLookOutRight;
    int eyeLookUpRight;

    int eyesClosed;

    const float timeToBlink = 0.05f;
    const float timeBetweenBlinks = 3.5f;
    float timeUntilNextBlink = 0;


    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        skinnedMesh = meshRenderer.sharedMesh;
        eyeLookDownLeft = skinnedMesh.GetBlendShapeIndex("AK_11_EyeLookDownLeft");
        eyeLookInLeft = skinnedMesh.GetBlendShapeIndex("AK_13_EyeLookInLeft");
        eyeLookOutLeft = skinnedMesh.GetBlendShapeIndex("AK_15_EyeLookOutLeft");
        eyeLookUpLeft = skinnedMesh.GetBlendShapeIndex("AK_17_EyeLookUpLeft");

        eyeLookDownRight = skinnedMesh.GetBlendShapeIndex("AK_12_EyeLookDownRight");
        eyeLookInRight = skinnedMesh.GetBlendShapeIndex("AK_14_EyeLookInRight");
        eyeLookOutRight = skinnedMesh.GetBlendShapeIndex("AK_16_EyeLookOutRight");
        eyeLookUpRight = skinnedMesh.GetBlendShapeIndex("AK_18_EyeLookUpRight");

        eyesClosed = skinnedMesh.GetBlendShapeIndex("AU_45_Blink");

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 directionLeft = leftEye.InverseTransformPoint(objectToTrack.transform.position).normalized * 100;
        directionLeft = -directionLeft;
        if (directionLeft.x > 0)
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownLeft, Mathf.Clamp(-directionLeft.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookUpLeft, Mathf.Clamp(directionLeft.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookInLeft, Mathf.Clamp(directionLeft.z, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookOutLeft, Mathf.Clamp(-directionLeft.z, 0, 100));
        }
        else
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownLeft, 0);
            meshRenderer.SetBlendShapeWeight(eyeLookUpLeft, 0);
            meshRenderer.SetBlendShapeWeight(eyeLookInLeft, 0);
            meshRenderer.SetBlendShapeWeight(eyeLookOutLeft, 0);
        }
        Vector3 directionRight = rightEye.InverseTransformPoint(objectToTrack.transform.position).normalized * 100;
        directionRight = -directionRight;
        if (directionRight.x > 0)
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownRight, Mathf.Clamp(-directionRight.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookUpRight, Mathf.Clamp(directionRight.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookInRight, Mathf.Clamp(-directionRight.z, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookOutRight, Mathf.Clamp(directionRight.z, 0, 100));
        }
        else
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownRight, 0);
            meshRenderer.SetBlendShapeWeight(eyeLookUpRight, 0);
            meshRenderer.SetBlendShapeWeight(eyeLookInRight, 0);
            meshRenderer.SetBlendShapeWeight(eyeLookOutRight, 0);
        }

        timeUntilNextBlink += Time.deltaTime;
        if (timeUntilNextBlink > timeBetweenBlinks)
        {
            timeUntilNextBlink = 0;
            StartCoroutine(nameof(BlinkingCoroutine));
        }

    }

    IEnumerator BlinkingCoroutine()
    {
        float t = 0;
        while (t < timeToBlink)
        {
            t += Time.deltaTime;
            meshRenderer.SetBlendShapeWeight(eyesClosed, t * 100 / timeToBlink);
            yield return new WaitForFixedUpdate();
        }
        t = timeToBlink;
        while (t > 0)
        {
            t -= Time.deltaTime;
            meshRenderer.SetBlendShapeWeight(eyesClosed, t * 100 / timeToBlink);
            yield return new WaitForFixedUpdate();
        }
    }
}
