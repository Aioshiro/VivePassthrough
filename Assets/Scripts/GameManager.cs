using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

/// <summary>
/// GameManager script to set up experiments settings
/// </summary>
public class GameManager : MonoBehaviour
{

    /// <summary>
    /// Struct necessary to recieve player info from the server (player number for now)
    /// </summary>
    public struct PlayerInfo : NetworkMessage
    {
        public int playerNumber;
    }

    /// <summary>
    /// Game Manager singleton instance
    /// </summary>
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

    [Tooltip("Other person head length from chin to midpoint of hairline")]
    public float otherPersonHeadLength = 0.17f; //average person head length from chin to midpoint of hairline (crinion)

    [Tooltip("Is language set to english ? (French otherwise)")]
    public bool languageSetToEnglish = false;

    [Tooltip("The server's ip")]
    public string serverIp = "localhost";

    public int currentTask = 0; //0 for no task, 1 for first task, 2 for second task
    public string TwentyQuestionsWord="";


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
        RegisterHandler();
    }
    /// <summary>
    /// Registering handler for playerInfo messages
    /// </summary>
    public void RegisterHandler()
    {
        NetworkClient.RegisterHandler<PlayerInfo>(SetPlayerNumber, false);
        Debug.Log("Handling PlayerInfo");
    }
    /// <summary>
    /// Change the scene
    /// </summary>
    /// <param name="SceneToLoad"> Scene's name</param>
    public void ChangeScene(string SceneToLoad)
    {
        if (allowSceneChange)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneToLoad);
        }
    }
    /// <summary>
    /// Change the particpant's id
    /// </summary>
    /// <param name="id"></param>
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

    /// <summary>
    /// Toggle language
    /// </summary>
    /// <param name="value"> True if English, false otherwise</param>
    public void ToggleLanguage(bool value)
    {
        languageSetToEnglish = value;
    }
    /// <summary>
    /// Toggle avatar activation
    /// </summary>
    public void ToggleHeads()
    {
        HeadsActive = !HeadsActive;
    }
    /// <summary>
    /// Toggles cartoon avatars
    /// </summary>
    public void ToggleCartoon()
    {
        isCartoon = !isCartoon;
    }
    /// <summary>
    /// Toggle gender
    /// </summary>
    public void ToggleGender()
    {
        isMale = !isMale;
    }

    /// <summary>
    /// If input is true, activate caucasian avatars
    /// </summary>
    /// <param name="value"></param>
    public void OnCaucasianToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.Caucasian;
        }
    }

    /// <summary>
    /// If input is true, activate asian avatars
    /// </summary>
    /// <param name="value"></param>
    public void OnAsianToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.Asian;
        }
    }
    /// <summary>
    /// If input is true, activate latino avatars
    /// </summary>
    /// <param name="value"></param>
    public void OnLatinoToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.Latino;
        }
    }
    /// <summary>
    /// If input is true, activate african avatars
    /// </summary>
    /// <param name="value"></param>
    public void OnAfricanToggle(bool value)
    {
        if (value)
        {
            chosedEthnie = Ethnie.African;
        }
    }

    /// <summary>
    /// If input is true, activate facial tracker input for mouth
    /// </summary>
    /// <param name="value"></param>
    public void OnFacialTrackerToggle(bool value)
    {
        facialTracker = value;
    }

    /// <summary>
    /// Updates other player head length
    /// </summary>
    /// <param name="value"> String containing head length in cm</param>
    public void OnHeadLengthEnter(string value)
    {
        otherPersonHeadLength= float.Parse(value)/100;
    }
    /// <summary>
    /// Sets player number
    /// </summary>
    /// <param name="playerInfo"> The PlayerInfo containing the player number </param>
    public void SetPlayerNumber(PlayerInfo playerInfo)
    {
        GameManager.Instance.playerNumber = playerInfo.playerNumber;        
    }

    /// <summary>
    /// Sets the server ip
    /// </summary>
    /// <param name="ip">Server's ip</param>
    public void SetServerIp(string ip)
    {
        serverIp = ip;
    }

    public void SetWord(string word)
    {
        TwentyQuestionsWord = word;
    }
}
