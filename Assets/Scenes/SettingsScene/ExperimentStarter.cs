using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Script to start experiment
/// </summary>
public class ExperimentStarter : NetworkBehaviour
{
    /// <summary>
    /// The singleton instance
    /// </summary>
    public static ExperimentStarter instance;

    [Tooltip("Is player one ready ?")]
    [SyncVar]
    public bool playerOneReady = false;

    [Tooltip("Is player two ready ?")]
    [SyncVar]
    public bool playerTwoReady = false;

    /// <summary>
    /// Has the experiment started ?
    /// </summary>
    bool startedExperiment = false;

    /// <summary>
    /// The instruction canvas text
    /// </summary>
    [SerializeField]
    TMPro.TMP_Text instructionsCanvasText;

    [Tooltip("Objects to enable when experiment is starting")]
    [SerializeField]
    List<GameObject> objectsToEnable;

    [Tooltip("Objects to disable when experiment is starting")]
    [SerializeField]
    List<GameObject> objectsToDisable;

    /// <summary>
    /// The countdown time until experiment start
    /// </summary>
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


    /// <summary>
    /// Client rpc to start experiment countdown
    /// </summary>
    [ClientRpc]
    private void RpcStartExperimentCountDown()
    {
        Debug.Log("Starting exp coroutine");
        if (!startedExperiment)
        {
            startedExperiment = true;
            StartCoroutine(nameof(ExperimentCountdown));
        }
    }

    /// <summary>
    /// Coroutine for experiment countdown
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Experiment starter, enables and disables objects and start mic record and chronometer
    /// </summary>
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
        //var micRecord = FindObjectOfType<OculusLipSyncMicInput>();
       // micRecord.StartMicrophoneRecord(micRecord.recordLength);
        FindObjectOfType<RegisterResults>().chronometer.StartChronometer();
    }


    /// <summary>
    /// Upload player ready boolean on server
    /// </summary>
    /// <param name="index"> Player number</param>
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
