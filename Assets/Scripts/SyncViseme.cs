using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncViseme : NetworkBehaviour
{

    public static SyncViseme instance;
    private OVRLipSyncContextBase lipsyncContext = null;
    [Range(1, 100)]
    [Tooltip("Smoothing of 1 will yield only the current predicted viseme, 100 will yield an extremely smooth viseme response.")]
    public int smoothAmount = 70;


    readonly public SyncList<float> playerOneViseme = new SyncList<float>();
    readonly public SyncList<float> playerTwoViseme = new SyncList<float>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        for (int i = 0; i < 15; i++)
        {
            playerOneViseme.Add(0);
            playerTwoViseme.Add(0);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerOneViseme.Callback += OnPlayerOneUpdated;
        playerTwoViseme.Callback += OnPlayerTwoUpdated;
        lipsyncContext = FindObjectOfType<OVRLipSyncContextBase>();
        if (lipsyncContext == null)
        {
            Debug.LogError("SyncViseme.Start Error: " +
                "No OVRLipSyncContext component in scene!");
        }
        else
        {
            // Send smoothing amount to context
            lipsyncContext.Smoothing = smoothAmount;
        }
    }

    private void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (isClientOnly)
        {
            int playerNumber = GameManager.Instance.playerNumber;
            OVRLipSync.Frame frame = lipsyncContext.GetCurrentPhonemeFrame();
            //upload own viseme
            UpdateVisemes(playerNumber, frame.Visemes);
            Debug.Log(playerOneViseme[5]);
        }
    }

    [Command(requiresAuthority = false)]
    void UpdateVisemes(int index, float[] newValues)
    {
        if (index == 0)
        {
            for (int i = 0; i < 15; i++)
            {
                playerOneViseme[i] = newValues[i];
            }
            Debug.Log(playerOneViseme[5]);
        }
        else
        {
            for (int i = 0; i < 15; i++)
            {
                playerTwoViseme[i] = newValues[i];
            }
        }
    }

    void OnPlayerOneUpdated(SyncList<float>.Operation op, int index, float oldItem, float newItem)
    {
        if (GameManager.Instance.playerNumber == 1) { Debug.Log("Updated other player head"); }
        switch (op)
        {
            case SyncList<float>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                break;
            case SyncList<float>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<float>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<float>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<float>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }

    void OnPlayerTwoUpdated(SyncList<float>.Operation op, int index, float oldItem, float newItem)
    {
        if (GameManager.Instance.playerNumber == 0) { Debug.Log("Updated other player head"); }
        switch (op)
        {
            case SyncList<float>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                break;
            case SyncList<float>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<float>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<float>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<float>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }
}
