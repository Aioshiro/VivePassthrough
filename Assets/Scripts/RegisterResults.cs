﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using Mirror;


public class RegisterResults : NetworkBehaviour
{

    public Chronometer chronometer;

    void Start()
    {
        chronometer = new Chronometer();
    }

    private void OnApplicationQuit()
    {
        if (isClient)
        {
            Save();
        }
    }

    void Save()
    {
        float totalTime = chronometer.StopChronometer();

        StringBuilder csv = new StringBuilder();

        string id = GameManager.Instance.participantID.ToString();
        string avatarOn;
        string cartoon;
        string male;
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
        string ethnic = GameManager.Instance.chosedEthnie.ToString();
        string time = totalTime.ToString();
        var newLine = string.Format("{0};{1};{2};{3};{4};{5}", id,avatarOn,cartoon,male,ethnic, time);
        csv.AppendLine(newLine);

        SaveLocally(csv.ToString());
        SaveOnServer(csv.ToString());
    }

    void SaveLocally(string newLine)
    {
        Debug.Log("Saving data locally");
        string filePath = GetPath();
        File.AppendAllText(filePath, newLine);
    }

    [Command(requiresAuthority =false)]
    void SaveOnServer(string newLine)
    {
        Debug.Log("Saving data on server");
        string filePath = GetPath();
        File.AppendAllText(filePath, newLine);
    }


    // Following method is used to retrive the relative path as device platform
    private string GetPath()
    {

        string fileName = "results.csv";
#if UNITY_EDITOR
        return Application.dataPath + "/CSV/" + fileName;
#else
        return Application.dataPath +"/"+ fileName;
#endif
    }
}

