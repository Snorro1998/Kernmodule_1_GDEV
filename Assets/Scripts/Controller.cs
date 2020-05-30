using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class Controller : Singleton<Controller>
{
    public Text scoreTxt1, scoreTxt2;
    public GameObject ballObject;
    public TetrisPlayer player1, player2;
    public GameObject gameOverGUI;
    public Text winText;

    [System.Serializable]
    public class moveBounds
    {
        public Vector2 topLeft, bottomRight;
    }

    public moveBounds bounds;

    private void Start()
    {
        AudioManager.Instance.PlaySound("music");
        gameOverGUI.SetActive(false);
    }

    internal void AddBall(float xPos, TetrisPlayer player)
    {
        Ball ball = Instantiate(ballObject, new Vector2(xPos, Random.Range(0.5f, 19.5f)), Quaternion.identity).GetComponent<Ball>();
        ball.player = player;
    }

    internal bool CheckInsideField(Vector2 pos)
    {
        return pos.x > bounds.topLeft.x && pos.x < bounds.bottomRight.x && pos.y < bounds.topLeft.y && pos.y > bounds.bottomRight.y;
    }

    private void UpdatePlayerScores()
    {
        scoreTxt1.text = player1.score.ToString();
        scoreTxt2.text = player2.score.ToString();
    }

    public void RestartGame()
    {
        AudioManager.Instance.StopAllSounds();
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    private void FixedUpdate()
    {
        UpdatePlayerScores();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }

        if (player1.dead && player2.dead && !gameOverGUI.activeInHierarchy)
        {
            string winningPlayer = player1.score > player2.score ? "1" : "2";
            winText.text = player1.score == player2.score ? "Gelijk spel!" : "Speler " + winningPlayer + " heeft gewonnen!";

            gameOverGUI.SetActive(true);
        }
    }
}
