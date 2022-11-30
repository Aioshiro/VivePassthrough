using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using ViveSR.anipal.Eye;
public class GazeRay : MonoBehaviour
{
    public int LengthOfRay = 25;
    [SerializeField] private LineRenderer GazeRayRenderer;

    public float totalTimeLookingAtHead = 0;

    public bool showGazeRay;
    private bool missingFrames;
    Vector3 oldGazeDirectionCombined;
    float currentIgnoredTime = 0;
    const float timeToIgnoreFrames = 0.35f;

    public int numberOfFixations = 0;
    public float totalFixationTime = 0;
    private float currentFixationTime = 0;
    const float minimumFixationTime = 0.05f; //50ms
    public float timeLookingAtEyes = 0f;
    public float timeLookingAtForehead = 0f;
    public float timeLookingAtMouth = 0f;

    [SerializeField] Transform headTransform;

    private void Start()
    {
        Assert.IsNotNull(GazeRayRenderer);
        if (headTransform == null)
        {
            headTransform = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0).GetChild(2);
        }
    }

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
        if (headTransform == null)
        {
            headTransform = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0).GetChild(2);
        }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out Vector3 GazeOriginCombinedLocal, out Vector3 GazeDirectionCombinedLocal, EyeDataGetter.ownEyeData)) { }
        else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, EyeDataGetter.ownEyeData)) { }
        else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, EyeDataGetter.ownEyeData)) { }
        else
        {
            missingFrames = true;
            return;
        }

        Vector3 GazeDirectionCombined;
        if (missingFrames)
        {
            GazeDirectionCombined = oldGazeDirectionCombined;
            currentIgnoredTime += Time.deltaTime;
            if (currentIgnoredTime > timeToIgnoreFrames)
            {
                currentIgnoredTime = 0;
                missingFrames = false;
            }
            else
            {
                return;
            }
        }
        GazeDirectionCombined = headTransform.TransformDirection(GazeDirectionCombinedLocal).normalized;
        oldGazeDirectionCombined = GazeDirectionCombined;

        if (Physics.Raycast(headTransform.position, GazeDirectionCombined,out RaycastHit info,25, LayerMask.GetMask("Eyes","Mouth","Forehead")))
        {
            //Debug.Log(info.collider.gameObject.layer);
            totalTimeLookingAtHead += Time.deltaTime;
            currentFixationTime += Time.deltaTime;
            int colliderLayer = info.collider.gameObject.layer;
            if (colliderLayer == LayerMask.NameToLayer("Eyes"))
            {
                //Debug.Log("Looking at eyes");
                timeLookingAtEyes += Time.deltaTime;
            }
            else if (colliderLayer == LayerMask.NameToLayer("Mouth"))
            {
                //Debug.Log("Looking at Mouth");
                timeLookingAtMouth += Time.deltaTime;
            }
            else if (colliderLayer == LayerMask.NameToLayer("Forehead"))
            {
                //Debug.Log("Looking at forehead");
                timeLookingAtForehead += Time.deltaTime;
            }

        }
        else
        {
            if (currentFixationTime > minimumFixationTime)
            {
                numberOfFixations += 1;
                totalFixationTime += currentFixationTime;
            }
            currentFixationTime = 0;
        }
        if (showGazeRay)
        {
            GazeRayRenderer.enabled = true;
            GazeRayRenderer.SetPosition(0, headTransform.position - headTransform.up * 0.05f + headTransform.right * 0.02f);
            GazeRayRenderer.SetPosition(1, headTransform.position + GazeDirectionCombined * LengthOfRay);
        }
        else
        {
            GazeRayRenderer.enabled = false;
        }
    }
}
