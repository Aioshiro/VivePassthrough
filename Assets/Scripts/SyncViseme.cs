using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Sync the visemes of the players on the server
/// </summary>
public class SyncViseme : NetworkBehaviour
{
    /// <summary>
    /// Singleton isntance
    /// </summary>
    public static SyncViseme instance;
    /// <summary>
    /// OVRLipSyncContext, OculusLipSyncMicInput here
    /// </summary>
    private OVRLipSyncContextBase lipsyncContext = null;
    [Range(1, 100)]
    [Tooltip("Smoothing of 1 will yield only the current predicted viseme, 100 will yield an extremely smooth viseme response.")]
    public int smoothAmount = 70;

    /// <summary>
    /// Player one list of visemes
    /// </summary>
    public readonly SyncList<float> playerOneViseme = new SyncList<float>();

    /// <summary>
    /// Player two list of visemes
    /// </summary>
    public readonly SyncList<float> playerTwoViseme = new SyncList<float>();

    /// <summary>
    /// On server start, initialize syncLists of visemes
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        for (int i = 0; i < 15; i++)
        {
            playerOneViseme.Add(0);
            playerTwoViseme.Add(0);
        }
    }
    /// <summary>
    /// On client start, register callbacks and find OverLipSyncContextBase
    /// </summary>
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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        
    }

    void Update()
    {
        if (isClientOnly)
        {
            if (!GameManager.Instance.facialTracker)
            {
                if (lipsyncContext == null)
                {
                    lipsyncContext = FindObjectOfType<OVRLipSyncContextBase>();
                    return;
                }
                int playerNumber = GameManager.Instance.playerNumber;
                OVRLipSync.Frame frame = lipsyncContext.GetCurrentPhonemeFrame();
                //upload own viseme
                UpdateVisemes(playerNumber, frame.Visemes);
            }
        }
    }

    /// <summary>
    /// Upload Visemes on server
    /// </summary>
    /// <param name="index"> Player number</param>
    /// <param name="newValues"> New visemes values </param>
    [Command(requiresAuthority = false)]
    void UpdateVisemes(int index, float[] newValues)
    {
        if (index == 0)
        {
            for (int i = 0; i < 15; i++)
            {
                playerOneViseme[i] = newValues[i];
            }
        }
        else
        {
            for (int i = 0; i < 15; i++)
            {
                playerTwoViseme[i] = newValues[i];
            }
        }
    }

    /// <summary>
    /// Callback when player one has updated values
    /// </summary>
    /// <param name="op"></param>
    /// <param name="index"></param>
    /// <param name="oldItem"></param>
    /// <param name="newItem"></param>
    void OnPlayerOneUpdated(SyncList<float>.Operation op, int index, float oldItem, float newItem)
    {
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
                playerOneViseme[index] = newItem;
                break;
            case SyncList<float>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }
    /// <summary>
    /// Callback when player two has updated values
    /// </summary>
    /// <param name="op"></param>
    /// <param name="index"></param>
    /// <param name="oldItem"></param>
    /// <param name="newItem"></param>
    void OnPlayerTwoUpdated(SyncList<float>.Operation op, int index, float oldItem, float newItem)
    {
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
                playerTwoViseme[index] = newItem;
                break;
            case SyncList<float>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }
}
