using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Eerder een enkele objectpool dan een manager. Zorgt momenteel alleen voor het poolen van de minos
/// </summary>
public class V3PoolManager : Singleton<V3PoolManager>
{
    //geen queues, omdat het vaak voorkomt dat er geschoven wordt met objecten die midden in een lijst zitten
    public List<GameObject> idleObjects = new List<GameObject>();
    public List<GameObject> busyObjects = new List<GameObject>();
    public GameObject poolPrefab;

    protected override void Awake()
    {
        base.Awake();
    }

    // voegt n objecten aan de pool toe
    public void AddNObjects(int n)
    {
        if (idleObjects == null)
        {
            idleObjects = new List<GameObject>();
        } 

        for (int i = 0; i < n; i++)
        {
            GameObject gm = Instantiate(poolPrefab);
            gm.SetActive(false);
            gm.name = "obj" + i;
            idleObjects.Add(gm);
        }
    }

    // is er een idle object in de pool beschikbaar?
    public bool IdleObjectAvailable()
    {
        if (idleObjects == null)
        {
            return false;
        }

        return idleObjects.Count != 0;
    }

    // is er een busy object in de pool beschikbaar?
    public bool BusyObjectAvailable()
    {
        if (busyObjects == null)
        {
            return false;
        }

        return busyObjects.Count != 0;
    }

    // pakt het eerste idle object uit de pool
    public GameObject GetIdleObject()
    {
        GameObject gm = idleObjects[0];
        idleObjects.Remove(gm);
        busyObjects.Add(gm);
        return gm;
    }

    // pakt het eerste busy object uit de pool
    public GameObject GetBusyObject()
    {
        // kan voor problemen zorgen als er een actieve mino van een grid gepakt word, omdat hij dan zijn positie in het grid niet naar null terug zet
        GameObject gm = busyObjects[0];
        busyObjects.Remove(gm);
        busyObjects.Add(gm);
        return gm;
    }

    // zet gameobject terug in idlepool
    public void AddBackToIdlePool(GameObject gm)
    {
        busyObjects.Remove(gm);
        idleObjects.Add(gm);
        gm.SetActive(false);
    }
}
