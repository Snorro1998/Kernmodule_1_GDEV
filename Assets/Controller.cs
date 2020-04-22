using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; private set; }
    
    public float score1, score2;
    public Text scoreTxt1, scoreTxt2;
    public GameObject ballObject;

    [System.Serializable]
    public class moveBounds
    {
        public Vector2 topLeft, bottomRight;
    }

    public moveBounds bounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AudioManager.Instance.playSound("music");
    }

    public void addBall(float xPos)
    {
        GameObject gm = Instantiate(ballObject, new Vector2(xPos, Random.Range(0.5f, 19.5f)), Quaternion.identity);
    }

    public bool checkInsideField(Vector2 pos)
    {
        return pos.x > bounds.topLeft.x && pos.x < bounds.bottomRight.x && pos.y < bounds.topLeft.y && pos.y > bounds.bottomRight.y; // && transform.position.x < bounds.rightEdge.x;
    }

    private void FixedUpdate()
    {
        scoreTxt1.text = score1.ToString();
        scoreTxt2.text = score2.ToString();
    }
}
