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
            canvasTransform.SetPositionAndRotation(playerRig.position + playerRig.transform.forward * 0.5f, playerRig.rotation);
            positionned = true;
        }
        fillingImage.fillAmount = (float) mapMarker.count /  (float) mapMarker.movingAverageLengthPos;
        if (fillingImage.fillAmount > 0.99)
        {
            fillingImage.enabled = false;
            markerImage.enabled = false;
            text.text = "En attente du second participant...";
            ExperimentStarter.instance.SetPlayerReady(GameManager.Instance.playerNumber);
            this.enabled = false;
        }
    }
}
