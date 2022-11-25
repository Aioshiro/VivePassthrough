using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class InstructionCanvas : MonoBehaviour
{
    bool positionned = false;
    [SerializeField]
    Transform playerRig;
    RectTransform canvasTransform;
    [SerializeField]
    TMPro.TMP_Text text;
    [SerializeField]
    Image fillingImage;
    [SerializeField]
    Image markerImage;
    [SerializeField]
    TransformSmoother mapMarker;
    


    private void Start()
    {
        canvasTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!positionned && playerRig.position!= Vector3.zero)
        {
            Vector3 forward = Vector3.ProjectOnPlane(playerRig.transform.forward, Vector3.up);
            canvasTransform.SetPositionAndRotation(playerRig.position + forward * 0.5f, Quaternion.Euler(0, playerRig.rotation.eulerAngles.y,0));
            positionned = true;
        }
        fillingImage.transform.position = mapMarker.transform.position;
        fillingImage.fillAmount = (float) mapMarker.count /  (float) mapMarker.movingAverageLengthPos;
        if (fillingImage.fillAmount > 0.99)
        {
            fillingImage.enabled = false;
            markerImage.enabled = false;
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
