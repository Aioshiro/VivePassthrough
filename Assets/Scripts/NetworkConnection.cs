using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;
using ViveHandTracking;

/// <summary>
/// Network manager to connect to server and properly initialize clients
/// </summary>
public class NetworkConnection : NetworkManager
{
    [Tooltip("Player rig game object")]
    [SerializeField] GameObject player;
    [Tooltip("Objects we want to give a player authority")]
    [SerializeField] List<NetworkIdentity> objectsToGiveAuthorityOn;
    /// <summary>
    /// Is this the first player to connect ?
    /// </summary>
    private bool firstPlayer = true;

    /// <summary>
    /// Client connections in case we need them
    /// </summary>
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
        networkAddress = GameManager.Instance.serverIp;
        if (networkAddress == "")
        {
            networkAddress = "localhost";
        }
        base.StartClient();
        player.SetActive(true);
        if (LipFrameWork != null)
        {
            LipFrameWork.EnableLip = true;
            LipFrameWork.StartFramework();
        }
        if (ViveSR.anipal.Eye.SRanipal_Eye_Framework.Instance != null)
        {
            ViveSR.anipal.Eye.SRanipal_Eye_Framework.Instance.EnableEye = true;
            ViveSR.anipal.Eye.SRanipal_Eye_Framework.Instance.StartFramework();
        }
        var gestureProvider = FindObjectOfType<GestureProvider>();
        if (gestureProvider != null)
        {
            gestureProvider.enabled = true;
        }
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

    /// <summary>
    /// Called on the server when the client's ready, send the player number to the client and give him object authority
    /// </summary>
    /// <param name="conn"></param>
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

    /// <summary>
    /// When client disconnect, remove its objects and authority
    /// </summary>
    /// <param name="conn"></param>
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
    /// <summary>
    /// Renders button on the UI to make a connection at a chosen ip
    /// </summary>
    private void OnGUI()
    {

        networkAddress = GUI.TextField(new Rect(0, 70, 200, 20),networkAddress);
        if (GUI.Button(new Rect(0, 100, 200, 20), "Connect"))
        {
            GameManager.Instance.RegisterHandler();
            base.StopClient();
            base.StartClient();
        }
    
    }
   
}
