using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;


public class Floor : NetworkBehaviour {

    public GameObject blockPrefab;
    public GameObject[][] blockList;

    public int width = 16;
    public int height = 8;

    public List<List<GameObject>> matchDatas = new List<List<GameObject>>();
    public List<GameObject> matchblock;

    struct Direction
    {
        public int x;
        public int y;
    };

    Direction[] dir = new Direction[4];

    Vector3 originPos;

    void CreateNewBlock(GameObject obj)
    {
        Block newBlock = obj.GetComponent<Block>();
        int type = UnityEngine.Random.Range(0, newBlock.blockMaterials.Count);

        if (obj.transform.position.z >= 10) newBlock.TeamNumber = 1;
        else newBlock.TeamNumber = 2;

        newBlock.blockType = type;
        newBlock.GetComponent<MeshRenderer>().material = newBlock.blockMaterials[type];

        NetworkServer.Spawn(obj);

        RpcSetBlock(obj, type);
    }

    public void CreateStage()
    {
        if (!isServer) return;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(blockPrefab, new Vector3(transform.position.x + x, 0, transform.position.z + y), Quaternion.identity) as GameObject;
                CreateNewBlock(obj);

                blockList[y][x] = obj;
            }
        }
        MatchAllBlock();
    }

    [ClientRpc]
    void RpcSetBlock(GameObject obj, int type)
    {
        Block newBlock = obj.GetComponent<Block>();
        newBlock.blockType = type;
        newBlock.GetComponent<MeshRenderer>().material = newBlock.blockMaterials[type];
    }

    void Awake()
    {
        blockList = new GameObject[height][];
        for (int i = 0; i < height; i++)
        {
            blockList[i] = new GameObject[width];
        }

        dir[0].x = 1;
        dir[0].y = 0;

        dir[1].x = -1;
        dir[1].y = 0;

        dir[2].x = 0;
        dir[2].y = 1;

        dir[3].x = 0;
        dir[3].y = -1;

    }

    void Start()
    {
        originPos = transform.position;
        CreateStage();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void BlockAttack(int x, int y, GameObject player)
    {
        if (blockList[y][x].GetComponent<Block>().isMatchChecked)
        {
            int index = blockList[y][x].GetComponent<Block>().matchIndex;

            int blockType = blockList[y][x].GetComponent<Block>().blockType;
            player.GetComponent<PlayerHealth>().gage[blockType] += matchDatas[index].Count;

            foreach (GameObject ob in matchDatas[index])
            {
                StartCoroutine(BlockFadeIn((int)ob.transform.position.x, (int)ob.transform.position.z));
                StartCoroutine(BlockFadeOut(ob));
            }
        }
    }

    public void BlockDefence(int x, int y, GameObject player)
    {
        if (blockList[y][x].GetComponent<Block>().isMatchChecked)
        {
            int index = blockList[y][x].GetComponent<Block>().matchIndex;

            int blockType = blockList[y][x].GetComponent<Block>().blockType;
            player.GetComponent<PlayerHealth>().gage[blockType] += matchDatas[index].Count;

            foreach (GameObject ob in matchDatas[index])
            {
                StartCoroutine(BlockDefence(ob));
                StartCoroutine(BlockFadeIn((int)ob.transform.position.x, (int)ob.transform.position.z));
            }
        }
    }

    void MatchAllBlock()
    {
        matchDatas.Clear();
        ResetMatch();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (blockList[y][x].GetComponent<Block>().isMatchChecked == true)
                {
                    continue;
                }
                else
                {
                    matchblock = new List<GameObject>();
                    CheckMatch(x, y);
                    if (matchblock.Count > 1)
                    {
                        foreach (GameObject go in matchblock)
                            go.GetComponent<Block>().matchIndex = matchDatas.Count;
                        matchDatas.Add(matchblock);
                    }
                    else matchblock.Clear();
                }
            }
        }
    }

    void CheckMatch(int x, int y)
    {
        if (blockList[y][x] == null) return;

        int blockType = blockList[y][x].GetComponent<Block>().blockType;

        for (int i = 0; i < 4; i++)
        {
            if (y + dir[i].y >= 0 && y + dir[i].y < height &&
                x + dir[i].x >= 0 && x + dir[i].x < width)
            {
                if (blockList[y + dir[i].y][x + dir[i].x].GetComponent<Block>().blockType == blockType)
                {
                    if (blockList[y + dir[i].y][x + dir[i].x].GetComponent<Block>().isMatchChecked == true)
                        continue;
                    else
                    {
                        matchblock.Add(blockList[y + dir[i].y][x + dir[i].x]);
                        blockList[y + dir[i].y][x + dir[i].x].GetComponent<Block>().isMatchChecked = true;
                        CheckMatch(x + dir[i].x, y + dir[i].y);
                    }
                }
            }
        }
    }

    void ResetMatch()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                blockList[y][x].GetComponent<Block>().Reset();
            }
        }
    }

    IEnumerator BlockFadeOut(GameObject go)
    {
        float timer = 0f;
        float delay = 0.5f;

        while (timer < 0.3f)
        {
            timer += 0.1f;
            go.transform.position += new Vector3(0, +0.35f, 0);
            yield return new WaitForSeconds(0.1f);
        }
        timer = 0;

        float offset = 18f / (delay * 10);
        if (go.transform.position.z >= 10)
            offset *= -1;

        while (timer < delay)
        {
            timer += 0.1f;

            if (go == null)
                yield break;

            go.transform.position += new Vector3(0, 0, offset);
            yield return new WaitForSeconds(0.1f);
        }

        if (go != null)
            Destroy(go);
    }

    IEnumerator BlockFadeIn(int x, int y)
    {
        int _x = (int)Math.Round(originPos.x);
        int _y = (int)Math.Round(originPos.z);

        float timer = 0f;

        GameObject obj = Instantiate(blockPrefab, new Vector3(x, -0.3f, y), Quaternion.identity) as GameObject;

        CreateNewBlock(obj);

        blockList[y - _y][x - _x] = obj;

        while (timer < 0.3f)
        {
            timer += 0.1f;
            blockList[y - _y][x - _x].transform.position += new Vector3(0, 0.1f, 0);
            yield return new WaitForSeconds(0.1f);
        }

        MatchAllBlock();
    }

    IEnumerator BlockDefence(GameObject go)
    {
        float timer = 0f;

        go.GetComponent<Block>().state = Block.State.Defence;

        while (timer < 0.2f)
        {
            timer += 0.1f;
            go.transform.position += new Vector3(0, +0.5f, 0);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(2.0f);

        Destroy(go);
    }

    public void RedAttack()
    {
        for (int i = 0; i < 30; i++)
        {
            float x = UnityEngine.Random.Range(transform.position.x, transform.position.x + width - 1);
            float spawnY;
            if (transform.position.z >= 10f) spawnY = 0f;
            else spawnY = 10f;

            float y = UnityEngine.Random.Range(spawnY, spawnY + height - 1);
            GameObject obj = Instantiate(blockPrefab, new Vector3(x, 5, y), Quaternion.identity) as GameObject;

            Block newBlock = obj.GetComponent<Block>();
            int type = 2;

            if (transform.position.z >= 10) newBlock.TeamNumber = 1;
            else newBlock.TeamNumber = 2;

            newBlock.blockType = type;
            newBlock.GetComponent<MeshRenderer>().material = newBlock.blockMaterials[type];
            newBlock.state = Block.State.Attack;

            NetworkServer.Spawn(obj);
            RpcSetBlock(obj, type);

            StartCoroutine(RedBlockAttack(obj));
        }
    }

    IEnumerator RedBlockAttack(GameObject go)
    {
        float timer = 0f;
        
        while (timer < 2f)
        {
            timer += 0.1f;

            float y = UnityEngine.Random.Range(0.3f, 2f);

            if(go!=null)
            go.transform.position += new Vector3(0, -y, 0);

            yield return new WaitForSeconds(0.1f);
        }

        if (go != null)
            Destroy(go);

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Floor");
        foreach(GameObject floor in gameObjects)
        {
            if(go.transform != floor.transform)
            {
                floor.GetComponent<Floor>().ResetStage();
            }
        }
    }
    
    public void ResetStage()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x=0;x<width;x++)
            {
                if(blockList[y][x] == null)
                {
                    GameObject obj = Instantiate(blockPrefab, new Vector3(transform.position.x + x, 0, transform.position.z + y), Quaternion.identity) as GameObject;
                    CreateNewBlock(obj);

                    blockList[y][x] = obj;
                }
            }
        }
        MatchAllBlock();
    }
}
