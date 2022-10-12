using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RegisterHandlers : MonoBehaviour
{

    [SerializeField] List<AvatarLipMulti> LipScripts;

    //Registering handlers on awake, with prefilled list, to make sure they are initialized before client connects to server
    private void Awake()
    {
        foreach (var lip in LipScripts)
        {
            NetworkClient.RegisterHandler<AvatarLipMulti.PlayerInfo>(lip.GetPlayerNumber);
        }
    }

    
}
