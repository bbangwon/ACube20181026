using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Block : NetworkBehaviour{
    public enum State { Idle, Attack, Defence };
    
    // 블록 재질.
    [SyncVar]
    public int blockType;
    public List<Material> blockMaterials;

    // 똑같은 블록이 있는지 체크.
    public bool isMatchChecked = false;
    public int matchIndex;

    // 어느팀 블록.
    public int TeamNumber;

    public State state = State.Idle;

    [ClientRpc]
    public void RpcSetBlock()
    {
        GetComponent<Block>().GetComponent<MeshRenderer>().material = blockMaterials[blockType];
    }

    public void Reset()
    {
        isMatchChecked = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cube")
        {
            Block blockAttack = other.transform.GetComponent<Block>();
            if (blockAttack.TeamNumber != TeamNumber)
            {
                switch(state)
                {
                    case State.Defence:
                        Destroy(other.gameObject);
                        Destroy(gameObject);
                        break;
                    case State.Attack:
                        Destroy(gameObject);
                        break;
                    default:
                        break;

                }
            }
        }
    }
}
