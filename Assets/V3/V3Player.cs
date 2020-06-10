using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script wat bij een spelerobject aanwezig is en het meeste hiervoor regelt
/// </summary>
public class V3Player : MonoBehaviour
{
    private AudioManager audioManager;
    private V3Controller controller;
    private V3BlockManager blockManager;

    public GameObject tetrominoPrefab;
    internal V3Tetromino tetromino;
    public wallManager wall;

    int gridWidth;
    int gridHeight;
    public Transform[,] grid;

    public float ballSpawnXPos = 0;
    public float score = 0;

    [Header("Movement Speed")]
    public float horMoveSpeed = 0.4f;
    public float verMoveStartSpeed = 0.5f;
    public float verMoveEndSpeed = 0.1f;
    internal float verMoveSpeed;
    private float verMovementTimer = 0;

    internal bool dead;
    internal V3Player otherPlayer;

    public struct Bounds
    {
        public Vector3 bottomLeft, bottomRight, topLeft, topRight;
    }

    Bounds bounds;

    [System.Serializable]
    public struct Keys
    {
        public KeyCode leftKey, rightKey, upKey, downKey, fallKey;
    }

    public Keys Controls;

    private void Start()
    {
        // is geloof ik beter voor performance dan telkens iets.instance te gebruiken
        audioManager = AudioManager.Instance;
        controller = V3Controller.Instance;
        blockManager = V3BlockManager.Instance;

        // stel het grid en de bounds in
        gridWidth = controller.gridWidth;
        gridHeight = controller.gridHeight;

        grid = new Transform[gridWidth, gridHeight];
        UpdateOrigins(gridWidth, gridHeight);
        SetLineRendererPositions();

        // voeg minos toe aan pool
        blockManager.AddNMinos(gridWidth * gridHeight);

        // was eerst globaal, maar dat is nu niet meer nodig, omdat de tetromino naar zijn startpositie kan springen
        Vector3 tetrominoSpawnPosition = new Vector3(bounds.topLeft.x + (bounds.topRight.x - bounds.topLeft.x) / 2, bounds.topLeft.y, bounds.topLeft.z);
        // maak tetromino aan
        tetromino = Instantiate(tetrominoPrefab, tetrominoSpawnPosition, Quaternion.identity).GetComponent<V3Tetromino>();
        tetromino.player = this;

        otherPlayer = controller.player1 == this ? controller.player2 : controller.player1;
        controller.SpawnBall(this);

        verMoveSpeed = verMoveStartSpeed;
    }

    private void Update()
    {
        if (dead)
        {
            return;
        }

        bool advance = false;
        int horSpeed = 0;
        int verSpeed = 0;

        UpdateBlockPosition(ref horSpeed, ref verSpeed, ref advance);
        CheckIfQuickDropOrAtBottom(ref verSpeed, ref advance);
    }

    #region PlayerVariablesModifiers

    // verhoogt de snelheid waarmee blokken vallen voor een speler
    public void IncreasePlayerSpeed(V3Player player)
    {
        if (player != null)
        {
            player.verMoveSpeed = verMoveEndSpeed;
        }
    }

    // voegt waarde aan score van een player toe
    public void IncreasePlayerScore(V3Player player, float amount)
    {
        if (player != null)
        {
            player.score += amount;
        }     
    }

    #endregion

    // speler heeft grid tot boven gevuld
    void GameOver()
    {
        dead = true;
        audioManager.PlaySound("gameover");
        IncreasePlayerSpeed(otherPlayer);
        controller.CheckBothDead();
    }

    // verwijdert referentie naar object die op positie "pos" in het grid staat
    public void RemoveAtPosition(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x - bounds.bottomLeft.x);
        int y = Mathf.FloorToInt(pos.y - bounds.bottomLeft.y);

        if (x < 0 || x > gridWidth - 1 || y < 0 || y > gridHeight - 1)
        {
            print("Kan geen object uit het grid van " + name + " verwijderen op de positie " + x + ", " + y);
        }

