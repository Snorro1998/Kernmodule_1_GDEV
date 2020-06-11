using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behandelt alle mino-objecten, de bouwstenen voor de tetrominos
/// </summary>
public class V3BlockManager : Singleton<V3BlockManager>
{
    [System.Serializable]
    public class Shape
    {
        public Vector2[] minoPositions;
        public Sprite minoSprite;
    }

    V3PoolManager poolManager;

    private void Start()
    {
        poolManager = V3PoolManager.Instance;
    }

    public Shape[] shapes;

    // voegt n minos aan pool toe
    public void AddNMinos(int n)
    {
        V3PoolManager.Instance.AddNObjects(n);
    }

    // kiest willekeurige vorm
    public Shape GetRandomShape()
    {
        int i = Random.Range(0, shapes.Length);
        return shapes[i];
    }

    // geeft minos aan tetromino
    public void GetNewShape(V3Tetromino tetromino)
    {
        // kiest willekeurig blok
        Shape s = GetRandomShape();

        // haalt 4 minos uit de pool
        for (int i = 0; i < s.minoPositions.Length; i++)
        {   
            GameObject gm = null;
            V3Mino mino = null;

            bool idleAvailable = poolManager.IdleObjectAvailable();
            bool busyAvailable = poolManager.BusyObjectAvailable();
            
            // pak een idle object uit de pool als er een beschikbaar is
            if (idleAvailable)
            {
                gm = poolManager.GetIdleObject();
                mino = gm.GetComponent<V3Mino>();
            }

            // pak een busy object uit de pool als er een beschikbaar is
            else if (busyAvailable)
            {
                gm = poolManager.GetBusyObject();
                mino = gm.GetComponent<V3Mino>();

                // zet de positie in het grid op null als hij er aan een toegewezen is
                mino.RemoveFromGrid();
            }

            // als het gelukt is om een object uit de pool te halen
            if (gm != null)
            {
                mino.tetromino = tetromino;
                mino.hp = Random.Range(1, 3);

                gm.transform.SetParent(tetromino.transform);
                gm.transform.localPosition = s.minoPositions[i];
                gm.GetComponent<SpriteRenderer>().sprite = s.minoSprite;
                // maak het object pas zichtbaar als alle wijzigingen erop zijn toegepast
                gm.SetActive(true);
            }
        }
    }

    // stopt mino terug in de idle pool
    public void ReturnMinoToIdlePool(GameObject gm)
    {
        gm.transform.SetParent(null);
        V3PoolManager.Instance.AddBackToIdlePool(gm);
    }
}
