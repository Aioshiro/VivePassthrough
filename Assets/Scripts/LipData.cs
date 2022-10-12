using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using Mirror;

public class LipData : NetworkBehaviour
{

    //The synchronized dictionnary for each player, they are automatically synced on clients when they change on server
    public readonly SyncDictionary<LipShape_v2, float> LipWeightingsFirstPlayer = new SyncDictionary<LipShape_v2, float>();
    public readonly SyncDictionary<LipShape_v2, float> LipWeightingsSecondPlayer = new SyncDictionary<LipShape_v2, float>();



    //on server start, intialiazing the dictionnaries
    public override void OnStartServer()
    {
        for (int i = 0; i < (int)LipShape_v2.Max - 1; i++)
        {
            LipWeightingsFirstPlayer.Add((LipShape_v2)i, 0);
            LipWeightingsSecondPlayer.Add((LipShape_v2)i, 0);
        }
    }

    //In case we need to do other things on dictionnary change
    //Dictionnary are automatically synced, no need to initialized on client
    //public override void OnStartClient()
    //{
        //LipWeightingsFirstPlayer.Callback += OnFirstPlayerChange;
        //LipWeightingsSecondPlayer.Callback += OnSecondPlayerChange;

    //}

    //Updating new lip data
    public void UpdateWeightings(Dictionary<LipShape_v2,float> dict, int playerNumber)
    {
        foreach (KeyValuePair<LipShape_v2, float> kvp in dict)
        {
            UpdateDictionnaryOnServer(kvp.Key,kvp.Value, playerNumber);
        }

    }

    //Uploading individual (key,value) tuples on server
    //upload is made on server to be authorized and sync to clients
    //Upload is made for each tuple individually as Dictionary is not a supported type in [Command] functions
    [Command]
    public void UpdateDictionnaryOnServer(LipShape_v2 key, float value, int playerNumber)
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


    //In case we need to do additional things on dictionnary change
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
