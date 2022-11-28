using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExperimentEnder : NetworkBehaviour
{
    [SyncVar]
    public bool playerOneFinished = false;
    [SyncVar]
    public bool playerTwoFinished = false;


    public void TogglePlayerAsFinished()
    {
        Cmd_TogglePlayerAsFinished(GameManager.Instance.playerNumber);
    }

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

    [ClientRpc]
    void Rpc_EndExperiment()
    {
        FindObjectOfType<RegisterResults>().Save();
        FindObjectOfType<OculusLipSyncMicInput>().EndMicrophoneRecord();
        Application.Quit();
    }
}
