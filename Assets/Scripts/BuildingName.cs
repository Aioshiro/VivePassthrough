using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingName : MonoBehaviour
{
    [SerializeField]
    private string englishName;

    [SerializeField]
    private string frenchName;

    // Start is called before the first frame update
    void Start()
    {
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
