using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingName : MonoBehaviour
{
    [Tooltip("English name of the building")]
    [SerializeField]
    private string englishName;

    [Tooltip("French name of the building")]
    [SerializeField]
    private string frenchName;

    void Start()
    {
        //Setting up the name with the right language
        if (GameManager.Instance.languageSetToEnglish)
        {
            GetComponent<TMPro.TMP_Text>().text = englishName;
        }
        else
        {
            GetComponent<TMPro.TMP_Text>().text = frenchName;
        }
    }


}
