using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


/// <summary>
/// Script to update instructions for the task
/// </summary>
public class InstructionUpdater : MonoBehaviour
{
#if (UNITY_SERVER)
#else
    [Tooltip("List of instructions in French")]
    [SerializeField] List<string> instructions;
    [Tooltip("List of instructions in English")]
    [SerializeField] List<string> englishInstructions;
    /// <summary>
    /// Has instruction been initialized ?
    /// </summary>
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
        if (initialized)
        {
            this.enabled = false;
        }
    }

    /// <summary>
    /// Chooses the right language instructions
    /// </summary>
    /// <param name="languageIsEnglish"></param>
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

    /// <summary>
    /// Updates panel with instructions given
    /// </summary>
    /// <param name="input"> List of string instructions</param>
    void UpdateInstructions(List<string> input)
    {
        TMPro.TMP_Text shownText = GetComponent<TMPro.TMP_Text>();
        shownText.text = "";
        int start = 0;
        if (GameManager.Instance.playerNumber == 1) { start = instructions.Count/2; }
        for (int i = 0; i < instructions.Count / 2; i++)
        {
            shownText.text += input[start + i] + "\n";
        }
    }
#endif
}
