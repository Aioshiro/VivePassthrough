using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkConnection : NetworkManager
{
    [SerializeField] List<NetworkIdentity> objectsToGiveAuthorityOn;

    // Start is called before the first frame update
    override public void Start()
    {

        base.Start();
#if (UNITY_SERVER)
        base.StartServer();
        Destroy(GameObject.FindGameObjectWithTag("Player"));
#elif (UNITY_STANDALONE || UNITY_EDITOR)
        base.StartClient();
#endif
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        foreach (var obje in objectsToGiveAuthorityOn)
        {
            obje.AssignClientAuthority(conn);
        }
    }
}
