//Adaapted from Vive avatarEyeSampleV2 script
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;
using System;

/// <summary>
/// Moves an avatar eyes with the output of EyeDataGetter.cs, i.e. partner's Vive Tracker
/// </summary>
public class AvatarEyeControlMulti : MonoBehaviour
{
    [Tooltip("Transform of left eye and then right eye, must have a 0 rotation when looking forward")]
    [SerializeField] private Transform[] EyesModels = new Transform[0];

    [Tooltip("List of eyeshapes tables to link eye output and blendshapes")]
    [SerializeField] private List<EyeShapeTable_v2> EyeShapeTables;

    [Tooltip("Customize this curve to fit the blend shapes of your avatar.")]
    [SerializeField] private AnimationCurve EyebrowAnimationCurveUpper;

    [Tooltip("Customize this curve to fit the blend shapes of your avatar.")]
    [SerializeField] private AnimationCurve EyebrowAnimationCurveLower;

    [Tooltip("Customize this curve to fit the blend shapes of your avatar.")]
    [SerializeField] private AnimationCurve EyebrowAnimationCurveHorizontal;

    private AnimationCurve[] EyebrowAnimationCurves = new AnimationCurve[(int)EyeShape_v2.Max];
    /// <summary>
    /// Eyes anchors to link object on eyes
    /// </summary>
    private GameObject[] EyeAnchors;
    /// <summary>
    /// Should rarely be different than two
    /// </summary>
    private const int NUM_OF_EYES = 2;

    /// <summary>
    /// Max time during which we ignore frames
    /// </summary>
    const float timeToIgnoreFrames = 0.35f;
    /// <summary>
    /// Clock to see how much time has passed since we started to ignore frames
    /// </summary>
    float currentIgnoredTime = 0;
    /// <summary>
    /// Is true when some of the gaze frames have been missing, mainly during blinks
    /// </summary>
    bool missingFramesInternal =false;

    /// <summary>
    /// Should the weight for the blendshapes be between [0,1] or [0,100] ?
    /// </summary>
    public bool multiplyBlendshapeBy100 = false;

    private void Start()
    {
        //setting up as in the script samples

        SetEyesModels(EyesModels[0], EyesModels[1]);
        SetEyeShapeTables(EyeShapeTables);

        AnimationCurve[] curves = new AnimationCurve[(int)EyeShape_v2.Max];
        for (int i = 0; i < EyebrowAnimationCurves.Length; ++i)
        {
            if (i == (int)EyeShape_v2.Eye_Left_Up || i == (int)EyeShape_v2.Eye_Right_Up) curves[i] = EyebrowAnimationCurveUpper;
            else if (i == (int)EyeShape_v2.Eye_Left_Down || i == (int)EyeShape_v2.Eye_Right_Down) curves[i] = EyebrowAnimationCurveLower;
            else curves[i] = EyebrowAnimationCurveHorizontal;
        }
        SetEyeShapeAnimationCurves(curves);
    }

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
        int playerNumber = GameManager.Instance.playerNumber;

        //Checking if other player has missing frames (during a blink generally)
        bool missingFrames;
        if (playerNumber == 0)
        {
            missingFrames = EyeDataGetter.playerTwoMissingFrames;
        }
        else
        {
            missingFrames = EyeDataGetter.playerOneMissingFrames;
        }

        //If other player miss frame now or has been recently
        if (missingFrames||missingFramesInternal)
        {
            missingFramesInternal = true;
            currentIgnoredTime += Time.deltaTime;
            //if enough time has passed since we starting ignore frames, we go back to normal
            if (currentIgnoredTime > timeToIgnoreFrames)
            {
                currentIgnoredTime = 0;
                missingFramesInternal = false;
            }
            //Otherwise, we make the blink longer to hide the missing frames
            else
            {
                float closedValue;
                if (currentIgnoredTime < timeToIgnoreFrames / 2)
                {
                    closedValue = 1;
                }
                else
                {
                    closedValue = -2 * (currentIgnoredTime / timeToIgnoreFrames) + 2;
                }
                EyeDataGetter.otherEyeWeightings[EyeShape_v2.Eye_Left_Blink] = closedValue;
                EyeDataGetter.otherEyeWeightings[EyeShape_v2.Eye_Right_Blink] = closedValue;
                UpdateEyeShapes(EyeDataGetter.otherEyeWeightings);
                return;
            }
        }

