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

    const float timeToBlink = 0.05f;
    const float timeBetweenBlinks = 3.5f;
    float timeUntilNextBlink = 0;
    const float timeToReset = 0.1f;
    const float maxLookingSpeed = 0.01f;

    bool isOtherTalking = false;

    public Coroutine resetCoroutine;

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

    void Update()
    {
        if (!isOtherTalking)
        {
            if (resetCoroutine == null) { resetCoroutine = StartCoroutine(nameof(ResetCoroutine)); }
            return;
        }

        LookAtObject();
        UpdateBlink();

    }

    void LookAtObject()
    {
        Vector3 directionLeft = leftEye.InverseTransformPoint(objectToTrack.transform.position).normalized * 100;
        float currentVelocity = 0;
        if (directionLeft.z < 0)
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownLeft, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookDownLeft), Mathf.Clamp(-directionLeft.y, 0, 100), ref currentVelocity, maxLookingSpeed));
            meshRenderer.SetBlendShapeWeight(eyeLookUpLeft, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookUpLeft), Mathf.Clamp(directionLeft.y, 0, 100), ref currentVelocity, maxLookingSpeed));
            meshRenderer.SetBlendShapeWeight(eyeLookInLeft, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookInLeft), Mathf.Clamp(-directionLeft.x, 0, 100), ref currentVelocity, maxLookingSpeed));
            meshRenderer.SetBlendShapeWeight(eyeLookOutLeft, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookOutLeft), Mathf.Clamp(directionLeft.x, 0, 100), ref currentVelocity, maxLookingSpeed));
        }
        else //If trying to look at something behind him, probably won't happen
        {
            Debug.LogWarning("Tried to look behind him");
            ResetGazeLeft();
        }
        Vector3 directionRight = rightEye.InverseTransformPoint(objectToTrack.transform.position).normalized * 100;
        if (directionRight.z < 0)
        {
            meshRenderer.SetBlendShapeWeight(eyeLookDownRight, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookDownRight), Mathf.Clamp(-directionRight.y, 0, 100), ref currentVelocity, maxLookingSpeed));
            meshRenderer.SetBlendShapeWeight(eyeLookUpRight, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookUpRight), Mathf.Clamp(directionRight.y, 0, 100), ref currentVelocity, maxLookingSpeed));
            meshRenderer.SetBlendShapeWeight(eyeLookInRight, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookInRight), Mathf.Clamp(directionRight.x, 0, 100), ref currentVelocity, maxLookingSpeed));
            meshRenderer.SetBlendShapeWeight(eyeLookOutRight, Mathf.SmoothDamp(meshRenderer.GetBlendShapeWeight(eyeLookOutRight), Mathf.Clamp(-directionRight.x, 0, 100), ref currentVelocity, maxLookingSpeed));
        }
        else//If trying to look at something behind him, probably won't happen
        {
            Debug.LogWarning("Tried to look behind him");
            ResetGazeRight();
        }

    }

    void UpdateBlink()
    {
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

    IEnumerator ResetCoroutine()
    {
        float initialeyeLookDownLeft = meshRenderer.GetBlendShapeWeight(eyeLookDownLeft);
        float initialeyeLookDownRight = meshRenderer.GetBlendShapeWeight(eyeLookDownRight);
        float initialeyeLookUpLeft = meshRenderer.GetBlendShapeWeight(eyeLookUpLeft);
        float initialeyeLookUpRight = meshRenderer.GetBlendShapeWeight(eyeLookUpRight);
        float initialeyeLookInLeft = meshRenderer.GetBlendShapeWeight(eyeLookInLeft);
        float initialeyeLookInRight = meshRenderer.GetBlendShapeWeight(eyeLookInRight);
        float initialeyeLookOutLeft = meshRenderer.GetBlendShapeWeight(eyeLookOutLeft);
        float initialeyeLookOutRight = meshRenderer.GetBlendShapeWeight(eyeLookOutRight);

        float t = 0;
        while (t < timeToReset)
        {
            t += Time.deltaTime;
            float temp = t / timeToReset;
            meshRenderer.SetBlendShapeWeight(eyeLookDownLeft, Mathf.Lerp(initialeyeLookDownLeft,0, temp));
            meshRenderer.SetBlendShapeWeight(eyeLookUpLeft, Mathf.Lerp(initialeyeLookUpLeft, 0, temp));
            meshRenderer.SetBlendShapeWeight(eyeLookInLeft, Mathf.Lerp(initialeyeLookInLeft, 0, temp));
            meshRenderer.SetBlendShapeWeight(eyeLookOutLeft, Mathf.Lerp(initialeyeLookOutLeft, 0, temp));
            meshRenderer.SetBlendShapeWeight(eyeLookDownRight, Mathf.Lerp(initialeyeLookDownRight, 0, temp));
            meshRenderer.SetBlendShapeWeight(eyeLookUpRight, Mathf.Lerp(initialeyeLookUpRight, 0, temp));
            meshRenderer.SetBlendShapeWeight(eyeLookInRight, Mathf.Lerp(initialeyeLookInRight, 0, temp));
            meshRenderer.SetBlendShapeWeight(eyeLookOutRight, Mathf.Lerp(initialeyeLookOutRight, 0, temp));
            yield return new WaitForFixedUpdate();
        }
        resetCoroutine = null;
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
