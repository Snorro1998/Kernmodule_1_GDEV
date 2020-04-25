using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : Singleton<Controller>
{   
    public float score1, score2;
    public Text scoreTxt1, scoreTxt2;
    public GameObject ballObject;

    [System.Serializable]
    public class moveBounds
    {
        public Vector2 topLeft, bottomRight;
    }

    public moveBounds bounds;

    private void Start()
    {
        AudioManager.Instance.PlaySound("music");
    }

    public void AddBall(float xPos)
    {
        GameObject gm = Instantiate(ballObject, new Vector2(xPos, Random.Range(0.5f, 19.5f)), Quaternion.identity);
    }

    public bool CheckInsideField(Vector2 pos)
    {
        return pos.x > bounds.topLeft.x && pos.x < bounds.bottomRight.x && pos.y < bounds.topLeft.y && pos.y > bounds.bottomRight.y; // && transform.position.x < bounds.rightEdge.x;
    }

    private void FixedUpdate()
    {
        scoreTxt1.text = score1.ToString();
        scoreTxt2.text = score2.ToString();
    }
}
