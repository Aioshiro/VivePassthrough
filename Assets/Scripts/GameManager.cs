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

    public bool HeadsActive = true;
    public bool isCartoon = true;
    public bool isMale = false;
    public int participantID = -1;
    public bool allowSceneChange = false;
    public int playerNumber = 0;
    public enum Ethnie 
    {Asian,African,Caucasian,Latino};

    public Ethnie chosedEthnie = Ethnie.Caucasian;

    public string serverIp;



    private void Awake()
    {
        NetworkClient.RegisterHandler<PlayerInfo>(SetPlayerNumber,false);
    }

    void Start()
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

    public void SetPlayerNumber(PlayerInfo playerInfo)
    {
        GameManager.Instance.playerNumber = playerInfo.playerNumber;        
    }

    public void SetServerIp(string ip)
    {
        serverIp = ip;
    }

}
