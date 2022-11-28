using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExperimentStarter : NetworkBehaviour
{

    public static ExperimentStarter instance;
    [SyncVar]
    public bool playerOneReady = false;
    [SyncVar]
    public bool playerTwoReady = false;
    bool startedExperiment = false;
    [SerializeField]
    TMPro.TMP_Text instructionsCanvasText;

    [SerializeField]
    List<GameObject> objectsToEnable;
    [SerializeField]
    List<GameObject> objectsToDisable;

    int timeUntilExperimentStart = 15;

    private void Awake()
    {
        if (ExperimentStarter.instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (!this.isServer) { return; }
        if (playerOneReady && playerTwoReady && !startedExperiment)
        {
            startedExperiment = true;
            Debug.Log("Starting exp on clients");
            RpcStartExperimentCountDown();
            this.enabled = false;
        }
    }

    [ClientRpc]
    private void RpcStartExperimentCountDown()
    {
        Debug.Log("Starting exp coroutine");
        StartCoroutine(nameof(ExperimentCountdown));
    }

    IEnumerator ExperimentCountdown()
    {
        Debug.Log("Starting exp countdown");
        instructionsCanvasText.fontSize *= 2;
        while (timeUntilExperimentStart > 0)
        {
            if (GameManager.Instance.languageSetToEnglish)
            {
                instructionsCanvasText.text = "The experiment will start in " + timeUntilExperimentStart.ToString() + " seconds.";
            }
            else
            {
                instructionsCanvasText.text = "L'expérience débutera dans " + timeUntilExperimentStart.ToString() + " secondes.";
            }
            timeUntilExperimentStart -= 1;
            yield return new WaitForSeconds(1);
        }
        StartExperiment();
    }
    private void StartExperiment()
    {
        foreach(var obje in objectsToEnable)
        {
            obje.SetActive(true);
        }
        foreach(var obje in objectsToDisable)
        {
            obje.SetActive(false);
        }
        FindObjectOfType<RegisterResults>().chronometer.StartChronometer();
    }

    [Command(requiresAuthority =false)]
    public void SetPlayerReady(int index)
    {
        if (index == 0)
        {
            playerOneReady = true;
        }
        else
        {
            playerTwoReady = true;
        }
    }
}
