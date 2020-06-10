using UnityEngine;

public class MinoScript : MonoBehaviour, IHealth
{
    private TetrisPlayer player;

    public float hp
    {
        get;
        set;
    }

    private void Start()
    {
        player = GetComponentInParent<TetrisBlock>().tet;
        hp = Random.Range(1, 3);
    }

    public void Hit()
    {
        hp--;

        if (hp == 0)
        {
            player.AddScoreToOther(2);
            Destroy(gameObject);
        }
    }
}
