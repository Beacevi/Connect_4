using UnityEngine;
using System.Diagnostics;

public abstract class IAConnect4Base : IAConnect4
{
    public long NodesVisited { get; protected set; }

    private const int WIN_SCORE = 100000;

    // Métricas
    public Vector2Int GetBestMove(Board board)
    {
        NodesVisited = 0;
        Stopwatch stopwatch = Stopwatch.StartNew();

        Vector2Int move = GetBestMoveInternal(board);

        stopwatch.Stop();
        UnityEngine.Debug.Log($"{this.GetType().Name} - Nodos visitados: {NodesVisited}");
        UnityEngine.Debug.Log($"{this.GetType().Name} - Tiempo de ejecución: {stopwatch.ElapsedMilliseconds} ms");

        return move;
    }

    protected abstract Vector2Int GetBestMoveInternal(Board board);


    // Métodos comunes para IA
    protected bool BoardIsRedTurn(Board board)
    {
        int red = 0, yellow = 0;
        int[,] g = board.CopyBoard();
        foreach (int v in g)
        {
            if (v == -1) red++;
            if (v == 1) yellow++;
        }
        return red == yellow;
    }

    protected int GetPlayableRow(int[,] g, int col)
    {
        for (int r = BoardCapacity.rows - 1; r >= 0; r--)
            if (g[r, col] == 0) return r;
        return -1;
    }

    protected bool IsTerminal(int[,] grid)
    {
        int score = Evaluate(grid, 1);
        return Mathf.Abs(score) >= WIN_SCORE || IsFull(grid);
    }

    protected bool IsFull(int[,] grid)
    {
        for (int c = 0; c < BoardCapacity.cols; c++)
            if (grid[0, c] == 0) return false;
        return true;
    }

    protected int Evaluate(int[,] g, int player)
    {
        int score = 0;


        for (int r = 0; r < BoardCapacity.rows; r++)
            if (g[r, 3] == player) score += 6;


        for (int r = 0; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols - 3; c++)
                score += EvaluateWindow(new int[] { g[r, c], g[r, c + 1], g[r, c + 2], g[r, c + 3] }, player);


        for (int r = 0; r < BoardCapacity.rows - 3; r++)
            for (int c = 0; c < BoardCapacity.cols; c++)
                score += EvaluateWindow(new int[] { g[r, c], g[r + 1, c], g[r + 2, c], g[r + 3, c] }, player);


        for (int r = 0; r < BoardCapacity.rows - 3; r++)
            for (int c = 0; c < BoardCapacity.cols - 3; c++)
                score += EvaluateWindow(new int[] { g[r, c], g[r + 1, c + 1], g[r + 2, c + 2], g[r + 3, c + 3] }, player);


        for (int r = 3; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols - 3; c++)
                score += EvaluateWindow(new int[] { g[r, c], g[r - 1, c + 1], g[r - 2, c + 2], g[r - 3, c + 3] }, player);

        return score;
    }

    protected int EvaluateWindow(int[] w, int player)
    {
        int opp = -player;
        int p = 0, o = 0, empty = 0;

        foreach (var v in w)
        {
            if (v == player) p++;
            else if (v == opp) o++;
            else empty++;
        }

        if (p == 4) return 100000;
        if (o == 4) return -100000;
        if (p == 3 && empty == 1) return 100;
        if (o == 3 && empty == 1) return -80;
        if (p == 2 && empty == 2) return 10;
        if (o == 2 && empty == 2) return -8;

        return 0;
    }

    protected bool EvaluateWindowForWin(int[,] g, int player, int row, int col)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(1,1),
            new Vector2Int(1,-1)
        };

        foreach (var dir in directions)
        {
            int count = 1;
            for (int step = 1; step < 4; step++)
            {
                int r = row + dir.x * step;
                int c = col + dir.y * step;
                if (r < 0 || r >= BoardCapacity.rows || c < 0 || c >= BoardCapacity.cols) break;
                if (g[r, c] == player) count++; else break;
            }
            for (int step = 1; step < 4; step++)
            {
                int r = row - dir.x * step;
                int c = col - dir.y * step;
                if (r < 0 || r >= BoardCapacity.rows || c < 0 || c >= BoardCapacity.cols) break;
                if (g[r, c] == player) count++; else break;
            }
            if (count >= 4) return true;
        }
        return false;
    }
}

