using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInitializer : MonoBehaviour
{

    [SerializeField] List<GameObject> maleHeads;
    [SerializeField] List<GameObject> womanHeads;
    [SerializeField] GameObject cartoonHeads;
    [SerializeField] GameObject realisticHeads;

    private void Start()
    {
        SetActiveList(maleHeads, false);
        SetActiveList(womanHeads, false);
        cartoonHeads.SetActive(false);
        realisticHeads.SetActive(false);
        InitializeHeads();
    }

    private void InitializeHeads()
    {
        if (!GameManager.Instance.HeadsActive) { return; }

        if (GameManager.Instance.isCartoon) { cartoonHeads.SetActive(true); }
        else { realisticHeads.SetActive(true); }

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
