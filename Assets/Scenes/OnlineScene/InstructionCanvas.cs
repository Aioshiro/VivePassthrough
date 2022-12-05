using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

/// <summary>
/// Instruction canvas for before experiment start
/// </summary>
public class InstructionCanvas : MonoBehaviour
{
    /// <summary>
    /// Canvas transform (in world space)
    /// </summary>
    RectTransform canvasTransform;

    [Tooltip("Text to update")]
    [SerializeField]
    TMPro.TMP_Text text;

    [Tooltip("The image to fill")]
    [SerializeField]
    Image fillingImage;

    [Tooltip("Marker 10's image")]
    [SerializeField]
    Image markerImage;

    [Tooltip("Marker 10's TransformSmoother")]
    [SerializeField]
    TransformSmoother mapMarker;

    private void Start()
    {
        canvasTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        fillingImage.transform.SetPositionAndRotation(mapMarker.transform.position, mapMarker.transform.rotation);
        fillingImage.fillAmount = (float) mapMarker.count /  (float) mapMarker.movingAverageLengthPos;
        if (mapMarker.count==mapMarker.movingAverageLengthPos)
        {
            fillingImage.enabled = false;
            markerImage.enabled = false;
            this.transform.parent = null;
            canvasTransform.SetPositionAndRotation(mapMarker.transform.position, mapMarker.transform.rotation);
            canvasTransform.transform.localScale = new Vector3(-1, 1, 1); //flipping x to have panel in right orientation
            if (GameManager.Instance.languageSetToEnglish)
            {
                text.text = "Waiting for second participant";
            }
            else
            {
                text.text = "En attente du second participant...";
            }
            ExperimentStarter.instance.SetPlayerReady(GameManager.Instance.playerNumber);
            this.enabled = false;
        }
    }
}
