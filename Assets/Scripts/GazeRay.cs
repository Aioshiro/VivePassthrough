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
        GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal).normalized;
        oldGazeDirectionCombined = GazeDirectionCombined;

        if (Physics.Raycast(headTransform.position, GazeDirectionCombined, 25, LayerMask.GetMask("Head")))
        {
            Debug.Log("Looking at head");
            totalTimeLookingAtHead += Time.deltaTime;
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
