using System.Collections;
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

    [SyncVar]
    public bool playerOneIsMale;

    [SyncVar]
    public bool playerTwoIsMale;

    void Start()
    {
        chronometer = new Chronometer();
        //gazeRay = GetComponentInChildren<GazeRay>();
    }

    [Command(requiresAuthority =false)]
    public void SetGenderOfPlayer(bool isMale,int index)
    {
        if (index == 0)
        {
            playerOneIsMale = isMale;
        }
        else
        {
            playerTwoIsMale = isMale;
        }
    }

    /// <summary>
    /// Formats the data to save it in csv on client and on server
    /// </summary>
    public void Save(int taskNumber)
    {
        SetGenderOfPlayer(GameManager.Instance.isMale, GameManager.Instance.playerNumber);

        float totalTime = chronometer.GetChronometerTime();

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
            if (GameManager.Instance.playerNumber == 0)
            {
                if (playerOneIsMale)
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
                if (playerTwoIsMale)
                {
                    male = "Male";
                }
                else
                {
                    male = "Female";
                }
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
        string numberOfBlinks = gazeRay.NumberOfBlinks.ToString();
        string avgLeftPupilDiameter = (gazeRay.totalSizeOfPupilLeft / gazeRay.countOfEyePupilsize).ToString();
        string avgRightPupilDiameter = (gazeRay.totalSizeOfPupilRight / gazeRay.countOfEyePupilsize).ToString();
        var newLine = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16}", DateTime.Now.ToString(),id,avatarOn,cartoon,male,ethnic, lipAnimation,
            time,timeLookingAtHead, timeLookingAtEyes, timeLookingAtMouth, timeLookingAtForehead,numberOfFixations,averageFixationTime,numberOfBlinks,
            avgLeftPupilDiameter, avgRightPupilDiameter);
        csv.AppendLine(newLine);

        SaveLocally(csv.ToString(),taskNumber);
        SaveOnServer(csv.ToString(),taskNumber);
        ResetData();
    }

    private void ResetData()
    {
        gazeRay.ResetData();
    }

    /// <summary>
    /// Show the current measures on screen
    /// </summary>
    private void OnGUI()
    {
        GUI.backgroundColor = Color.red;
        GUI.Label(new Rect(700, 0, 400, 100), "Current measures values :");
        GUI.Label(new Rect(700, 20, 400, 100), $"Current time of experiment : {chronometer.GetChronometerTime()} s");
        GUI.Label(new Rect(700, 40, 400, 100), $"Current time looking at head : {gazeRay.totalTimeLookingAtHead} s");
        GUI.Label(new Rect(700, 60, 400, 100), $"Current time looking at eyes : {gazeRay.timeLookingAtEyes} s");
        GUI.Label(new Rect(700, 80, 400, 100), $"Current time looking at mouth : {gazeRay.timeLookingAtMouth} s");
        GUI.Label(new Rect(700, 100, 400, 100), $"Current time looking at forehead : {gazeRay.timeLookingAtForehead} s");
        GUI.Label(new Rect(700, 120, 400, 100), $"Current number of fixations: {gazeRay.numberOfFixations}");
        GUI.Label(new Rect(700, 140, 400, 100), $"Current average fixations time : {gazeRay.totalFixationTime / gazeRay.numberOfFixations}s");
        GUI.Label(new Rect(700, 160, 400, 100), $"Current number of blinks : {gazeRay.NumberOfBlinks}");
        GUI.Label(new Rect(700, 180, 400, 100), $"Current average size of left pupil : {gazeRay.totalSizeOfPupilLeft/gazeRay.countOfEyePupilsize} mm");
        GUI.Label(new Rect(700, 200, 400, 100), $"Current average size of right pupil : {gazeRay.totalSizeOfPupilRight/ gazeRay.countOfEyePupilsize} mm");

    }

    /// <summary>
    /// Saves data locally
    /// </summary>
    /// <param name="newLine">String of data to save</param>
    void SaveLocally(string newLine,int taskNumber)
    {
        Debug.Log("Saving data locally");
        string filePath = GetPath(taskNumber);
        File.AppendAllText(filePath, newLine);
    }

    /// <summary>
    /// Saves data on client
    /// </summary>
    /// <param name="newLine"> String of data to save </param>
    [Command(requiresAuthority =false)]
    void SaveOnServer(string newLine,int taskNumber)
    {
        Debug.Log("Saving data on server");
        string filePath = GetPath(taskNumber);
        File.AppendAllText(filePath, newLine);
    }


    /// <summary>
    /// Following method is used to retrive the relative path as device platform
    /// </summary>
    /// <returns></returns>
    private string GetPath(int taskNumber)
    {

        string fileName = $"resultsTask{taskNumber}.csv";
#if UNITY_EDITOR
        return Application.dataPath + "/CSV/" + fileName;
#else
        return Application.dataPath + "/"+ fileName;
#endif
    }
}

