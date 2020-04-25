using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    wallManager w;

    private void Start()
    {
        w = transform.parent.GetComponent<wallManager>();
    }

    public void Deactivate()
    {
        w.DeactivateObject(transform);
    }
}
