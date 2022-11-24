﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using ViveSR.anipal.Eye;

public class SyncGaze : NetworkBehaviour
{

    public readonly SyncList<float> playerOneGaze = new SyncList<float>();
    public readonly SyncList<float> playerTwoGaze = new SyncList<float>();

    [SyncVar]
    public bool playerOneMissingFrames;
    [SyncVar]
    public bool playerTwoMissingFrames;

    public override void OnStartServer()
    {
        base.OnStartServer();
        for (int i = 0; i <(int) EyeShape_v2.Max; i++)
        {
            playerOneGaze.Add(0);
            playerTwoGaze.Add(0);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isClientOnly)
        {
            int playerNumber = GameManager.Instance.playerNumber;
            //First, upload own headPos and headRot
            UpdateDict(playerNumber,EyeDataGetter.ownEyeWeightings);
            ApplyOtherEyeData(playerNumber);

            if (GetGazeRayValidity(GazeIndex.COMBINE,EyeDataGetter.ownEyeData)) { UpdateMissingFrames(playerNumber, false); }
            else if (GetGazeRayValidity(GazeIndex.LEFT,  EyeDataGetter.ownEyeData)) { UpdateMissingFrames(playerNumber, false); }
            else if (GetGazeRayValidity(GazeIndex.RIGHT,  EyeDataGetter.ownEyeData)) { UpdateMissingFrames(playerNumber, false); }
            else
            {
                UpdateMissingFrames(playerNumber, true);
            }

            EyeDataGetter.playerOneMissingFrames = playerOneMissingFrames;
            EyeDataGetter.playerTwoMissingFrames = playerTwoMissingFrames;

        }
    }

    void UpdateDict(int index,Dictionary<EyeShape_v2,float> newValues)
    {
        foreach (var key in newValues.Keys)
        {
            if ((int)key <(int) EyeShape_v2.Max && (int)key >=0)
            {
                UpdateIndividualValue(index, key, newValues[key]);
            }
        }
    }

    [Command(requiresAuthority =false)]
    void UpdateMissingFrames(int index, bool value)
    {
        if (index == 0)
        {
            playerOneMissingFrames = value;
        }
        else
        {
            playerTwoMissingFrames = value;
        }
    }

    [Command(requiresAuthority =false)]
    void UpdateIndividualValue(int index, EyeShape_v2 key, float value)
    {
        if (index == 0)
        {
            playerOneGaze[(int)key] = value;
        }
        else
        {
            playerTwoGaze[(int)key] = value;
        }
    }

    void ApplyOtherEyeData(int index)
    {
        if (index == 0)
        {
            for (int i = 0; i <(int) EyeShape_v2.Max; i++)
            {
                EyeDataGetter.otherEyeWeightings[(EyeShape_v2)i] = playerTwoGaze[i];
            }
        }
        else
        {
            for (int i = 0; i < (int)EyeShape_v2.Max; i++)
            {
                EyeDataGetter.otherEyeWeightings[(EyeShape_v2)i] = playerOneGaze[i];
            }
        }
    }

    private bool GetGazeRayValidity(GazeIndex gazeIndex, EyeData_v2 eye_data)
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            return true;
        }
        else
        {
            SingleEyeData[] eyesData = new SingleEyeData[(int)GazeIndex.COMBINE + 1];
            eyesData[(int)GazeIndex.LEFT] = eye_data.verbose_data.left;
            eyesData[(int)GazeIndex.RIGHT] = eye_data.verbose_data.right;
            eyesData[(int)GazeIndex.COMBINE] = eye_data.verbose_data.combined.eye_data;

            if (gazeIndex == GazeIndex.COMBINE)
            {
                return eyesData[(int)GazeIndex.COMBINE].GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
            }
            else if (gazeIndex == GazeIndex.LEFT || gazeIndex == GazeIndex.RIGHT)
            {
                return eyesData[(int)gazeIndex].GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
            }
        }
        return false;
    }
}
