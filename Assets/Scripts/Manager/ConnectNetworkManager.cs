using UnityEngine.Networking;

public class ConnectNetworkManager : NetworkManager {

    public void EndGame()
    {
        StopClient();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        StopServer();
    }
}
