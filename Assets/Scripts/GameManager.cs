using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class GameManager : MonoBehaviour
{

    //struct necessary to recieve messages from the server (player number here)
    public struct PlayerInfo : NetworkMessage
    {
        public int playerNumber;
    }

    public static GameManager Instance;

    [Tooltip("Does the other player have an avatar ?")]
    public bool HeadsActive = true;
    [Tooltip("Is the other player avatar cartoon ?")]
    public bool isCartoon = true;
    [Tooltip("Is the other player avatar male ?")]
    public bool isMale = false;

    [Tooltip("Participant id to save results")]
    public int participantID = -1;
    private bool allowSceneChange = false;
    [Tooltip("Player number, 0 is first, 1 is second")]
    public int playerNumber = 0;

    [Tooltip("Are we using the facial tracker (or the lip sync instead)")]
    public bool facialTracker = false;
    public enum Ethnie 
    {Asian,African,Caucasian,Latino};

    [Tooltip("Other player ethnic")]
    public Ethnie chosedEthnie = Ethnie.Caucasian;
    public float otherPersonHeadLength = 0.17f; //average person head length from chin to midpoint of hairline (crinion)

    public string serverIp = "localhost";


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        //register callback when server sends player number
        NetworkClient.RegisterHandler<PlayerInfo>(SetPlayerNumber,false);
    }

    public void ChangeScene(string SceneToLoad)
    {
        if (allowSceneChange)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneToLoad);
        }
    }

    public void ChangeParticipantID(string id)
    {
        try
        {
            participantID = int.Parse(id);
            allowSceneChange = true;
        }
        catch
        {
            allowSceneChange = false;
        }
    }

    public void ToggleHeads()
    {
        HeadsActive = !HeadsActive;
    }
    public void ToggleCartoon()
    {
        isCartoon = !isCartoon;
    }
    public void ToggleGender()
    {
        isMale = !isMale;
    }

    public void OnCaucasianToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.Caucasian;
        }
    }

    public void OnAsianToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.Asian;
        }
    }
    public void OnLatinoToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.Latino;
        }
    }
    public void OnAfricanToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.African;
        }
    }

    public void OnFacialTrackerToggle(bool value)
    {
        facialTracker = value;
    }

    public void OnHeadLengthEnter(string value)
    {
        otherPersonHeadLength= float.Parse(value)/100;
    }

    public void SetPlayerNumber(PlayerInfo playerInfo)
    {
        GameManager.Instance.playerNumber = playerInfo.playerNumber;        
    }

    public void SetServerIp(string ip)
    {
        serverIp = ip;
    }

}
