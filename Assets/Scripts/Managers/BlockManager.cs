using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : Singleton<BlockManager>
{
    public GameObject[] Blocks;

    protected override void Awake()
    {
        base.Awake();
    }

    internal TetrisBlock CreateBlock(Vector3 spawnPos)
    {
        int i = Random.Range(0, Blocks.Length);
        GameObject gm = Instantiate(Blocks[i], spawnPos, Quaternion.identity);
        return gm.GetComponent<TetrisBlock>();
    }
}
