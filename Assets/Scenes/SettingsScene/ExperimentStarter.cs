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

    const int NUMBER_OF_TASKS = 2;

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

    [Tooltip("Objects to enable when experiment 1 is starting")]
    [SerializeField]
    List<GameObject> objectsToEnableTask1;

    [Tooltip("Objects to disable when experiment 1 is starting")]
    [SerializeField]
    List<GameObject> objectsToDisableTask1;


    [Tooltip("Objects to enable when experiment 2 is starting")]
    [SerializeField]
    List<GameObject> objectsToEnableTask2;

    [Tooltip("Objects to disable when experiment 2 is starting")]
    [SerializeField]
    List<GameObject> objectsToDisableTask2;

    /// <summary>
    /// The countdown time until experiment start
    /// </summary>
    int timeUntilExperimentStart = 15;

    public bool endAfterTimeHasPassed = true;
    private Chronometer chrono;
    public float timeOfExperiment = 60 * 5;//5 min

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

    private void Start()
    {
        chrono = new Chronometer();
    }

    private void Update()
    {
        if (this.isServer)
        {
            if (playerOneReady && playerTwoReady)
            {
                GameManager.Instance.currentTask += 1;
                startedExperiment = true;
                Debug.Log($"Starting exp {GameManager.Instance.currentTask} on clients");
                playerOneReady = false;
                playerTwoReady = false;
                RpcStartExperimentCountDown();
            }
        }
        else
        {
            if (startedExperiment && endAfterTimeHasPassed && chrono.GetChronometerTime() > timeOfExperiment)
            {
                Debug.Log("finishing experiment");
                FindObjectOfType<ExperimentEnder>().TogglePlayerAsFinished();
                //this.enabled = false;
            }
        }
    }


    /// <summary>
    /// Client rpc to start experiment countdown
    /// </summary>
    [ClientRpc]
    private void RpcStartExperimentCountDown()
    {
        GameManager.Instance.currentTask += 1;
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
                instructionsCanvasText.text = $"The task {GameManager.Instance.currentTask} will start in " + timeUntilExperimentStart.ToString() + " seconds.";
            }
            else
            {
                instructionsCanvasText.text = $"La tâche {GameManager.Instance.currentTask} débutera dans " + timeUntilExperimentStart.ToString() + " secondes.";
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
        if (GameManager.Instance.currentTask == 1)
        {
            foreach (var obje in objectsToEnableTask1)
            {
                obje.SetActive(true);
            }
            foreach (var obje in objectsToDisableTask1)
            {
                obje.SetActive(false);
            }
        }
        else
        {
            foreach (var obje in objectsToEnableTask2)
            {
                obje.SetActive(true);
            }
            foreach (var obje in objectsToDisableTask2)
            {
                obje.SetActive(false);
            }
        }
        //var micRecord = FindObjectOfType<OculusLipSyncMicInput>();
       // micRecord.StartMicrophoneRecord(micRecord.recordLength);
        FindObjectOfType<RegisterResults>().chronometer.StartChronometer();
        chrono.StartChronometer();
    }


    /// <summary>
    /// Upload player ready boolean on server
    /// </summary>
    /// <param name="index"> Player number</param>
    [Command(requiresAuthority =false)]
    public void SetPlayerReady(int index,bool value)
    {
        if (index == 0)
        {
            playerOneReady = value;
        }
        else
        {
            playerTwoReady = value;
        }
    }
}
