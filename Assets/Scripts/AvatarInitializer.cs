using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInitializer : MonoBehaviour
{

    [SerializeField] List<GameObject> maleHeads;
    [SerializeField] List<GameObject> womanHeads;
    [SerializeField] List<GameObject> cartoonHeads;
    [SerializeField] List<GameObject> realisticHeads;
    [SerializeField] GameObject africanHeads;
    [SerializeField] GameObject asianHeads;
    [SerializeField] GameObject caucasianHeads;
    [SerializeField] GameObject latinoHeads;

    private void Start()
    {
        SetActiveList(maleHeads, false);
        SetActiveList(womanHeads, false);
        SetActiveList(cartoonHeads,false);
        SetActiveList(realisticHeads,false);
        africanHeads.SetActive(false);
        asianHeads.SetActive(false);
        caucasianHeads.SetActive(false);
        latinoHeads.SetActive(false);
        InitializeHeads();
    }

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
    }


    private void SetActiveList(List<GameObject> list,bool setActive)
    {
        foreach (var head in list)
        {
            head.SetActive(setActive);
        }
    }

}
