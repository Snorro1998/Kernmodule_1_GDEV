﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisPlayer : MonoBehaviour
{
    public static int gridWidth = 10;
    public static int gridHeight = 20;

    public Transform[,] grid = new Transform[gridWidth, gridHeight];

    public float horMoveSpeed = 0.4f;
    public float verMoveSpeed = 0.5f;
    //float horMovementTimer = 0;
    float verMovementTimer = 0;

    bool dead = false;

    public wallManager wall;

    TetrisBlock currentBlock;
    public GameObject[] Blocks;

    Vector3 bottomLeft, bottomRight, topLeft, topRight, spawnPos;
    public KeyCode leftKey, rightKey, upKey, downKey, fallKey;

    private void Awake()
    {
        UpdateOrigins();
        CreateNextBlock();
        Controller.Instance.addBall(Mathf.Sign(transform.position.x) * 2);
    }

    public void CreateNextBlock()
    {
        int i = Random.Range(0, Blocks.Length);
        GameObject gm = Instantiate(Blocks[i], spawnPos, Quaternion.identity);
        currentBlock = gm.GetComponent<TetrisBlock>();
    }

    private void Update()
    {
        DrawDebugLines();

        int horSpeed = 0;
        int verSpeed = 0;

        if (dead) return;

        if (Input.GetKeyDown(leftKey))
        {
            horSpeed--;
        }

        if (Input.GetKeyDown(rightKey))
        {
            horSpeed++;
        }

        if (Input.GetKeyDown(upKey))
        {
            //draai het blok. Als hij na de rotatie op een ongeldige plek staat dan draait hij hem terug
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

        bool advance = false;

        if (Input.GetKeyDown(downKey) || Time.time > verMovementTimer + verMoveSpeed)
        {
            advance = Time.time > verMovementTimer + verMoveSpeed;
            verMovementTimer = Time.time;
            verSpeed--;
        }

        ComputeMovement(ref horSpeed, ref verSpeed);
        MoveBlock(horSpeed, verSpeed);


        if (Input.GetKeyDown(fallKey) || (advance && verSpeed == 0))
        {
            int dx = 0, dy = -1;
            while (dy != 0)
            {
                ComputeMovement(ref dx, ref dy);
                MoveBlock(dx, dy);
            }
            foreach (Transform t in currentBlock.transform)
            {
                int x = Mathf.FloorToInt(t.position.x) - (int)bottomLeft.x;
                int y = Mathf.FloorToInt(t.position.y) - (int)bottomLeft.y;

                if (y < 0 || y > gridHeight - 1)
                {
                    AudioManager.Instance.playSound("gameover");
                    dead = true;
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
                int score = 20;

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

                if(transform.position.x < 0) Controller.Instance.score1 += score;
                else Controller.Instance.score2 += score;

                AudioManager.Instance.playSound("clear" + nRowsCleared + "row");
                for (int i = 0; i < nRowsCleared; i++)
                {
                    wall.ActivateRandomObject();
                }
            }
            else
            {
                AudioManager.Instance.playSound("nextblock");
            }

            currentBlock.PassChildrenTo(transform);
            CreateNextBlock();
            return;
        }
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
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    bool IsFullRowAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] == null) return false;
        }
        return true;
    }

    bool IsOutOfBoundsX(float x)
    {
        return x < bottomLeft.x || x >= bottomRight.x;
    }

    bool IsOutOfBoundsY(float y)
    {
        return y < bottomLeft.y;
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
        x -= Mathf.FloorToInt(bottomLeft.x);
        y -= Mathf.FloorToInt(bottomLeft.y);
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
        bottomLeft = transform.position;
        bottomRight = bottomLeft + Vector3.right * gridWidth;
        topLeft = transform.position + Vector3.up * gridHeight;
        topRight = topLeft + Vector3.right * gridWidth;
        spawnPos = new Vector3(topLeft.x + (topRight.x - topLeft.x) / 2, topLeft.y, topLeft.z);
    }

    void DrawDebugLines()
    {    
        Debug.DrawLine(topLeft, topRight);
        Debug.DrawLine(topRight, bottomRight);
        Debug.DrawLine(bottomLeft, bottomRight);
        Debug.DrawLine(topLeft, bottomLeft);
    }
}
