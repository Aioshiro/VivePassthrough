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


    public void SetPlayerAsFinished()
    {
        Cmd_SetPlayerAsFinished(GameManager.Instance.playerNumber);
    }

    [Command(requiresAuthority =false)]
    public void Cmd_SetPlayerAsFinished(int index)
    {
        if (index == 0)
        {
            playerOneFinished = true;
        }
        else
        {
            playerTwoFinished = true;
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
        Application.Quit();
    }
}
