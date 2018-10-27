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
        if (gage[(int)color.Red] >= 1)
        {
            transform.GetComponent<PlayerMovement>().floor.RedAttack();
            gage[(int)color.Red] = 0;
        }
    }
    
    void SetHPBar()
    {
        GageManager.Instance.SetHPBar(hp);
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
