//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using Mirror;

public class AvatarLipMulti : MonoBehaviour
{

    //struct necessary to recieve messages from the server (player number here)
    public struct PlayerInfo : NetworkMessage
    {
        public int playerNumber;
    }

    [Tooltip("Lip shape tables for correspondancies between vive tracker and the model blendshapes")]
    [SerializeField] private List<LipShapeTable_v2> LipShapeTables;

    [Tooltip("The client player number")]
    public int ownNumber = 0;

    //Reference to LipData script, to upload data to server and get other player data
    private LipData lipData;

    //overrides multiplicators for facial tracker, in case we need to amplify some values
    public float[] overrides;

    [Tooltip("Check to multiply facial tracker input by 100, in case model blendshape goes from 0 to 100")]
    [SerializeField] bool multiplyBy100 = false;

    private void Start()
    {
        if (!SRanipal_Lip_Framework.Instance.EnableLip)
        {
            enabled = false;
            return;
        }
        lipData = FindObjectOfType<LipData>();
        SetLipShapeTables(LipShapeTables);
    }

    private void Update()
    {
        //If issue with framework, we stop
        if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return;

        //Getting facial tracker data
        SRanipal_Lip_v2.GetLipWeightings(out var weightings);
        //Uploading facial tracker to server
        lipData.UpdateWeightings(weightings, ownNumber);

        //Uploading local model blenshapes values
        if (ownNumber == 0)
        {
            //UpdateLipShapes(lipData.LipWeightingsSecondPlayer);
            UpdateLipShapes(lipData.LipWeightingsFirstPlayer);
        }
        else
        {
            UpdateLipShapes(lipData.LipWeightingsFirstPlayer);
        }

    }

    //Callback when recieveing PlayerInfo message from server,
    //Initialized in RegisterHandlers.cs
    public void GetPlayerNumber(int number)
    {
        ownNumber = number;
    }

    //Setting up lipShapesTables, code taken from Vive Samples
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

    //Updating every lipshape table, code taken from Vive Samples
    public void UpdateLipShapes(SyncDictionary<LipShape_v2, float> lipWeightings)
    {
        foreach (var table in LipShapeTables)
            RenderModelLipShape(table, lipWeightings);
    }

    //Updating individual lipshape table, code taken from Vive samples
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