using UnityEngine;

public interface IStartPosition
{
    Vector3 startPosition
    {
        get;
        set;
    }

    void SetStartPosition(Vector3 pos);

    void JumpToStartPosition();
}