        //If everything fine, we just update the blendshapes
        UpdateEyeShapes(EyeDataGetter.otherEyeWeightings);
        UpdateGazeRay(EyeDataGetter.otherGazeDirectionLocal);
    }
    /// <summary>
    /// Destroying EyeAnchors on destroy
    /// </summary>
    private void OnDestroy()
    {
        DestroyEyeAnchors();
    }
    /// <summary>
    /// Setting up eye models and anchors.
    /// </summary>
    /// <param name="leftEye"> Left eye transform</param>
    /// <param name="rightEye"> Right eye transform</param>
    public void SetEyesModels(Transform leftEye, Transform rightEye)
    {
        if (leftEye != null && rightEye != null)
        {
            EyesModels = new Transform[NUM_OF_EYES] { leftEye, rightEye };
            DestroyEyeAnchors();
            CreateEyeAnchors();
        }
    }

    /// <summary>
    /// Setting up eye shapes tables
    /// </summary>
    /// <param name="eyeShapeTables"> The eyeshapes tables list</param>
    public void SetEyeShapeTables(List<EyeShapeTable_v2> eyeShapeTables)
    {
        bool valid = true;
        if (eyeShapeTables == null)
        {
            valid = false;
        }
        else
        {
            for (int table = 0; table < eyeShapeTables.Count; ++table)
            {
                if (eyeShapeTables[table].skinnedMeshRenderer == null)
                {
                    valid = false;
                    break;
                }
                for (int shape = 0; shape < eyeShapeTables[table].eyeShapes.Length; ++shape)
                {
                    EyeShape_v2 eyeShape = eyeShapeTables[table].eyeShapes[shape];
                    if (eyeShape > EyeShape_v2.Max || eyeShape < 0)
                    {
                        valid = false;
                        break;
                    }
                }
            }
        }
        if (valid)
            EyeShapeTables = eyeShapeTables;
    }

    /// <summary>
    /// Setting up eye brow animation curves
    /// </summary>
    /// <param name="eyebrowAnimationCurves"></param>
    public void SetEyeShapeAnimationCurves(AnimationCurve[] eyebrowAnimationCurves)
    {
        if (eyebrowAnimationCurves.Length == (int)EyeShape_v2.Max)
            EyebrowAnimationCurves = eyebrowAnimationCurves;
    }


    /// <summary>
    /// Updating gaze ray direction
    /// </summary>
    /// <param name="gazeDirectionCombinedLocal"> The gaze direction in the camera space
    /// </param>
    public void UpdateGazeRay(Vector3 gazeDirectionCombinedLocal)
    {
        for (int i = 0; i < EyesModels.Length; ++i)
        {
            Vector3 target = EyeAnchors[i].transform.TransformPoint(gazeDirectionCombinedLocal);
            //Debug.DrawLine(EyeAnchors[i].transform.position, target, Color.red);
            //EyesModels[i].rotation = Quaternion.LookRotation(target);
            EyesModels[i].LookAt(target); //this is why the L/R eyes transform must have a zero rotation when looking forward
            //EyesModels[i].rotation = Quaternion.LookRotation(EyesModels[i].forward, target);
        }
    }
    /// <summary>
    /// Updating all eye shapes
    /// </summary>
    /// <param name="eyeWeightings"> The weightings obtained from Vive's SRanipal </param>
    public void UpdateEyeShapes(Dictionary<EyeShape_v2, float> eyeWeightings)
    {
        foreach (var table in EyeShapeTables)
            RenderModelEyeShape(table, eyeWeightings);
    }

    /// <summary>
    /// Updating individual eye shape table
    /// </summary>
    /// <param name="eyeShapeTable"> The eyeShapeTable to update</param>
    /// <param name="weighting">The weightings obtained from Vive's SRanipal</param>
    private void RenderModelEyeShape(EyeShapeTable_v2 eyeShapeTable, Dictionary<EyeShape_v2, float> weighting)
    {
        for (int i = 0; i < eyeShapeTable.eyeShapes.Length; ++i)
        {
            EyeShape_v2 eyeShape = eyeShapeTable.eyeShapes[i];
            if (eyeShape > EyeShape_v2.Max || eyeShape < 0) continue;

            if (eyeShape == EyeShape_v2.Eye_Left_Blink || eyeShape == EyeShape_v2.Eye_Right_Blink)
                eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[eyeShape] * (99f * Convert.ToUInt16(multiplyBlendshapeBy100) + 1));
            else
            {
                AnimationCurve curve = EyebrowAnimationCurves[(int)eyeShape];
                eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, curve.Evaluate(weighting[eyeShape]) * (99f * Convert.ToUInt16(multiplyBlendshapeBy100) + 1));
            }
        }
    }

    /// <summary>
    /// Creating EyeAnchors
    /// </summary>
    private void CreateEyeAnchors()
    {
        EyeAnchors = new GameObject[NUM_OF_EYES];
        for (int i = 0; i < NUM_OF_EYES; ++i)
        {
            EyeAnchors[i] = new GameObject
            {
                name = "EyeAnchor_" + i
            };
            EyeAnchors[i].transform.SetParent(gameObject.transform);
            EyeAnchors[i].transform.position = EyesModels[i].position;
            EyeAnchors[i].transform.rotation = EyesModels[i].rotation;
            EyeAnchors[i].transform.localScale = EyesModels[i].localScale;
        }
    }

    /// <summary>
    /// Destroying eye anchors
    /// </summary>
    private void DestroyEyeAnchors()
    {
        if (EyeAnchors != null)
        {
            foreach (var obj in EyeAnchors)
                if (obj != null) Destroy(obj);
        }
    }
}