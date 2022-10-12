using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RegisterHandlers : MonoBehaviour
{

    [SerializeField] List<AvatarLipMulti> LipScripts;

    private void Awake()
    {
        foreach (var lip in LipScripts)
        {
            NetworkClient.RegisterHandler<AvatarLipMulti.PlayerInfo>(lip.GetPlayerNumber);
        }
    }

    
}
