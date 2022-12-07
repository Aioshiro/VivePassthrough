using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Ends the experiment
/// </summary>
public class ExperimentEnder : NetworkBehaviour
{
    [Tooltip("Does player one considers the experiment finished ?")]
    [SyncVar]
    public bool playerOneFinished = false;

    [Tooltip("Does player one considers the experiment finished ?")]
    [SyncVar]
    public bool playerTwoFinished = false;

    /// <summary>
    /// Called on local clients to tell server that the player considers the experiment finished
    /// </summary>
    public void TogglePlayerAsFinished()
    {
        Cmd_TogglePlayerAsFinished(GameManager.Instance.playerNumber);
    }

    /// <summary>
    /// Updates the booleans of player finished on server, and call for end of experiment on client if both are finished
    /// </summary>
    /// <param name="index"> Player number</param>
    [Command(requiresAuthority =false)]
    public void Cmd_TogglePlayerAsFinished(int index)
    {
        if (index == 0)
        {
            playerOneFinished = !playerOneFinished;
        }
        else
        {
            playerTwoFinished = !playerTwoFinished;
        }
        if (playerOneFinished && playerTwoFinished)
        {
            Rpc_EndExperiment();
        }
    }


    /// <summary>
    /// Ends experiment by saving eye gaze data and microphone conversation, then quits
    /// </summary>
    [ClientRpc]
    void Rpc_EndExperiment()
    {
        FindObjectOfType<RegisterResults>().Save();
        //FindObjectOfType<OculusLipSyncMicInput>().EndMicrophoneRecord();
        Application.Quit();
    }
}
