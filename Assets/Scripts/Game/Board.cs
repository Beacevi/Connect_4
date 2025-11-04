using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Colors
{
    public static readonly Color BLUE = new Color32(0, 28, 135, 255);
    public static readonly Color RED = new Color32(255, 0, 0, 255);
    public static readonly Color YELLOW = new Color32(255, 255, 0, 255);
}

public static class BoardCapacity
{
    public const int rows = 6;
    public const int cols = 7;
}

public class Board : MonoBehaviour
{
    private IAConnect4 ai;

    private GameObject[,] board = new GameObject[BoardCapacity.rows, BoardCapacity.cols];

    private bool isPlayerTurn = true; // true = Red, false = Yellow
    private bool aiThinking = false;
    private bool gameOver = false;

    private NextToken nextToken;
    private Connect4 connect4;

    // Initialize AI type
    public void AiType(int type)
    {
        switch (type)
        {
            case 0:  ai = new NegaMax(); break;
            case 1:  ai = new NegaMaxAB(); break;
            case 2:  ai = new MTD(); break;
            case 3: ai = new NegaScout(); break;
            case 4: ai = new AspirationalSearch(); break;
            default: ai = new NegaMax(); break;
        }
    }

    // Initialize board with colors
    void Start()
    {
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
                if (c >= BoardCapacity.cols)
                {
                    c = 0;
                    r++;
                }

                if (r >= BoardCapacity.rows) break;
            }
        }
    }

    // Update logic for AI turns
    private void Update()
    {
        if (gameOver) return;
        if (!isPlayerTurn && !aiThinking)
        {
            aiThinking = true;
            StartCoroutine(AIMove());
        }
    }

    // AI move coroutine
    private IEnumerator AIMove()
    {
        yield return new WaitForSeconds(0.5f);
        Vector2Int aiMove = ai.GetBestMove(this);
        if (aiMove.y != -1)
        {
            string columnTag = "C" + (aiMove.y + 1);
            PutToken(columnTag);
        }
        aiThinking = false;
    }

    // Convert tag to column index
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

    // Place token in the specified column
    public void PutToken(string columnTag)
    {
        int columnIndex = TagToColumn(columnTag);
        if (columnIndex == -1) return;

        for (int r = BoardCapacity.rows - 1; r >= 0; r--)
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
                    isPlayerTurn = !isPlayerTurn;
                    nextToken.ChangeTurn(isPlayerTurn);
                }
                return;
            }
        }
        Debug.Log("Column full: " + columnTag);
        return;
    }

    // Check if a player has connected 4 tokens
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
            count += CountDirection(row, col, dir.x, dir.y, color);
            count += CountDirection(row, col, -dir.x, -dir.y, color);

            if (count >= 4) return true;
        }
        return false;
    }

    // Count tokens in a given direction
    private int CountDirection(int row, int col, int rowstep, int colstep, Color color)
    {
        int count = 0;
        while (true)
        {
            row += rowstep;
            col += colstep;

            if (row < 0 || row >= BoardCapacity.rows || col < 0 || col >= BoardCapacity.cols) break;
            if (board[row, col].GetComponent<Image>().color != color) break;
            count++;
        }
        return count;
    }

    // Get the winner string based on color
    public string GetWinner(Color color)
    {
        return color == Colors.YELLOW ? "yellow" : color == Colors.RED ? "red" : "draw";
    }

    
    public bool IsBoardFull()
    {
        for (int c = 0; c < BoardCapacity.cols; c++)
        {
            if (board[0, c].GetComponent<Image>().color == Colors.BLUE)
                return false;
        }
        return true;
    }

    public bool CanPlay(int col)
    {
        return board[0, col].GetComponent<Image>().color == Colors.BLUE;
    }

    public int GetRow(int col)
    {
        for (int r = BoardCapacity.rows - 1; r >= 0; r--)
            if (board[r, col].GetComponent<Image>().color == Colors.BLUE) return r;
        return -1;
    }

    
    public int Play(int col, int[,] IAgrid, int IAnum)
    {
        for (int row = BoardCapacity.rows - 1; row >= 0; row--)
        {
            if (IAgrid[row, col] == 0)
            {
                IAgrid[row, col] = IAnum;
                board[row, col].GetComponent<Image>().color = IAnum == 1 ? Colors.YELLOW : Colors.RED;  // AI = Yellow, Player = Red
                return row;
            }
        }
        return -1;
    }

    public void Undo(int[,] IAgrid, int col, int row)
    {
        board[row, col].GetComponent<Image>().color = Colors.BLUE;

        IAgrid[row, col] = 0;
    }


    public int[,] CopyBoard()
    {
        int[,] copy = new int[BoardCapacity.rows, BoardCapacity.cols];

        for (int r = 0; r < BoardCapacity.rows; r++)
        {
            for (int c = 0; c < BoardCapacity.cols; c++)
            {
                var color = board[r, c].GetComponent<Image>().color;
                if (color == Colors.RED) copy[r, c] = -1;  // Player
                else if (color == Colors.YELLOW) copy[r, c] = 1;  // AI
                else copy[r, c] = 0;  // Empty
            }
        }
        return copy;
    }
}