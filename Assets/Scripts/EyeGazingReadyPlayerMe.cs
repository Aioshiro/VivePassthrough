using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EyeGazingReadyPlayerMe : MonoBehaviour
{

    [SerializeField] private Transform objectToTrack;
    private SkinnedMeshRenderer meshRenderer;
    Mesh skinnedMesh;
    Transform leftEye;
    Transform rightEye;

    int eyeLookDownLeft;
    int eyeLookInLeft;
    int eyeLookOutLeft;
    int eyeLookUpLeft;

    int eyeLookDownRight;
    int eyeLookInRight;
    int eyeLookOutRight;
    int eyeLookUpRight;

    int eyesClosed;

    float timeToBlink = 0.05f;
    float timeBetweenBlinks = 3.5f;
    float timeUntilNextBlink = 0;

    bool isOtherTalking = false;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = meshRenderer.sharedMesh;
        leftEye = transform.GetChild(0);
        rightEye = transform.GetChild(1);
        eyeLookDownLeft = skinnedMesh.GetBlendShapeIndex("eyeLookDownLeft");
        eyeLookInLeft = skinnedMesh.GetBlendShapeIndex("eyeLookInLeft");
        eyeLookOutLeft = skinnedMesh.GetBlendShapeIndex("eyeLookOutLeft");
        eyeLookUpLeft = skinnedMesh.GetBlendShapeIndex("eyeLookUpLeft");

        eyeLookDownRight = skinnedMesh.GetBlendShapeIndex("eyeLookDownRight");
        eyeLookInRight = skinnedMesh.GetBlendShapeIndex("eyeLookInRight");
        eyeLookOutRight = skinnedMesh.GetBlendShapeIndex("eyeLookOutRight");
        eyeLookUpRight = skinnedMesh.GetBlendShapeIndex("eyeLookUpRight");

        eyesClosed = skinnedMesh.GetBlendShapeIndex("eyesClosed");

        VoiceDetector.startedTalking.AddListener(OnOtherStartingSpeaking);
        VoiceDetector.stoppedTalking.AddListener(OnOtherStoppedSpeaking);
    }

    private void OnOtherStartingSpeaking()
    {
        isOtherTalking = true;
    }

    private void OnOtherStoppedSpeaking()
    {
        isOtherTalking = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isOtherTalking) { ResetGazeLeft();ResetGazeRight();return; }

        Vector3 directionLeft = leftEye.InverseTransformPoint(objectToTrack.transform.position).normalized*100;
        if (directionLeft.z< 0)
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownLeft, Mathf.Clamp(-directionLeft.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookUpLeft,Mathf.Clamp(directionLeft.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookInLeft,Mathf.Clamp(-directionLeft.x, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookOutLeft,Mathf.Clamp(directionLeft.x, 0, 100));
        }
        else
        {
            ResetGazeLeft();
        }
        Vector3 directionRight = rightEye.InverseTransformPoint(objectToTrack.transform.position).normalized * 100;
        if (directionRight.z< 0)
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownRight, Mathf.Clamp(-directionRight.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookUpRight, Mathf.Clamp(directionRight.y, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookInRight,Mathf.Clamp(directionRight.x, 0, 100));
            meshRenderer.SetBlendShapeWeight(eyeLookOutRight,Mathf.Clamp(-directionRight.x, 0, 100));
        }
        else
        {
            ResetGazeRight();
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
            meshRenderer.SetBlendShapeWeight(eyesClosed, t*100/timeToBlink);
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

    private void ResetGazeRight()
    {
        meshRenderer.SetBlendShapeWeight(eyeLookDownRight, 0);
        meshRenderer.SetBlendShapeWeight(eyeLookUpRight, 0);
        meshRenderer.SetBlendShapeWeight(eyeLookInRight, 0);
        meshRenderer.SetBlendShapeWeight(eyeLookOutRight, 0);
    }

    private void ResetGazeLeft()
    {
        meshRenderer.SetBlendShapeWeight(eyeLookDownLeft, 0);
        meshRenderer.SetBlendShapeWeight(eyeLookUpLeft, 0);
        meshRenderer.SetBlendShapeWeight(eyeLookInLeft, 0);
        meshRenderer.SetBlendShapeWeight(eyeLookOutLeft, 0);
    }
}
