using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script voor minos, de bouwstenen van tetrominos
/// </summary>
public class V3Mino : MonoBehaviour
{
    public V3Tetromino tetromino;
    internal float health = 1;

    // geraakt door een bal of een volle rij die verwijdert word
    public void RemoveFromGrid()
    {
        if (tetromino != null)
        {
            tetromino.player.RemoveAtPosition(transform.position);
        }  
    }

    public void GetHit()
    {
        health--;

        if (health == 0)
        {
            GetDestroyed();
        }
    }

    public void GetDestroyed()
    {
        V3BlockManager.Instance.ReturnMinoToIdlePool(gameObject);
        RemoveFromGrid();
        tetromino.CheckIfAnyChildren();
    }
}
