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
#if (UNITY_SERVER)
        base.StartServer();
        Destroy(GameObject.FindGameObjectWithTag("Player"));
        Destroy(LipFrameWork.gameObject);
#elif (UNITY_STANDALONE || UNITY_EDITOR)
        base.StartClient();
        LipFrameWork.EnableLip = true;
        LipFrameWork.StartFramework();
#endif
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        foreach (var obje in objectsToGiveAuthorityOn)
        {
            obje.AssignClientAuthority(conn);
        }
        if (firstPlayer)
        {
            AvatarLipMulti.PlayerInfo info = new AvatarLipMulti.PlayerInfo
            {
                playerNumber = 0
            };
            conn.Send(info);
            firstPlayer = false;
        }
        //else
        //{
        //    AvatarLipMulti.PlayerInfo info = new AvatarLipMulti.PlayerInfo
        //    {
        //        playerNumber = 1
        //    };
        //    conn.Send(info);
        //}
    }
}
