//Adaapted from Vive avatarEyeSampleV2 script
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;

public class AvatarEyeControlMulti : MonoBehaviour
{
    [SerializeField] private Transform[] EyesModels = new Transform[0];
    [SerializeField] private List<EyeShapeTable_v2> EyeShapeTables;
    /// <summary>
    /// Customize this curve to fit the blend shapes of your avatar.
    /// </summary>
    [SerializeField] private AnimationCurve EyebrowAnimationCurveUpper;
    /// <summary>
    /// Customize this curve to fit the blend shapes of your avatar.
    /// </summary>
    [SerializeField] private AnimationCurve EyebrowAnimationCurveLower;
    /// <summary>
    /// Customize this curve to fit the blend shapes of your avatar.
    /// </summary>
    [SerializeField] private AnimationCurve EyebrowAnimationCurveHorizontal;

    public bool NeededToGetData = true;
    private AnimationCurve[] EyebrowAnimationCurves = new AnimationCurve[(int)EyeShape_v2.Max];
    private GameObject[] EyeAnchors;
    private const int NUM_OF_EYES = 2;

    const float timeToIgnoreFrames = 0.35f;
    float currentIgnoredTime = 0;
    bool missingFramesInternal;

    private void Start()
    {
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
        bool missingFrames;
        if (playerNumber == 0)
        {
            missingFrames = EyeDataGetter.playerTwoMissingFrames;
        }
        else
        {
            missingFrames = EyeDataGetter.playerOneMissingFrames;
        }

        if (missingFrames||missingFramesInternal)
        {
            missingFramesInternal = true;
            currentIgnoredTime += Time.deltaTime;
            if (currentIgnoredTime > timeToIgnoreFrames)
            {
                currentIgnoredTime = 0;
                missingFramesInternal = false;
            }
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

        UpdateEyeShapes(EyeDataGetter.otherEyeWeightings);
        
    }

    private void OnDestroy()
    {
        DestroyEyeAnchors();
    }

    public void SetEyesModels(Transform leftEye, Transform rightEye)
    {
        if (leftEye != null && rightEye != null)
        {
            EyesModels = new Transform[NUM_OF_EYES] { leftEye, rightEye };
            DestroyEyeAnchors();
            CreateEyeAnchors();
        }
    }

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

    public void SetEyeShapeAnimationCurves(AnimationCurve[] eyebrowAnimationCurves)
    {
        if (eyebrowAnimationCurves.Length == (int)EyeShape_v2.Max)
            EyebrowAnimationCurves = eyebrowAnimationCurves;
    }

    public void UpdateGazeRay(Vector3 gazeDirectionCombinedLocal)
    {
        for (int i = 0; i < EyesModels.Length; ++i)
        {
            Vector3 target = EyeAnchors[i].transform.TransformPoint(gazeDirectionCombinedLocal);
            Debug.DrawLine(EyeAnchors[i].transform.position, target, Color.red);
            //EyesModels[i].rotation = Quaternion.LookRotation(target);
            EyesModels[i].LookAt(target);
            //EyesModels[i].rotation = Quaternion.LookRotation(EyesModels[i].forward, target);
        }
    }

    public void UpdateEyeShapes(Dictionary<EyeShape_v2, float> eyeWeightings)
    {
        foreach (var table in EyeShapeTables)
            RenderModelEyeShape(table, eyeWeightings);
    }

    private void RenderModelEyeShape(EyeShapeTable_v2 eyeShapeTable, Dictionary<EyeShape_v2, float> weighting)
    {
        for (int i = 0; i < eyeShapeTable.eyeShapes.Length; ++i)
        {
            EyeShape_v2 eyeShape = eyeShapeTable.eyeShapes[i];
            if (eyeShape > EyeShape_v2.Max || eyeShape < 0) continue;

            if (eyeShape == EyeShape_v2.Eye_Left_Blink || eyeShape == EyeShape_v2.Eye_Right_Blink)
                eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[eyeShape] * 100f);
            else
            {
                AnimationCurve curve = EyebrowAnimationCurves[(int)eyeShape];
                eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, curve.Evaluate(weighting[eyeShape]) * 100f);
            }
        }
    }

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

    private void DestroyEyeAnchors()
    {
        if (EyeAnchors != null)
        {
            foreach (var obj in EyeAnchors)
                if (obj != null) Destroy(obj);
        }
    }
}