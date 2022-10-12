using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkConnection : NetworkManager
{
    [SerializeField] List<NetworkIdentity> objectsToGiveAuthorityOn;
    private bool firstPlayer = true;

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        var LipFrameWork = FindObjectOfType<ViveSR.anipal.Lip.SRanipal_Lip_Framework>();
        //If it's the server, we destroy the lip framework (no need)
        //Having the lip framework enabled on the server makes it crash
#if (UNITY_SERVER)
        base.StartServer();
        Destroy(GameObject.FindGameObjectWithTag("Player"));
        Destroy(LipFrameWork.gameObject);
#elif (UNITY_STANDALONE || UNITY_EDITOR)//If it's the client, we enable the lip framework
        base.StartClient();
        LipFrameWork.EnableLip = true;
        LipFrameWork.StartFramework();
#endif
    }


    //Called on the server when the client's ready
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        //Giving authority of client on objects it can change (markers and lip/eye data mainly)
        foreach (var obje in objectsToGiveAuthorityOn)
        {
            obje.AssignClientAuthority(conn);
        }
        //Assigning player numbers so we know who's who
        if (firstPlayer)
        {
            AvatarLipMulti.PlayerInfo info = new AvatarLipMulti.PlayerInfo
            {
                playerNumber = 0
            };
            conn.Send(info);
            firstPlayer = false;
        }
        else
        {
            AvatarLipMulti.PlayerInfo info = new AvatarLipMulti.PlayerInfo
            {
                playerNumber = 1
            };
            conn.Send(info);
        }
    }
}
