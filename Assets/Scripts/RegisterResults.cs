﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using Mirror;

/// <summary>
/// Script to register results of experiment
/// </summary>
public class RegisterResults : NetworkBehaviour
{
    /// <summary>
    /// Chronometer to measure time of experiment
    /// </summary>
    public Chronometer chronometer;
    /// <summary>
    /// GazeRay to gather gaze data
    /// </summary>
    public GazeRay gazeRay;

    void Start()
    {
        chronometer = new Chronometer();
        //gazeRay = GetComponentInChildren<GazeRay>();
    }

    /// <summary>
    /// Formats the data to save it in csv on client and on server
    /// </summary>
    public void Save()
    {
        float totalTime = chronometer.StopChronometer();

        StringBuilder csv = new StringBuilder();

        string id = GameManager.Instance.participantID.ToString();
        string avatarOn;
        string cartoon;
        string male;
        string lipAnimation;
        if (GameManager.Instance.HeadsActive)
        {
            avatarOn = "Active";
            if (GameManager.Instance.isCartoon)
            {
                cartoon = "Cartoon";
            }
            else
            {
                cartoon = "Realistic";
            }
            if (GameManager.Instance.isMale)
            {
                male = "Male";
            }
            else
            {
                male = "Female";
            }
        }
        else
        {
           avatarOn = "Not active";
           cartoon = "";
           male = "";
        }
        if (GameManager.Instance.facialTracker)
        {
            lipAnimation = "Facial tracker";
        }
        else
        {
            lipAnimation = "Lip sync";
        }
        string ethnic = GameManager.Instance.chosedEthnie.ToString();
        string time = totalTime.ToString();
        string timeLookingAtHead = gazeRay.totalTimeLookingAtHead.ToString();
        string timeLookingAtEyes = gazeRay.timeLookingAtEyes.ToString();
        string timeLookingAtMouth = gazeRay.timeLookingAtMouth.ToString();
        string timeLookingAtForehead = gazeRay.timeLookingAtForehead.ToString();
        string numberOfFixations = gazeRay.numberOfFixations.ToString();
        string averageFixationTime = (gazeRay.totalFixationTime / gazeRay.numberOfFixations).ToString();
        var newLine = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12}", id,avatarOn,cartoon,male,ethnic, lipAnimation,time,timeLookingAtHead, timeLookingAtEyes, timeLookingAtMouth, timeLookingAtForehead,numberOfFixations,averageFixationTime);
        csv.AppendLine(newLine);

        SaveLocally(csv.ToString());
        SaveOnServer(csv.ToString());
    }
    /// <summary>
    /// Saves data locally
    /// </summary>
    /// <param name="newLine">String of data to save</param>
    void SaveLocally(string newLine)
    {
        Debug.Log("Saving data locally");
        string filePath = GetPath();
        File.AppendAllText(filePath, newLine);
    }

    /// <summary>
    /// Saves data on client
    /// </summary>
    /// <param name="newLine"> String of data to save </param>
    [Command(requiresAuthority =false)]
    void SaveOnServer(string newLine)
    {
        Debug.Log("Saving data on server");
        string filePath = GetPath();
        File.AppendAllText(filePath, newLine);
    }


    /// <summary>
    /// Following method is used to retrive the relative path as device platform
    /// </summary>
    /// <returns></returns>
    private string GetPath()
    {

        string fileName = "results.csv";
#if UNITY_EDITOR
        return Application.dataPath + "/CSV/" + fileName;
#else
        return Application.dataPath + "/"+ fileName;
#endif
    }
}

