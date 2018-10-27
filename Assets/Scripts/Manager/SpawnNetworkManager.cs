using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnNetworkManager : NetworkManager
{
    void Awake()
    {
        GameObject prefab = Resources.Load<GameObject>("Floor");
        spawnPrefabs.Add(prefab);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        GameObject floor = Instantiate(spawnPrefabs[0]);
        NetworkServer.Spawn(floor);
    }
}
