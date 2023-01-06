using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwentyQuestionsTextUpdater : MonoBehaviour
{

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
        TMPro.TMP_Text shownText = GetComponent<TMPro.TMP_Text>();
        if (languageIsEnglish)
        {
            shownText.text = "Play the 20 questions game, one has a secret word and the other asks up to 20 closed questions (yes/no) to find it, then do it again by reversing roles.";
            shownText.text += $" Your word is : {GameManager.Instance.TwentyQuestionsWord}.";
        }
        else
        {
            shownText.text = "Jouez au jeu des 20 questions, l'un a un mot secret et l'autre lui pose jusqu'à 20 questions fermées (oui/non) pour le trouver, puis refaites le en inversant les rôles.";
            shownText.text += $" Votre mot est : {GameManager.Instance.TwentyQuestionsWord}.";
        }
        if (GameManager.Instance.playerNumber == 0)
        {
            if (languageIsEnglish)
            {
                shownText.text += " You start asking questions !";
            }
            else
            {
                shownText.text += " Vous commencez à poser des questions !";
            }
        }
        else
        {
            if (languageIsEnglish)
            {
                shownText.text += " Your partner starts asking questions.";
            }
            else
            {
                shownText.text += " Votre partenaire commence à poser des questions.";
            }
        }
    }


}
