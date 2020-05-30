using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    internal TetrisPlayer tet;

    public void PassChildrenTo(Transform tran)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            transform.GetChild(i).parent = tran;
        }
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (transform.childCount == 0)
        {
            tet.CreateNextBlock();
            Destroy(gameObject);
        }
    }
}
