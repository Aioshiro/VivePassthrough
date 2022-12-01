using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Initializes the right avatar this the data from the GameManager
/// </summary>
public class AvatarInitializer : MonoBehaviour
{
    [Tooltip("Male avatar's heads")]
    [SerializeField] List<GameObject> maleHeads;

    [Tooltip("Woman avatar's heads")]
    [SerializeField] List<GameObject> womanHeads;

    [Tooltip("Cartoon avatar's heads")]
    [SerializeField] List<GameObject> cartoonHeads;

    [Tooltip("Realistic avatar's heads")]
    [SerializeField] List<GameObject> realisticHeads;

    [Tooltip("African avatar's heads")]
    [SerializeField] GameObject africanHeads;

    [Tooltip("Asian avatar's heads")]
    [SerializeField] GameObject asianHeads;

    [Tooltip("Caucasian avatar's heads")]
    [SerializeField] GameObject caucasianHeads;

    [Tooltip("Latino avatar's heads")]
    [SerializeField] GameObject latinoHeads;

    private void Start()
    {
        //We make sure everything is unactive, just in case
        SetActiveList(maleHeads, false);
        SetActiveList(womanHeads, false);
        SetActiveList(cartoonHeads,false);
        SetActiveList(realisticHeads,false);
        africanHeads.SetActive(false);
        asianHeads.SetActive(false);
        caucasianHeads.SetActive(false);
        latinoHeads.SetActive(false);
        AvatarLipMulti[] facialScripts = FindObjectsOfType<AvatarLipMulti>();
        foreach (var script in facialScripts)
        {
            script.enabled = false;
        }
        LipSyncMulti[] lipSyncScripts = FindObjectsOfType<LipSyncMulti>();
        foreach(var script in lipSyncScripts)
        {
            script.enabled = false;
        }

        //We initialize properly depending on the settings
        InitializeHeads();
    }

    /// <summary>
    /// Enable the right avatar gameObject
    /// </summary>
    private void InitializeHeads()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HeadsActive) { return; }

        switch (GameManager.Instance.chosedEthnie)
        {
            case GameManager.Ethnie.Asian:
                asianHeads.SetActive(true);
                break;

            case GameManager.Ethnie.African:
                africanHeads.SetActive(true);
                break;

            case GameManager.Ethnie.Caucasian:
                caucasianHeads.SetActive(true);
                break;

            case GameManager.Ethnie.Latino:
                latinoHeads.SetActive(true);
                break;

        }

        if (GameManager.Instance.isCartoon) { SetActiveList(cartoonHeads,true); }
        else { SetActiveList(realisticHeads,true); }

        if (GameManager.Instance.isMale) { SetActiveList(maleHeads, true); }
        else { SetActiveList(womanHeads, true); }

        if (GameManager.Instance.facialTracker)
        {
            AvatarLipMulti[] facialScripts = FindObjectsOfType<AvatarLipMulti>();
            foreach (var script in facialScripts)
            {
                script.enabled = true;
            }
        }
        else
        {
            LipSyncMulti[] lipSyncScripts = FindObjectsOfType<LipSyncMulti>();
            foreach (var script in lipSyncScripts)
            {
                script.enabled = true;
            }
        }
    }

    /// <summary>
    /// Activate or desactivate a whole list of gameObjects
    /// </summary>
    /// <param name="list"> List to (de)activate</param>
    /// <param name="setActive"> True if list must be active, false is list must be unactive</param>
    private void SetActiveList(List<GameObject> list,bool setActive)
    {
        foreach (var head in list)
        {
            head.SetActive(setActive);
        }
    }

}
