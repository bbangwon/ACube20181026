using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : NetworkBehaviour
{
    public enum color {Blue, Green, Red, Yellow};

    [SyncVar]
    float hp = 10f;
    public int[] gage = new int[4];

    public int TeamNumber; 
    
	void Start () {

        if (transform.position.z >= 10) TeamNumber = 1;
        else TeamNumber = 2;
	}

    [ServerCallback]
    void Update()
    {
        RpcSetGage(gage);
        if (gage[(int)color.Red] >= 25)
        {
            transform.GetComponent<PlayerMovement>().floor.RedAttack();
            gage[(int)color.Red] = 0;
        }

    }
    
    [ClientRpc]
    void RpcSetGage(int[] rpcGage)
    {
        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(1).GetChild(0).GetComponent<GageManager>().sliders[i].value = rpcGage[i];
        }
    }
    
    void SetHPBar()
    {
        transform.GetChild(1).GetChild(0).GetComponent<GageManager>().SetHPBar(hp);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cube")
        {
            Block blockAttack = other.transform.GetComponent<Block>();
            if (blockAttack.TeamNumber != TeamNumber)
            {
                if (hp > 0) hp -= 1;
                if (hp <= 0)
                {
                    RpcClientDead();
                }

                SetHPBar();
            }
        }
    }

    [ClientRpc]
    void RpcClientDead()
    {
        Destroy(gameObject);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player != gameObject && player.GetComponent<PlayerHealth>().TeamNumber == TeamNumber)
            {
                return;
            }
        }

        EndGame();
    }
    
    void EndGame()
    {
        GameObject.Find("NetworkManager").GetComponent<ConnectNetworkManager>().EndGame();
    }

}
