using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script wat bij een balobject aanwezig is en het meeste hiervoor regelt
/// </summary>
public class V3Ball : MonoBehaviour, IStartPosition
{
    public static readonly float radius = 0.5f;
    public static readonly float[] angles = new float[] { -45, -30, -10, 10, 30, 45 };

    public float moveSpeed = 12f;
    private Vector2 moveDir;

    private LinkedList<Vector3> positionHistory = new LinkedList<Vector3>();

    internal V3Player player;

    public Vector3 startPosition
    {
        get;
        set;
    }

    public void SetStartPosition(Vector3 pos)
    {
        startPosition = pos;
    }

    public void JumpToStartPosition()
    {
        transform.position = startPosition;
    }

    private void Start()
    {
        SendInNewRandomDirection();
        SetStartPosition(transform.position);
    }

    private void SendInNewRandomDirection()
    {
        // zorgt ervoor dat hij niet verticaal gaat
        int random = Random.Range(0, 2);
        float angle = random == 0 ? Random.Range(-70, 70) : Random.Range(110, 250);
        moveDir = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
    }

    private void Respawn()
    {
        //als hij uit het scherm gaat en in een muur spawnt voorkomt dit dat hij terug naar zijn laatste positie, buiten het scherm verspringt
        positionHistory.Clear();

        if (player.dead)
        {
            if (!player.otherPlayer.dead)
            {
                player = player.otherPlayer;
                Vector3 tmpPos = startPosition;
                tmpPos.x = player.otherPlayer.ballSpawnXPos;
                SetStartPosition(tmpPos);
            }         

            else
            {
                gameObject.SetActive(false);
            }
        }

        player.IncreasePlayerScore(player.otherPlayer, 50);
        JumpToStartPosition();
        SendInNewRandomDirection();
    }

    private RaycastHit2D MyRaycast()
    {
        // raycast in bewegingsrichting
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, radius);

        if (hit.collider != null)
        {
            return hit;
        }

        float angle = Mathf.Atan2(moveDir.y, moveDir.x);

        // raycast in alle richtingen totdat een collider geraakt wordt
        foreach (float a in angles)
        {
            hit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos((angle + a) * Mathf.Deg2Rad), Mathf.Sin((angle + a) * Mathf.Deg2Rad)), radius);

            if (hit.collider != null)
            {
                return hit;
            }
        }

        return hit;
    }

    private void FixedUpdate()
    {
        if (!V3Controller.Instance.CheckInsideField(transform.position))
        {
            Respawn();
        }

        if (positionHistory.Count > 200)
        {
            positionHistory.RemoveFirst();
        }

        transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime;
        RaycastHit2D hit = MyRaycast();

        Debug.DrawRay(transform.position, moveDir * radius, Color.red);
        Debug.Assert(hit.collider != this);

        if (hit.collider == null)
        {
            positionHistory.AddLast(transform.position);
            return;
        }

        Wall wall = hit.transform.GetComponent<Wall>();
        V3Mino mino = hit.transform.GetComponent<V3Mino>();

        if (wall != null)
        {
            player.IncreasePlayerScore(player.otherPlayer ,10);
            wall.Deactivate();
        }

        else if (mino != null)
        {
            mino.GetHit();
        }

        // hij knalt ergens tegenaan en stapt terug in zn positiehistorie totdat dit niet meer het geval is
        while (positionHistory.Count > 0)
        {
            transform.position = positionHistory.Last.Value;
            positionHistory.RemoveLast();

            RaycastHit2D nextHit = MyRaycast();
            Debug.Assert(nextHit.collider != this);

            if (nextHit.collider == null)
            {
                float angle = Mathf.Round(Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg / 90) * 90;
                // bepaal invalshoek
                Vector3 inAngle = Vector3.Normalize(new Vector3(hit.point.x, hit.point.y, 0) - transform.position);
                // bereken normaalvector
                Vector3 normal = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                moveDir = Vector3.Reflect(inAngle, normal);
                return;
            }

            // ga naar het volgende punt
            hit = nextHit;
        }
    }
}
