using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkHUDManager : MonoBehaviour {

    ConnectNetworkManager ConnNetManager;
    void Start()
    {
        ConnNetManager = GameObject.Find("NetworkManager").GetComponent<ConnectNetworkManager>();
    }

    public void OpenServer()
    {
        ConnNetManager.StartServer();
    }

    public void OpenHost()
    {
        ConnNetManager.StartHost();
    }

    public void ConnectClientToServer()
    {
        ConnNetManager.StartClient();
    }
}
