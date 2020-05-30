using UnityEngine;

public class Wall : MonoBehaviour
{
    wallManager wall;

    private void Start()
    {
        wall = transform.parent.GetComponent<wallManager>();
    }

    public void Deactivate()
    {
        wall.DeactivateObject(transform);
    }
}
