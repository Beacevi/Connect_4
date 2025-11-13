using System.Collections;
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
    private IAConnect4 aiRed;     // IA que juega como rojo (jugador 1)
    private IAConnect4 aiYellow;  // IA que juega como amarillo (jugador 2)
    private GameObject[,] board = new GameObject[BoardCapacity.rows, BoardCapacity.cols];

    private bool isRedTurn = true;
    private bool aiThinking = false;
    private bool gameOver = false;

    private NextToken nextToken;
    private Connect4 connect4;

    // Inicializa los tipos de IA
    public void SetAiTypes(int ai1, int ai2)
    {
        connect4 = FindFirstObjectByType<Connect4>();

        aiRed = CreateAi(ai1);
        aiYellow = ai2 == -1 ? null : CreateAi(ai2); // si es -1, es el jugador humano
    }

    // Crea el tipo de IA según su código
    private IAConnect4 CreateAi(int type)
    {
        switch (type)
        {
            case 0: return new NegaMax();
            case 1: return new NegaMaxAB();
            case 2: return new MTD();
            case 3: return new NegaScout();
            case 4: return new AspirationalSearch();
            default: return new NegaMax();
        }
    }

    // Inicialización del tablero visual
    private void Start()
    {
        nextToken = FindFirstObjectByType<NextToken>();
        connect4 = GetComponent<Connect4>();

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

    // Turnos automáticos si hay IAs
    private void Update()
    {
        if (gameOver || aiThinking) return;

        bool aiTurn = false;

        if (connect4.PlayerExist)
        {
            // Modo Player vs AI 
            aiTurn = !isRedTurn;
        }
        else
        {
            // Modo AI vs AI
            aiTurn = true;
        }

        if (aiTurn)
        {
            aiThinking = true;
            StartCoroutine(AIMove());
        }
    }

    // Movimiento automático de IA
    private IEnumerator AIMove()
    {
        yield return new WaitForSeconds(0.7f);

        IAConnect4 currentAi = isRedTurn ? aiRed : aiYellow;
        if (currentAi == null)
        {
            aiThinking = false;
            yield break;
        }

        Vector2Int aiMove = currentAi.GetBestMove(this);
        if (aiMove.y != -1)
        {
            string columnTag = "C" + (aiMove.y + 1);
            PutToken(columnTag);
        }

        aiThinking = false;
    }

    // Convierte el tag de la columna al índice numérico
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

    // Coloca una ficha en la columna
    public void PutToken(string columnTag)
    {
        int columnIndex = TagToColumn(columnTag);
        if (columnIndex == -1) return;

        for (int r = BoardCapacity.rows - 1; r >= 0; r--)
        {
            if (board[r, columnIndex].GetComponent<Image>().color == Colors.BLUE)
            {
                Color playerColor = isRedTurn ? Colors.RED : Colors.YELLOW;
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
                    isRedTurn = !isRedTurn;
                    nextToken.ChangeTurn(isRedTurn);
 
                    if (connect4.PlayerExist && !isRedTurn && !aiThinking)
                    {
                        aiThinking = true;
                        StartCoroutine(AIMove());
                    }
                }
                return;
            }
        }

        Debug.Log("Column full: " + columnTag);
    }

    // Comprueba si hay una conexión de 4 fichas
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

    private int CountDirection(int row, int col, int rowstep, int colstep, Color color)
    {
        int count = 0;
        while (true)
        {
            row += rowstep;
            col += colstep;

            if (row < 0 || row >= BoardCapacity.rows || col < 0 || col >= BoardCapacity.cols)
                break;

            if (board[row, col].GetComponent<Image>().color != color)
                break;

            count++;
        }
        return count;
    }

    public string GetWinner(Color color)
    {
        return color == Colors.YELLOW ? "yellow" :
               color == Colors.RED ? "red" : "draw";
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

    // Métodos usados por las IA
    public bool CanPlay(int col)
    {
        return board[0, col].GetComponent<Image>().color == Colors.BLUE;
    }

    public int GetRow(int col)
    {
        for (int r = BoardCapacity.rows - 1; r >= 0; r--)
            if (board[r, col].GetComponent<Image>().color == Colors.BLUE)
                return r;
        return -1;
    }

    public int Play(int col, int[,] IAgrid, int IAnum)
    {
        for (int row = BoardCapacity.rows - 1; row >= 0; row--)
        {
            if (IAgrid[row, col] == 0)
            {
                IAgrid[row, col] = IAnum;
                board[row, col].GetComponent<Image>().color = IAnum == 1 ? Colors.YELLOW : Colors.RED;
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
                if (color == Colors.RED) copy[r, c] = -1;
                else if (color == Colors.YELLOW) copy[r, c] = 1;
                else copy[r, c] = 0;
            }
        }
        return copy;
    }
}