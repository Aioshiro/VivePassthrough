//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using Mirror;

public class AvatarLipMulti : MonoBehaviour
{

    public struct PlayerInfo : NetworkMessage
    {
        public int playerNumber;
    }

    [SerializeField] private List<LipShapeTable_v2> LipShapeTables;


    public bool NeededToGetData = true;
    public int ownNumber = 0;
    private LipData lipData;

    public float[] overrides;

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
        if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return;

        if (NeededToGetData)
        {
            SRanipal_Lip_v2.GetLipWeightings(out var weightings);
            lipData.UpdateWeightings(weightings,ownNumber);
            //UpdateLipShapes(lipData.LipWeightings[(ownNumber+1)%2]);
            if(ownNumber == 0)
            {
                //UpdateLipShapes(lipData.LipWeightingsSecondPlayer);
                UpdateLipShapes(lipData.LipWeightingsFirstPlayer);
            }
            else
            {
                UpdateLipShapes(lipData.LipWeightingsFirstPlayer);
            }
        }
    }

    public void GetPlayerNumber(PlayerInfo info)
    {
        ownNumber = info.playerNumber;
    }


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

    public void UpdateLipShapes(SyncDictionary<LipShape_v2, float> lipWeightings)
    {
        foreach (var table in LipShapeTables)
            RenderModelLipShape(table, lipWeightings);
    }

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