
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

    private IAConnect4 ai;

    private GameObject[,] board = new GameObject[rows, cols];

    private bool isPlayerTurn = true; // true = Red, false = Yellow
    private bool aiThinking   = false;
    private bool gameOver = false;

    private NextToken nextToken;
    private Connect4  connect4;


    private void AiType(int type)
    {
        if (type == 0)
            ai = new NegaMax();
        else if (type == 1)
            ai = new NegaMaxAB();
 
    }

    void Start()
    {
        //ai = new NegaMax();
        //ai = new NegaMaxAB();
        
        connect4 = GetComponent<Connect4>();
        nextToken = FindFirstObjectByType<NextToken>();

        AiType(connect4.aiType);

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
        if (gameOver) return;
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
                    gameOver = true;
                }
                else if (IsBoardFull())
                {
                    connect4.Results("draw");
                    nextToken.ActivateNextToken(false);
                    gameOver = true;
                }
                else
                {
                    // Switch turn
                    isPlayerTurn = !isPlayerTurn;
                    nextToken.ChangeTurn(isPlayerTurn);
                }
                    

                return;
            }
        }

        Debug.Log("Column full: " + columnTag);
        return;
    }

    public bool CheckConnection(int row, int col, Color color)
    {
        Vector2Int[] directions = 
        {
            new Vector2Int(0, 1), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1)
        };

        foreach (var dir in directions)
        {
            int count = 1;

            count += CountDirection(row, col,  dir.x,  dir.y, color);
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

        if (color == Colors.RED   ) return "red";

        return "draw";
    }


    public Color[,] CopyBoardColors()
    {
        Color[,] copy = new Color[6, 7];

        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 7; c++)
                copy[r, c] = board[r, c].GetComponent<Image>().color;

        return copy;
    }

    public List<Vector2Int> GetValidMoves(Color[,] boardColors)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        for (int c = 0; c < 7; c++)
        {
            for (int r = 5; r >= 0; r--)
            {
                if (boardColors[r, c] == Colors.BLUE)
                {
                    moves.Add(new Vector2Int(r, c));
                    break;
                }
            }
        }

        return moves;
    }

    public bool CheckConnection(Color[,] boardColors, int row, int col, Color color)
    {
        Vector2Int[] directions = 
        {
            new Vector2Int(0, 1), new Vector2Int(1,  0),
            new Vector2Int(1, 1), new Vector2Int(1, -1)
        };

        foreach (var dir in directions)
        {
            int count = 1;

            count += CountDirection(boardColors, row, col, dir.x, dir.y, color);
            count += CountDirection(boardColors, row, col, -dir.x, -dir.y, color);

            if (count >= 4) return true;
        }
        return false;
    }

    public int CountDirection(Color[,] boardColors, int row, int col, int rowStep, int colStep, Color color)
    {
        int count = 0;
        while (true)
        {
            row += rowStep;
            col += colStep;

            if (row < 0 || row >= 6 || col < 0 || col >= 7) break;

            if (boardColors[row, col] != color) break;
            count++;
        }
        return count;
    }

    public Color[,] CopyBoard(Color[,] boardColors)
    {
        Color[,] copy = new Color[6, 7];

        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 7; c++)
                copy[r, c] = boardColors[r, c];

        return copy;
    }

    private bool IsBoardFull()
    {
        for (int c = 0; c < cols; c++)
        {
            if (board[0, c].GetComponent<Image>().color == Colors.BLUE)
                return false;
        }
        return true;
    }
}
