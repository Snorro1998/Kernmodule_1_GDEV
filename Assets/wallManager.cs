using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wallManager : MonoBehaviour
{
    public List<Transform> activeObjects = new List<Transform>();
    public List<Transform> inactiveObjects = new List<Transform>();

    private void Start()
    {
        foreach(Transform t in transform)
        {
            activeObjects.Add(t);
        }
    }

    public void ActivateRandomObject()
    {
        int i = Random.Range(0, inactiveObjects.Count);
        Transform t = inactiveObjects[i];
        t.gameObject.SetActive(true);
        activeObjects.Add(t);
        inactiveObjects.Remove(t);
    }

    public void DeactivateObject(Transform t)
    {
        activeObjects.Remove(t);
        inactiveObjects.Add(t);
        t.gameObject.SetActive(false);
    }
}
