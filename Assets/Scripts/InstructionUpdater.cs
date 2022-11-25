using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InstructionUpdater : MonoBehaviour
{
#if (UNITY_SERVER)
#else
    [SerializeField] List<string> instructions;
    [SerializeField] List<string> englishInstructions;
    bool initialized = false;
    private void Start()
    {
        if (GameManager.Instance.playerNumber >= 0)
        {
            UpdateInstructions(GameManager.Instance.languageSetToEnglish);
            initialized = true;
        }
    }

    private void Update()
    {
        if (!initialized && GameManager.Instance.playerNumber >= 0)
        {
            UpdateInstructions(GameManager.Instance.languageSetToEnglish);
            initialized = true;
        }
    }

    void UpdateInstructions(bool languageIsEnglish)
    {
        if (languageIsEnglish)
        {
            UpdateInstructions(englishInstructions);
        }
        else
        {
            UpdateInstructions(instructions);
        }
    }

    void UpdateInstructions(List<string> input)
    {
        TMPro.TMP_Text shownText = GetComponent<TMPro.TMP_Text>();
        int start = 0;
        if (GameManager.Instance.playerNumber == 1) { start = 5; }
        for (int i = 0; i < 5; i++)
        {
            shownText.text += input[start + i] + "\n";
        }
    }
#endif
}
