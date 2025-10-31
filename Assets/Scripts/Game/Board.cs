
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Colors
{
    public static readonly Color BLUE   = new Color32(0, 28, 135, 255);
    public static readonly Color RED    = new Color32(255, 0, 0, 255);
    public static readonly Color YELLOW = new Color32(255, 255, 0, 255);
}

public class Board : MonoBehaviour
{
    private const int rows = 6;
    private const int cols = 7;

    private NegaMax ai;

    private GameObject[,] board = new GameObject[rows, cols];

    private bool isPlayerTurn = true; // true = Red, false = Yellow
    private bool aiThinking = false;

    [SerializeField] private NextToken nextToken;
    [SerializeField] private Connect4 connect4;

    private void Start()
    {
        ai = new NegaMax();
        connect4 = GetComponent<Connect4>();
        nextToken = FindFirstObjectByType<NextToken>();

        int r = 0, c = 0;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Image>())
            {
                child.GetComponent<Image>().color = Colors.BLUE;
                board[r, c] = child.gameObject;

                c++;
                if (c >= cols)
                {
                    c = 0;
                    r++;
                }

                if (r >= rows) break;
            }
        }
    }

    private void Update()
    {
        // AI turn
        if (!isPlayerTurn && !aiThinking)
        {
            aiThinking = true;
            StartCoroutine(AIMove());
        }
    }

    private IEnumerator AIMove()
    {
        yield return new WaitForSeconds(0.5f);

        Vector2Int aiMove = ai.GetBestMove(this, Colors.YELLOW);
        if (aiMove.y != -1)
        {
            string columnTag = "C" + (aiMove.y + 1);
            PutToken(columnTag);
        }

        aiThinking = false;
    }

    private int TagToColumn(string tag)
    {
        switch (tag)
        {
            case "C1": return 0;
            case "C2": return 1;
            case "C3": return 2;
            case "C4": return 3;
            case "C5": return 4;
            case "C6": return 5;
            case "C7": return 6;
            default: return -1;
        }
    }

    public void PutToken(string columnTag)
    {
        int columnIndex = TagToColumn(columnTag);
        if (columnIndex == -1) return;

        for (int r = rows - 1; r >= 0; r--)
        {
            if (board[r, columnIndex].GetComponent<Image>().color == Colors.BLUE)
            {
                Color playerColor = isPlayerTurn ? Colors.RED : Colors.YELLOW;
                board[r, columnIndex].GetComponent<Image>().color = playerColor;

                if (CheckConnection(r, columnIndex, playerColor))
                {
                    connect4.Results(GetWinner(playerColor));
                    nextToken.ActivateNextToken(false);
                    return;
                }

                // Switch turn
                isPlayerTurn = !isPlayerTurn;
                nextToken.ChangeTurn(isPlayerTurn);

                return;
            }
        }

        Debug.Log("Column full: " + columnTag);
        connect4.Results("draw");
    }

    public bool CheckConnection(int row, int col, Color color)
    {
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1)
        };

        foreach (var dir in directions)
        {
            int count = 1;
            count += CountDirection(row, col, dir.x, dir.y, color);
            count += CountDirection(row, col, -dir.x, -dir.y, color);
            if (count >= 4) return true;
        }

        return false;
    }

    private int CountDirection(int row, int col, int rowStep, int colStep, Color color)
    {
        int count = 0;
        while (true)
        {
            row += rowStep;
            col += colStep;
            if (row < 0 || row >= rows || col < 0 || col >= cols) break;
            if (board[row, col].GetComponent<Image>().color != color) break;
            count++;
        }
        return count;
    }

    public string GetWinner(Color color)
    {
        if (color == Colors.YELLOW) return "yellow";
        if (color == Colors.RED) return "red";
        return "draw";
    }

    // --- AI helper functions ---

    public List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        for (int c = 0; c < cols; c++)
        {
            for (int r = rows - 1; r >= 0; r--)
            {
                if (board[r, c].GetComponent<Image>().color == Colors.BLUE)
                {
                    moves.Add(new Vector2Int(r, c));
                    break;
                }
            }
        }
        return moves;
    }

    public void MakeMove(Vector2Int move, Color color)
    {
        board[move.x, move.y].GetComponent<Image>().color = color;
    }

    public Color GetPiece(Vector2Int move)
    {
        return board[move.x, move.y].GetComponent<Image>().color;
    }

    public Board Copy()
    {
        Board copy = new Board();
        copy.board = new GameObject[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // We only need the color values for AI calculations
                GameObject dummy = new GameObject();
                Image img = dummy.AddComponent<Image>();
                img.color = board[r, c].GetComponent<Image>().color;
                copy.board[r, c] = dummy;
            }
        }

        return copy;
    }
    public Color[,] CopyBoardColors()
    {
        Color[,] copy = new Color[6, 7];

        for (int r = 0; r < 6; r++)
        {
            for (int c = 0; c < 7; c++)
            {
                copy[r, c] = board[r, c].GetComponent<Image>().color;
            }
        }

        return copy;
    }
}
