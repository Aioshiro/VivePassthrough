using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;
using ViveHandTracking;

public class NetworkConnection : NetworkManager
{
    [SerializeField] GameObject player;
    [SerializeField] List<NetworkIdentity> objectsToGiveAuthorityOn;
    private bool firstPlayer = true;

    private NetworkConnectionToClient[] clientConn;

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        var LipFrameWork = FindObjectOfType<ViveSR.anipal.Lip.SRanipal_Lip_Framework>();
#if (UNITY_SERVER)
        //base.StartServer(); //If autostart is not activated in the networkManager
        //Destroy(GameObject.FindGameObjectWithTag("Player"));
        //Destroy(LipFrameWork.gameObject);
        clientConn = new NetworkConnectionToClient[2];
#elif (UNITY_STANDALONE || UNITY_EDITOR)//If it's the client, we enable the lip framework
        base.StartClient();
        player.SetActive(true);
        //LipFrameWork.EnableLip = true;
        //LipFrameWork.StartFramework();
        var gestureProvider = FindObjectOfType<GestureProvider>();
        gestureProvider.enabled = true;
#endif
    }


    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Client connected");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.LogWarning("Client disconnected");
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
            GameManager.PlayerInfo info = new GameManager.PlayerInfo
            {
                playerNumber = 0
            };
            conn.Send(info);
            firstPlayer = false;
            Debug.Log("Assigned player 0");
            clientConn[0] = conn;
        }
        else
        {
            GameManager.PlayerInfo info = new GameManager.PlayerInfo
            {
                playerNumber = 1
            };
            conn.Send(info);
            Debug.Log("Assigned player 1");
            clientConn[1] = conn;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        var ownedObjects = new NetworkIdentity[conn.clientOwnedObjects.Count];
        conn.clientOwnedObjects.CopyTo(ownedObjects);
        foreach (var networkIdentity in ownedObjects)
        {
            if (!networkIdentity.CompareTag("Player"))
            {
                networkIdentity.RemoveClientAuthority();
            }
        }
        base.OnServerDisconnect(conn);
        if (clientConn[0] == conn)
        {
            clientConn[0] = null;
            firstPlayer = true;
        }
        else
        {
            clientConn[1] = null;
            firstPlayer = false;
        }

    }
}
