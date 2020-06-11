using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script voor minos, de bouwstenen van tetrominos
/// </summary>
public class V3Mino : MonoBehaviour, IHealth
{
    public V3Tetromino tetromino;

    public float hp
    {
        get;
        set;
    }

    public void Hit()
    {
        hp--;

        if (hp == 0)
        {
            GetDestroyed();
        }
    }

    // geraakt door een bal of een volle rij die verwijdert word
    public void RemoveFromGrid()
    {
        if (tetromino != null)
        {
            tetromino.player.RemoveAtPosition(transform.position);
        }  
    }

    public void GetDestroyed()
    {
        V3BlockManager.Instance.ReturnMinoToIdlePool(gameObject);
        RemoveFromGrid();
        tetromino.CheckIfAnyChildren();
    }
}
