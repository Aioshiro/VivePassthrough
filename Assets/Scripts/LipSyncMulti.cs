using UnityEngine;
using System.Linq;

public class LipSyncMulti: MonoBehaviour
{
    // PUBLIC

    // Manually assign the skinned mesh renderer to this script
    [Tooltip("Skinned Mesh Rendered target to be driven by Oculus Lipsync")]
    public SkinnedMeshRenderer skinnedMeshRenderer = null;

    // Set the blendshape index to go to (-1 means there is not one assigned)
    [Tooltip("Blendshape index to trigger for each viseme.")]
    public int[] visemeToBlendTargets = Enumerable.Range(0, OVRLipSync.VisemeCount).ToArray();

    [Tooltip("Blendshape index to trigger for laughter")]
    public int laughterBlendTarget = OVRLipSync.VisemeCount;

    [Range(0.0f, 1.0f)]
    [Tooltip("Laughter probability threshold above which the laughter blendshape will be activated")]
    public float laughterThreshold = 0.5f;

    [Range(0.0f, 3.0f)]
    [Tooltip("Laughter animation linear multiplier, the final output will be clamped to 1.0")]
    public float laughterMultiplier = 1.5f;

    public bool multiplyInputBy100 = false;


    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        // morph target needs to be set manually; possibly other components will need the same
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("LipSyncContextMorphTarget.Start Error: " +
                "Please set the target Skinned Mesh Renderer to be controlled!");
            return;
        }
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        if (skinnedMeshRenderer != null && SyncViseme.instance != null)
        {
            SetVisemeToMorphTargetNetwork();
        }
    }

    void SetVisemeToMorphTargetNetwork()
    {
        if (GameManager.Instance.playerNumber == 0)
        {
            for (int i = 0; i < visemeToBlendTargets.Length; i++)
            {
                if (visemeToBlendTargets[i] != -1)
                {
                    if (multiplyInputBy100)
                    {
                        // Viseme blend weights are in range of 0->1.0, we need to make range 100
                        skinnedMeshRenderer.SetBlendShapeWeight(visemeToBlendTargets[i], SyncViseme.instance.playerTwoViseme[i] * 100.0f);
                    }
                    else
                    {
                        skinnedMeshRenderer.SetBlendShapeWeight(visemeToBlendTargets[i], SyncViseme.instance.playerTwoViseme[i]);
                    }

                }
            }
        }
        else
        {
            for (int i = 0; i < visemeToBlendTargets.Length; i++)
            {
                if (visemeToBlendTargets[i] != -1)
                {
                    if (multiplyInputBy100)
                    {
                        // Viseme blend weights are in range of 0->1.0, we need to make range 100
                        skinnedMeshRenderer.SetBlendShapeWeight(visemeToBlendTargets[i], SyncViseme.instance.playerOneViseme[i] * 100.0f);
                    }
                    else
                    {
                        skinnedMeshRenderer.SetBlendShapeWeight(visemeToBlendTargets[i], SyncViseme.instance.playerOneViseme[i]);
                    }

                }
            }
        }
    }

    /// <summary>
    /// Sets the laughter to morph target.
    /// </summary>
    void SetLaughterToMorphTarget(OVRLipSync.Frame frame)
    {
        if (laughterBlendTarget != -1)
        {
            // Laughter score will be raw classifier output in [0,1]
            float laughterScore = frame.laughterScore;

            // Threshold then re-map to [0,1]
            laughterScore = laughterScore < laughterThreshold ? 0.0f : laughterScore - laughterThreshold;
            laughterScore = Mathf.Min(laughterScore * laughterMultiplier, 1.0f);
            laughterScore *= 1.0f / laughterThreshold;

            skinnedMeshRenderer.SetBlendShapeWeight(
                laughterBlendTarget,
                laughterScore * 100.0f);
        }
    }
}
