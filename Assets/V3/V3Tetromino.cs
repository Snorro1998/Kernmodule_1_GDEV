using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script wat bij een tetromino-object, oftewel een vallend blok aanwezig is en het meeste hiervoor regelt
/// </summary>
public class V3Tetromino : MonoBehaviour
{
    private V3BlockManager blockManager;

    public V3Player player;
    private Vector3 startPosition;

    private void Start()
    {
        blockManager = V3BlockManager.Instance;
        startPosition = transform.position;
        GetNewShape();
    }

    // springt naar startpositie
    public void JumpToStartPosition()
    {
        transform.position = startPosition;
    }

    // vraagt een nieuwe vorm aan van de blockmanager
    public void GetNewShape()
    {
        JumpToStartPosition();
        blockManager.GetNewShape(this);
    }

    // blok is op de grond gevallen
    public void Land()
    {
        transform.DetachChildren();
        GetNewShape();
    }

    // vraagt nieuwe vorm aan als alle minos door ballen zijn gesloopt
    public void CheckIfAnyChildren()
    {
        if (transform.childCount == 0)
        {
            GetNewShape();
        }
    }
}
