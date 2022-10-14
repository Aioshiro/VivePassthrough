using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RegisterHandlers : MonoBehaviour
{

    [SerializeField] List<AvatarLipMulti> LipScripts;

#if (UNITY_SERVER)
#else
    //Registering handlers on awake, with prefilled list, to make sure they are initialized before client connects to server
    private void Awake()
    {
        NetworkClient.RegisterHandler<AvatarLipMulti.PlayerInfo>(GetPlayerNumber);
    }

    private void GetPlayerNumber(AvatarLipMulti.PlayerInfo info)
    {
        foreach(var lip in LipScripts)
        {
            lip.GetPlayerNumber(info.playerNumber);
        }
    }

#endif
}
