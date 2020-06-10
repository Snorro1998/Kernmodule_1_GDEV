using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    float hp
    {
        get;
        set;
    }

    void Hit();
}
