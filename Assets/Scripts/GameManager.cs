using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public bool HeadsActive = false;
    public bool isCartoon = false;
    public bool isMale = false;
    public int participantID = -1;
    public bool allowSceneChange = false;

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

}
