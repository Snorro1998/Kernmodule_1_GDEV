using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float moveSpeed = 1.20f;
    public static readonly float radius = 0.5f;
    public static readonly float[] angles = new float[]{ -45, -30, -10, 10, 30, 45 };
    Vector2 moveDir;

    LinkedList<Vector3> positionHistory = new LinkedList<Vector3>();

    Vector3 startPos;

    private void Start()
    {
        float angle = Random.Range(0, 360);
        moveDir = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
        startPos = transform.position;
    }

    private void respawn()
    {
        if (transform.position.x < 0) Controller.Instance.score2 += 100;
        else Controller.Instance.score1 += 100;
        transform.position = startPos;
    }

    private RaycastHit2D myRaycast()
    {
        // do ordinary raycast in moving direction.
        RaycastHit2D h = Physics2D.Raycast(transform.position, moveDir, radius);
        if (h.collider != null) return h;
        
        float angle = Mathf.Atan2(moveDir.y, moveDir.x);
        
        // raycast in all directions until a collider has been found.
        foreach (float a in angles)
        {
            h = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos((angle + a) * Mathf.Deg2Rad), Mathf.Sin((angle + a) * Mathf.Deg2Rad)), radius);
            if (h.collider != null) return h;
        }

        return h;
    }

    private void FixedUpdate()
    {
        if (!Controller.Instance.checkInsideField(transform.position))
        {
            respawn();
        }

        if (positionHistory.Count > 200) positionHistory.RemoveFirst();
        transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime;

        RaycastHit2D hit = myRaycast();

        // Debug stuff, collider can never be the ball itself.
        Debug.DrawRay(transform.position, moveDir * radius, Color.red);
        Debug.Assert(hit.collider != this);

        if (hit.collider == null)
        {
            positionHistory.AddLast(transform.position);
            return;
        }

        Wall w = hit.transform.GetComponent<Wall>();
        if (w != null)
        {
            if (transform.position.x > 0) Controller.Instance.score1 += 10;
            else Controller.Instance.score2 += 10;
            w.Deactivate();
        }

        // Collision has occurred, keep backtracking until the collision has been resolved or no previous non-colliding point could be found.
        while (positionHistory.Count > 0)
        {
            transform.position = positionHistory.Last.Value;
            positionHistory.RemoveLast();
            
            RaycastHit2D nextHit = myRaycast();
            Debug.Assert(nextHit.collider != this);

            if (nextHit.collider == null)
            {
                // Note that hit is used rather than nextHit, because hit is the last previous conflicting point (nextHit.collider == null here)

                // Compute quadrant that ball is moving to
                // TODO should this still add 180 degrees?
                float angle = Mathf.Round(Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg / 90) * 90;
                // Determine angle of collision
                Vector3 inAngle = Vector3.Normalize(new Vector3(hit.point.x, hit.point.y, 0) - transform.position);
                // Compute normal vector of colliding gameobject
                Vector3 normal = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                moveDir = Vector3.Reflect(inAngle, normal);
                return;
            }

            // advance to next point
            hit = nextHit;
        }
    }
}
