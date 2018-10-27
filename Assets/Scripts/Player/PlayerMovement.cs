using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerMovement : NetworkBehaviour {

    int width = 16;
    int height = 8;

    Vector3 movement;
    public Floor floor;
    public GameObject floorPrefab;
    Vector3 originPos;

    [Command]
    void CmdSpawnfloor()
    {
        GameObject newFloor = (GameObject)Instantiate(floorPrefab, originPos, Quaternion.identity);
        NetworkServer.Spawn(newFloor);

        floor = newFloor.GetComponent<Floor>();
        width = floor.width;
        height = floor.height;
    }

    [Command]
    void CmdSetOriginPos()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Cube");
        if (obj != null)
        {
            foreach (GameObject go in obj)
                go.GetComponent<Block>().RpcSetBlock();
        }

        if (transform.position.z >= 10) originPos = new Vector3(0, 0, 10f);
        else originPos = new Vector3(0, 0, 0);


    }

    [Command]
    void CmdSetFloor()
    {
        GameObject[] stages = GameObject.FindGameObjectsWithTag("Floor");
        foreach (GameObject stage in stages)
        {
            floor = stage.GetComponent<Floor>();

            if (stage.transform.position.z >= 10f && transform.position.z >= 10f)
            {
                break;
            }
            else if (stage.transform.position.z == 0f && transform.position.z < 10f)
            {
                break;
            }
        }
        width = floor.width;
        height = floor.height;
    }

    // Use this for initialization
    [ClientCallback]
    void Start ()
    {
        CmdSetOriginPos();

        GameObject[] stages = GameObject.FindGameObjectsWithTag("Floor");
        if(stages.Length < 2)
        {
            CmdSpawnfloor();
        }
        else
        {
            CmdSetFloor();
        }

    }

    // Update is called once per frame
   [ClientCallback]
	void Update () {
        if (!base.isLocalPlayer)
            return;

        float h = 0f;
        float v = 0f;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) h = -1;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) h = 1;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) v = 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) v = -1;

        if (h != 0f || v != 0f)
        {
            CmdMove(h, v);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CmdBlockAttack();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            CmdBlockDefence();
        }
    }

    IEnumerator SetFreezeTime()
    {
        yield return new WaitForSeconds(3f);
    }

    [Command]
    void CmdMove(float h, float v)
    {
        if (transform.rotation.y != 0f)
        {
            h *= -1;
            v *= -1;
        }

        int x = (int)Math.Round(transform.position.x);
        int y = (int)Math.Round(transform.position.z);

        int _x = (int)Math.Round(originPos.x);
        int _y = (int)Math.Round(originPos.z);

        if (transform.position.x - originPos.x + h < width && transform.position.x- originPos.x + h >= 0 &&
          transform.position.z- originPos.z + v < height && transform.position.z- originPos.z + v >= 0)
        {
            if (floor.blockList[y-_y + (int)v][x-_x + (int)h] != null)
            {
                movement.Set(h, 0f, v);
                transform.position += movement;
            }
        }
    }

    [Command]
    void CmdBlockAttack()
    {
        int x = (int)Math.Round(transform.position.x);
        int y = (int)Math.Round(transform.position.z);

        int _x = (int)Math.Round(originPos.x);
        int _y = (int)Math.Round(originPos.z);

        floor.BlockAttack(x-_x, y-_y, gameObject);
    }

    
    [Command]
    void CmdBlockDefence()
    {
        int x = (int)Math.Round(transform.position.x);
        int y = (int)Math.Round(transform.position.z);

        int _x = (int)Math.Round(originPos.x);
        int _y = (int)Math.Round(originPos.z);

        floor.BlockDefence(x - _x, y - _y, gameObject);
    }
}
