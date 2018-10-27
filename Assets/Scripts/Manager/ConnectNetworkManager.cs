using UnityEngine.Networking;

public class ConnectNetworkManager : NetworkManager {

    public enum Mode { DeathMatch, Sheld };

    public Mode mode;

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
