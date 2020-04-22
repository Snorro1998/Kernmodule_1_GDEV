using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinoScript : MonoBehaviour
{
    private void FixedUpdate()
    {
        print(transform + ": " +  transform.position);
    }
}
