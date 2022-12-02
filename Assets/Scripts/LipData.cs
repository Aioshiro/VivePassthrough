using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using Mirror;

/// <summary>
/// Script to synchronize data from facial tracker
/// </summary>
public class LipData : NetworkBehaviour
{

    /// <summary>
    /// The synchronized dictionnary for player one, it is automatically synced on clients when they change on server
    /// </summary>
    public readonly SyncDictionary<LipShape_v2, float> LipWeightingsFirstPlayer = new SyncDictionary<LipShape_v2, float>();

    /// <summary>
    /// The synchronized dictionnary for player two, it is automatically synced on clients when they change on server
    /// </summary>
    public readonly SyncDictionary<LipShape_v2, float> LipWeightingsSecondPlayer = new SyncDictionary<LipShape_v2, float>();

    /// <summary>
    /// On server start, intialiazing the dictionnaries
    /// </summary>
    public override void OnStartServer()
    {
        for (int i = 0; i < (int)LipShape_v2.Max - 1; i++)
        {
            LipWeightingsFirstPlayer.Add((LipShape_v2)i, 0);
            LipWeightingsSecondPlayer.Add((LipShape_v2)i, 0);
        }
    }

    //Dictionnary are automatically synced, no need to initialized on client
    //public override void OnStartClient()
    //{
    //LipWeightingsFirstPlayer.Callback += OnFirstPlayerChange;
    //LipWeightingsSecondPlayer.Callback += OnSecondPlayerChange;

    //}

    public void Update()
    {
        if (!SRanipal_Lip_Framework.Instance.EnableLip)
        {
            return;
        }

        SRanipal_Lip_v2.GetLipWeightings(out var weightings);
        //Uploading facial tracker to server
        UpdateWeightings(weightings, GameManager.Instance.playerNumber);
    }

    /// <summary>
    /// Uploading the new lip data on the server
    /// </summary>
    /// <param name="dict"> Lip weightings</param>
    /// <param name="playerNumber"> Player number</param>
    public void UpdateWeightings(Dictionary<LipShape_v2,float> dict, int playerNumber)
    {
        foreach (KeyValuePair<LipShape_v2, float> kvp in dict)
        {
            UpdateDictionnaryOnServer(kvp.Key,kvp.Value, playerNumber);
        }

    }

    /// <summary>
    /// Uploading individual (key,value) tuples on server.
    /// <para> Upload is made on server to be authorized and sync to clients </para>
    /// Upload is made for each tuple individually as Dictionary is not a supported type in [Command] functions
    /// </summary>
    /// <param name="key"> Dictionnary key</param>
    /// <param name="value"> New dictionnary value</param>
    /// <param name="playerNumber"> Player number</param>
    [Command(requiresAuthority =false)]
    private void UpdateDictionnaryOnServer(LipShape_v2 key, float value, int playerNumber)
    {
        if (playerNumber == 0)
        {
            LipWeightingsFirstPlayer[key] = value;
        }
        else
        {
            LipWeightingsSecondPlayer[key] = value;
        }
    }


    /// <summary>
    /// Callback when player one dictionnary changes
    /// </summary>
    /// <param name="op"></param>
    /// <param name="key"></param>
    /// <param name="item"></param>
    void OnFirstPlayerChange(SyncDictionary<LipShape_v2, float>.Operation op, LipShape_v2 key, float item)
    {
        switch (op)
        {
            case SyncDictionary<LipShape_v2, float>.Operation.OP_ADD:

                break;
            case SyncDictionary<LipShape_v2, float>.Operation.OP_SET:

                break;
            case SyncDictionary<LipShape_v2, float>.Operation.OP_REMOVE:

                break;
            case SyncDictionary<LipShape_v2, float>.Operation.OP_CLEAR:

                break;
        }
    }

    /// <summary>
    /// Callback when player two dictionnary changes
    /// </summary>
    /// <param name="op"></param>
    /// <param name="key"></param>
    /// <param name="item"></param>
    void OnSecondPlayerChange(SyncDictionary<LipShape_v2, float>.Operation op, LipShape_v2 key, float item)
    {
        switch (op)
        {
            case SyncDictionary<LipShape_v2, float>.Operation.OP_ADD:
                
                break;
            case SyncDictionary<LipShape_v2, float>.Operation.OP_SET:

                break;
            case SyncDictionary<LipShape_v2, float>.Operation.OP_REMOVE:

                break;
            case SyncDictionary<LipShape_v2, float>.Operation.OP_CLEAR:

                break;
        }
    }

}
