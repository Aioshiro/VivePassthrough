//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using Mirror;


/// <summary>
/// Updates avatar mouth with the other player facial tracker's data, using LipData.cs
/// </summary>
public class AvatarLipMulti : MonoBehaviour
{

    [Tooltip("Lip shape tables for correspondancies between vive tracker and the model blendshapes")]
    [SerializeField] private List<LipShapeTable_v2> LipShapeTables;

    /// <summary>
    /// Reference to LipData script, to upload data to server and get other player data
    /// </summary>
    private LipData lipData;

    [Tooltip("Overrides multiplicators for facial tracker, in case we need to amplify some values")]
    public float[] overrides;

    [Tooltip("Check to multiply facial tracker input by 100, in case model blendshape goes from 0 to 100")]
    [SerializeField] bool multiplyBy100 = false;

    private void Start()
    {
        lipData = FindObjectOfType<LipData>();
        SetLipShapeTables(LipShapeTables);
    }

    private void Update()
    {

        //Uploading local model blenshapes values
        if (GameManager.Instance.playerNumber == 0)
        {
            UpdateLipShapes(lipData.LipWeightingsSecondPlayer);
            //UpdateLipShapes(lipData.LipWeightingsFirstPlayer);
        }
        else
        {
            UpdateLipShapes(lipData.LipWeightingsFirstPlayer);
        }

    }

    /// <summary>
    /// Setting up lipShapesTables, code taken from Vive Samples
    /// </summary>
    /// <param name="lipShapeTables"> Lip shape tables to update</param>
    public void SetLipShapeTables(List<LipShapeTable_v2> lipShapeTables)
    {
        bool valid = true;
        if (lipShapeTables == null)
        {
            valid = false;
        }
        else
        {
            for (int table = 0; table < lipShapeTables.Count; ++table)
            {
                if (lipShapeTables[table].skinnedMeshRenderer == null)
                {
                    valid = false;
                    break;
                }
                for (int shape = 0; shape < lipShapeTables[table].lipShapes.Length; ++shape)
                {
                    LipShape_v2 lipShape = lipShapeTables[table].lipShapes[shape];
                    if (lipShape > LipShape_v2.Max || lipShape < 0)
                    {
                        valid = false;
                        break;
                    }
                }
            }
        }
        if (valid)
            LipShapeTables = lipShapeTables;
    }

    /// <summary>
    /// Updating every lipshape table, code taken from Vive Samples
    /// </summary>
    /// <param name="lipWeightings"> Dictionnary of lip weighting</param>
    public void UpdateLipShapes(SyncDictionary<LipShape_v2, float> lipWeightings)
    {
        foreach (var table in LipShapeTables)
            RenderModelLipShape(table, lipWeightings);
    }

    /// <summary>
    /// Updating individual lipshape table, code taken from Vive samples
    /// </summary>
    /// <param name="lipShapeTable">Lipshape table to update</param>
    /// <param name="weighting">Dictionnary of lip weighting</param>
    private void RenderModelLipShape(LipShapeTable_v2 lipShapeTable, SyncDictionary<LipShape_v2, float> weighting)
    {
        for (int i = 0; i < lipShapeTable.lipShapes.Length; i++)
        {
            int targetIndex = (int)lipShapeTable.lipShapes[i];
            if (targetIndex > (int)LipShape_v2.Max || targetIndex < 0) continue;
            float weight = Mathf.Clamp01(weighting[(LipShape_v2)targetIndex] * overrides[targetIndex]);
            if (multiplyBy100)
            {
                lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weight * 100);
            }
            else
            {
                lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weight);

            }
        }
    }

}