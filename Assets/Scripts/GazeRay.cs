using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using ViveSR.anipal.Eye;

/// <summary>
/// Script to measure data related to gaze
/// </summary>
public class GazeRay : MonoBehaviour
{ 
    [Tooltip("Enable to show ray in game")]
    public bool showGazeRay;
    [Tooltip("Length of the ray")]
    public int LengthOfRay = 25;
    [Tooltip("The ray renderer")]
    [SerializeField] private LineRenderer GazeRayRenderer;

    [Tooltip("Total time during the experiment that the user look at his partener's head")]
    public float totalTimeLookingAtHead = 0;

    /// <summary>
    /// Is true when some of the gaze frames are missing, mainly during blinks
    /// </summary>
    private bool missingFrames;
    /// <summary>
    /// Clock to see how much time has passed since we started to ignore frames
    /// </summary>
    float currentIgnoredTime = 0;
    /// <summary>
    /// Max time during which we ignore frames
    /// </summary>
    const float timeToIgnoreFrames = 0.35f;

    [Tooltip("Count the number of fixations on the partner's head")]
    public int numberOfFixations = 0;
    [Tooltip("Count the total time of fixations on the partner's head")]
    public float totalFixationTime = 0;
    /// <summary>
    /// How long is the current fixation
    /// </summary>
    private float currentFixationTime = 0;
    /// <summary>
    /// 50ms, we minimum time that we consider there is a fixation
    /// </summary>
    const float minimumFixationTime = 0.05f;
    [Tooltip("Count the total time of fixations on the partner's eyes")]
    public float timeLookingAtEyes = 0f;
    [Tooltip("Count the total time of fixations on the partner's forehead")]
    public float timeLookingAtForehead = 0f;
    [Tooltip("Count the total time of fixations on the partner's mouth")]
    public float timeLookingAtMouth = 0f;

    [Tooltip("The partner's head")]
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
    }
}
