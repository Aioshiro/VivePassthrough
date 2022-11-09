using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InstructionUpdater : MonoBehaviour
{
#if (UNITY_SERVER)
#else
    [SerializeField] List<string> instructions;
    bool initialized = false;
    private void Start()
    {
        if (GameManager.Instance.playerNumber >= 0)
        {
            UpdateInstructions(instructions[GameManager.Instance.playerNumber]);
            initialized = true;
        }
    }

    private void Update()
    {
        if (!initialized && GameManager.Instance.playerNumber >= 0)
        {
            UpdateInstructions(instructions[GameManager.Instance.playerNumber]);
            initialized = true;
        }
    }

    void UpdateInstructions(string text)
    {
        GetComponent<TMPro.TMP_Text>().text = text.Replace("\\", "\n"); //the replace is a workaround "\n" being desactivativated when input text to GUI
    }
#endif
}
