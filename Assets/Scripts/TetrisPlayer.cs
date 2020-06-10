using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisPlayer : MonoBehaviour
{
    [Header ("Grid Dimensions")]
    public static int gridWidth = 10;
    public static int gridHeight = 20;

    public Transform[,] grid = new Transform[gridWidth, gridHeight];

    [Header ("Movement Speed")]
    public float horMoveSpeed = 0.4f;
    public float verMoveSpeed = 0.5f;
    private float verMovementTimer = 0;

    internal bool dead = false;
    public float ballSpawnXPos;
    public wallManager wall;
    private TetrisBlock currentBlock;

    private Vector3 spawnPos;
    internal float score = -20;
    internal TetrisPlayer otherPlayer;

    public struct Bounds
    {
        public Vector3 bottomLeft, bottomRight, topLeft, topRight;
    }

    Bounds _bounds;

    [System.Serializable]
    public struct Keys
    {
        public KeyCode leftKey, rightKey, upKey, downKey, fallKey;
    }

    [Header("Controls")]
    public Keys Controls;

    private void Awake()
    {
        UpdateOrigins();
        otherPlayer = Controller.Instance.player1 != this ? Controller.Instance.player1 : Controller.Instance.player2;
        CreateNextBlock();

        Controller.Instance.AddBall(ballSpawnXPos, this);
        LineRenderer lr = GetComponent<LineRenderer>();

        if (lr != null)
        {
            lr.positionCount = 4;
            lr.SetPosition(0, _bounds.bottomLeft);
            lr.SetPosition(1, _bounds.bottomRight);
            lr.SetPosition(2, _bounds.topRight);
            lr.SetPosition(3, _bounds.topLeft);
        }
    }

    public void AddScoreToOther(float addScore)
    {
        if (!otherPlayer.dead)
        {
            otherPlayer.AddScore(addScore);
        }
    }

    public void AddScore(float addScore)
    {
        if (!dead)
        {
            score += addScore;
        }
    }

    public void CreateNextBlock()
    {
        AddScore(20);

        currentBlock = BlockManager.Instance.CreateBlock(spawnPos);
        currentBlock.tet = this;
    }

    void GameOver()
    {
        AudioManager.Instance.PlaySound("gameover");

        if (Controller.Instance.player1 != this)
        {
            Controller.Instance.player1.verMoveSpeed = 0.1f;
        }

        else
        {
            Controller.Instance.player2.verMoveSpeed = 0.1f;
        }

        dead = true;
    }

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

    void ComputeVerSpeed(ref int vSpeed, ref bool advance)
    {
        if (Input.GetKeyDown(Controls.downKey) || Time.time > verMovementTimer + verMoveSpeed)
        {
            advance = Time.time > verMovementTimer + verMoveSpeed;
            verMovementTimer = Time.time;
            vSpeed--;
        }
    }

    void ComputeRotation()
    {
        if (Input.GetKeyDown(Controls.upKey))
        {
            // draai het blok 90 graden. Als hij na de rotatie op een ongeldige plek staat dan draait hij hem terug
            currentBlock.transform.Rotate(0, 0, 90);
            bool outside = false;

            foreach (Transform t in currentBlock.transform)
            {
                if (IsOutOfBounds(t.position.x, t.position.y) || IsOccupied(t.position.x, t.position.y))
                {
                    outside = true;
                    break;
                }
            }

            if (outside) currentBlock.transform.Rotate(0, 0, -90);
        }
    }

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

            foreach (Transform t in currentBlock.transform)
            {
                int x = Mathf.FloorToInt(t.position.x) - (int)_bounds.bottomLeft.x;
                int y = Mathf.FloorToInt(t.position.y) - (int)_bounds.bottomLeft.y;

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

                AddScore(score);

                AudioManager.Instance.PlaySound("clear" + nRowsCleared + "row");

                for (int i = 0; i < nRowsCleared * 2; i++)
                {
                    wall.ActivateRandomObject();
                }
            }
            else
            {
                AudioManager.Instance.PlaySound("nextblock");
            }

            currentBlock.PassChildrenTo(transform);
            CreateNextBlock();
            return;
        }
    }

    void UpdateBlockPosition(ref int horSpeed, ref int verSpeed, ref bool advance)
    {
        ComputeRotation();
        ComputeHorSpeed(ref horSpeed);
        ComputeVerSpeed(ref verSpeed, ref advance);
        ComputeMovement(ref horSpeed, ref verSpeed);

        MoveBlock(horSpeed, verSpeed);
    }

    private void Update()
    {
        DrawDebugLines();

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

    void MoveRowsDownFrom(int y)
    {
        for (int i = y; i < gridHeight - 1; i++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, i] = grid[x, i + 1];
                if (grid[x, i] != null) grid[x, i].position += Vector3.down;
            }
        }
    }

    void DeleteRowAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            GameObject gm = grid[x, y].gameObject;
            gm.SetActive(false);
            //Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

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

    bool IsOutOfBoundsX(float x)
    {
        return x < _bounds.bottomLeft.x || x >= _bounds.bottomRight.x;
    }

    bool IsOutOfBoundsY(float y)
    {
        return y < _bounds.bottomLeft.y;
    }

    bool IsOutOfBounds(float x, float y)
    {
        return IsOutOfBoundsX(x) || IsOutOfBoundsY(y);
    }

    bool IsOccupied(float x, float y)
    {
        return IsOccupied(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }

    bool IsOccupied(int x, int y)
    {
        x -= Mathf.FloorToInt(_bounds.bottomLeft.x);
        y -= Mathf.FloorToInt(_bounds.bottomLeft.y);
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && grid[x, y];
    }

    bool CheckMovementXPossible(ref int hSpeed)
    {
        foreach (Transform t in currentBlock.transform)
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
        foreach (Transform t in currentBlock.transform)
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

    void MoveBlock(int dX, int dY)
    {
        currentBlock.transform.position += Vector3.right * dX + Vector3.up * dY;
    }

    void UpdateOrigins()
    {
        _bounds.bottomLeft = transform.position;
        _bounds.bottomRight = _bounds.bottomLeft + Vector3.right * gridWidth;
        _bounds.topLeft = transform.position + Vector3.up * gridHeight;
        _bounds.topRight = _bounds.topLeft + Vector3.right * gridWidth;
        spawnPos = new Vector3(_bounds.topLeft.x + (_bounds.topRight.x - _bounds.topLeft.x) / 2, _bounds.topLeft.y, _bounds.topLeft.z);
    }

    void DrawDebugLines()
    {    
        Debug.DrawLine(_bounds.topLeft, _bounds.topRight);
        Debug.DrawLine(_bounds.topRight, _bounds.bottomRight);
        Debug.DrawLine(_bounds.bottomLeft, _bounds.bottomRight);
        Debug.DrawLine(_bounds.topLeft, _bounds.bottomLeft);
    }
}