        else
        {
            grid[x, y] = null;
        }
    }

    #region RowManagement

    // beweegt een rij omlaag
    void MoveRowsDownFrom(int y)
    {
        for (int i = y; i < gridHeight - 1; i++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, i] = grid[x, i + 1];

                if (grid[x, i] != null)
                {
                    grid[x, i].position += Vector3.down;
                }
            }
        }
    }

    // verwijdert een rij
    void DeleteRowAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            GameObject gm = grid[x, y].gameObject;
            RemoveAtPosition(gm.transform.position);
            blockManager.ReturnMinoToIdlePool(gm);
        }
    }

    // kijkt of een rij volledig gevuld is
    bool IsFullRowAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region InsideGridChecks

    // kijkt of hij horizontaal in het speelveld zit
    bool IsOutOfBoundsX(float x)
    {
        return x < bounds.bottomLeft.x || x >= bounds.bottomRight.x;
    }

    // kijkt of hij verticaal in het speelveld zit
    bool IsOutOfBoundsY(float y)
    {
        return y < bounds.bottomLeft.y;
    }

    // kijkt of hij in het speelveld zit
    bool IsOutOfBounds(float x, float y)
    {
        return IsOutOfBoundsX(x) || IsOutOfBoundsY(y);
    }

    #endregion

    // werkt misschien niet helemaal lekker
    bool IsOccupied(float x, float y)
    {
        return IsOccupied(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }

    // werkt misschien niet helemaal lekker
    bool IsOccupied(int x, int y)
    {
        x -= Mathf.FloorToInt(bounds.bottomLeft.x);
        y -= Mathf.FloorToInt(bounds.bottomLeft.y);
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && grid[x, y];
    }

    #region TransformsCalculation

    //bepaalt rotatie
    void ComputeRotation()
    {
        if (Input.GetKeyDown(Controls.upKey))
        {
            // draai het blok 90 graden. Als hij na de rotatie op een ongeldige plek staat dan draait hij hem terug
            tetromino.transform.Rotate(0, 0, 90);
            bool outside = false;

            foreach (Transform t in tetromino.transform)
            {
                if (IsOutOfBounds(t.position.x, t.position.y) || IsOccupied(t.position.x, t.position.y))
                {
                    outside = true;
                    break;
                }
            }

            if (outside)
            {
                tetromino.transform.Rotate(0, 0, -90);
            }
        }
    }

    // bepaalt horizontale snelheid
    void ComputeHorSpeed(ref int hSpeed)
    {
        if (Input.GetKeyDown(Controls.leftKey))
        {
            hSpeed--;
        }

        if (Input.GetKeyDown(Controls.rightKey))
        {
            hSpeed++;
        }
    }

    // bepaalt verticale snelheid
    void ComputeVerSpeed(ref int vSpeed, ref bool advance)
    {
        if (Input.GetKeyDown(Controls.downKey) || Time.time > verMovementTimer + verMoveSpeed)
        {
            advance = Time.time > verMovementTimer + verMoveSpeed;
            verMovementTimer = Time.time;
            vSpeed--;
        }
    }

    bool CheckMovementXPossible(ref int hSpeed)
    {
        foreach (Transform t in tetromino.transform)
        {
            float newx = Mathf.Floor(t.position.x) + Mathf.Sign(hSpeed);

            if (IsOutOfBoundsX(newx) || IsOccupied(newx, t.position.y))
            {
                return false;
            }
        }
        return true;
    }

    bool CheckMovementYPossible(ref int vSpeed)
    {
        foreach (Transform t in tetromino.transform)
        {
            float newy = Mathf.Floor(t.position.y) + Mathf.Sign(vSpeed);

            if (IsOutOfBoundsY(newy) || IsOccupied(t.position.x, newy))
            {
                return false;
            }
        }
        return true;
    }

    void ComputeMovement(ref int hSpeed, ref int vSpeed)
    {
        if (!CheckMovementXPossible(ref hSpeed))
        {
            hSpeed = 0;
        }

        if (!CheckMovementYPossible(ref vSpeed))
        {
            vSpeed = 0;
        }
    }

    #endregion

    void MoveBlock(int dX, int dY)
    {
        tetromino.transform.position += Vector3.right * dX + Vector3.up * dY;
    }

    // bepaalt standaardtransformaties
    void UpdateBlockPosition(ref int horSpeed, ref int verSpeed, ref bool advance)
    {
        ComputeRotation();
        ComputeHorSpeed(ref horSpeed);
        ComputeVerSpeed(ref verSpeed, ref advance);
        ComputeMovement(ref horSpeed, ref verSpeed);

        MoveBlock(horSpeed, verSpeed);
    }

    // bepaalt wat te doen in bijzondere gevallen
    void CheckIfQuickDropOrAtBottom(ref int vSpeed, ref bool advance)
    {
        if (Input.GetKeyDown(Controls.fallKey) || (advance && vSpeed == 0))
        {
            int dx = 0, dy = -1;

            while (dy != 0)
            {
                ComputeMovement(ref dx, ref dy);
                MoveBlock(dx, dy);
            }

            foreach (Transform t in tetromino.transform)
            {
                int x = Mathf.FloorToInt(t.position.x) - (int)bounds.bottomLeft.x;
                int y = Mathf.FloorToInt(t.position.y) - (int)bounds.bottomLeft.y;

                if (y < 0 || y > gridHeight - 1)
                {
                    GameOver();
                    return;
                }

                grid[x, y] = t;
            }

            int nRowsCleared = 0;

            for (int y = 0; y < gridHeight; y++)
            {
                if (y >= 0 && IsFullRowAt(y))
                {
                    nRowsCleared++;
                    DeleteRowAt(y);
                    MoveRowsDownFrom(y);
                    --y;
                }
            }

            if (nRowsCleared != 0)
            {
                int score;

                switch (nRowsCleared)
                {
                    default:
                        score = 100;
                        break;
                    case 2:
                        score = 300;
                        break;
                    case 3:
                        score = 600;
                        break;
                    case 4:
                        score = 1000;
                        break;
                }

                IncreasePlayerScore(this, score);

                audioManager.PlaySound("clear" + nRowsCleared + "row");

                for (int i = 0; i < nRowsCleared * 2; i++)
                {
                    wall.ActivateRandomObject();
                }
            }
            else
            {
                audioManager.PlaySound("nextblock");
            }

            tetromino.Land();
            IncreasePlayerScore(this, 20);
            return;
        }
    }

    public void ClearGrid()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Transform t = grid[x, y];

                if (t != null)
                {
                    V3Mino mino = t.gameObject.GetComponent<V3Mino>();
                    mino.GetDestroyed();
                }
            }
        }
    }

    public void ResetSelf()
    {
        score = 0;
        verMoveSpeed = verMoveStartSpeed;
        ClearGrid();
        wall.ActivateAllObjects();
        dead = false;
    }

    // kan eenvoudiger, aangezien je alleen topleft en bottomright nodig hebt
    void UpdateOrigins(int gridWidth, int gridHeight)
    {
        bounds.bottomLeft = transform.position;
        bounds.bottomRight = bounds.bottomLeft + Vector3.right * gridWidth;
        bounds.topLeft = transform.position + Vector3.up * gridHeight;
        bounds.topRight = bounds.topLeft + Vector3.right * gridWidth;
    }

    // stelt linerenderer in
    void SetLineRendererPositions()
    {
        LineRenderer lr = GetComponent<LineRenderer>();

        if (lr != null)
        {
            lr.positionCount = 4;
            lr.SetPosition(0, bounds.bottomLeft);
            lr.SetPosition(1, bounds.bottomRight);
            lr.SetPosition(2, bounds.topRight);
            lr.SetPosition(3, bounds.topLeft);
        }
    }
}
