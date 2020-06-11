using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Script wat bij het controllerobject aanwezig is en algemene spelfunctionaliteit regelt
/// </summary>
public class V3Controller : Singleton<V3Controller>
{
    [Header("Grid Dimensions")]
    public int gridWidth = 10;
    public int gridHeight = 20;

    public Text scoreTxt1, scoreTxt2;
    //public TetrisPlayer player1, player2;

    public V3Player player1, player2;
    //public V3Player player2;

    public GameObject ballPrefab;
    public GameObject gameOverUI;

    public Text winText;

    //public List<V3Ball> ballList = new List<V3Ball>();

    [System.Serializable]
    public class moveBounds
    {
        public Vector2 topLeft, bottomRight;
    }

    public moveBounds bounds;

    protected override void Awake()
    {
        base.Awake();
        gameOverUI.SetActive(false);     
    }

    private void Start()
    {
        V3AudioManager.Instance.PlayMusic("music");
    }

    private void Update()
    {
        UpdatePlayerScores();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }
    
    private void UpdatePlayerScores()
    {
        scoreTxt1.text = player1.score.ToString();
        scoreTxt2.text = player2.score.ToString();
    }

    public void RestartGame()
    {
        player1.ResetSelf();
        player2.ResetSelf();
        gameOverUI.SetActive(false);
    }

    public void QuitGame()
    {
        if (Application.isEditor)
        {
            EditorApplication.isPlaying = false;
        }

        else
        {
            Application.Quit();
        }
    }

    internal bool CheckInsideField(Vector2 pos)
    {
        return pos.x > bounds.topLeft.x && pos.x < bounds.bottomRight.x && pos.y < bounds.topLeft.y && pos.y > bounds.bottomRight.y;
    }

    public void SpawnBall(V3Player player)
    {
        V3Ball ball = Instantiate(ballPrefab, new Vector2(player.ballSpawnXPos, Random.Range(0.5f, 19.5f)), Quaternion.identity).GetComponent<V3Ball>();
        ball.player = player;
        //ballList.Add(ball);
    }

    public void CheckBothDead()
    {
        if (player1.dead && player2.dead)
        {
            gameOverUI.SetActive(true);
            string winningPlayer = player1.score > player2.score ? "1" : "2";
            winText.text = player1.score == player2.score ? "Gelijk spel!" : "Speler " + winningPlayer + " heeft gewonnen!";
        }
    }
}
