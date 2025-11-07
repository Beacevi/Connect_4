using System.Collections.Generic;
using UnityEngine;

public class NegaScout : IAConnect4
{
    private const int WIN_SCORE = 1000000;
    private int maxDepth = 6;
    private readonly int[] moveOrder = { 3, 2, 4, 1, 5, 0, 6 };

    public Vector2Int GetBestMove(Board board)
    {
        int[,] grid = board.CopyBoard();
        int bestMove = -1;
        int bestScore = int.MinValue;

        foreach (int col in moveOrder)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, 1); // IA = +1
            int score = -Negascout(grid, maxDepth - 1, -1, board, int.MinValue + 1, int.MaxValue - 1);
            board.Undo(grid, col, row);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }
        }

        if (bestMove == -1) return new Vector2Int(-1, -1);

        int dropRow = board.GetRow(bestMove);
        return new Vector2Int(dropRow, bestMove);
    }

    private int Negascout(int[,] grid, int depth, int player, Board board, int alpha, int beta)
    {
        int eval = Evaluate(grid);
        if (Mathf.Abs(eval) == WIN_SCORE || depth == 0)
            return eval * player;

        int best = int.MinValue;
        bool firstChild = true;
        int a = alpha;

        foreach (int col in moveOrder)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, player);
            int val;

            if (firstChild)
            {
                // Búsqueda completa para el primer movimiento
                val = -Negascout(grid, depth - 1, -player, board, -beta, -a);
            }
            else
            {
                // Scout search
                val = -Negascout(grid, depth - 1, -player, board, -a - 1, -a);

                // Si parece prometedor, reexplorar con ventana completa
                if (val > a && val < beta)
                    val = -Negascout(grid, depth - 1, -player, board, -beta, -val);
            }

            board.Undo(grid, col, row);

            if (val > best) best = val;
            if (best > a) a = best;
            if (a >= beta) break; // poda
            firstChild = false;
        }

        return best;
    }

    private int Evaluate(int[,] g)
    {
        int[] count = new int[9];
        int rows = BoardCapacity.rows;
        int cols = BoardCapacity.cols;

        // Horizontal
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r, c + 1] + g[r, c + 2] + g[r, c + 3], count);

        // Vertical
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows - 3; r++)
                CountLine(g[r, c] + g[r + 1, c] + g[r + 2, c] + g[r + 3, c], count);

        // Diagonal 
        for (int r = 0; r < rows - 3; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r + 1, c + 1] + g[r + 2, c + 2] + g[r + 3, c + 3], count);

        // Diagonal 
        for (int r = 3; r < rows; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r - 1, c + 1] + g[r - 2, c + 2] + g[r - 3, c + 3], count);

        if (count[8] > 0) return WIN_SCORE;
        if (count[0] > 0) return -WIN_SCORE;

        int score = -count[1] * 6 - count[2] * 3 - count[3]
                    + count[7] * 6 + count[6] * 3 + count[5];

        int centerCol = cols / 2;
        for (int r = 0; r < rows; r++)
        {
            if (g[r, centerCol] == 1) score += 3;
            else if (g[r, centerCol] == -1) score -= 3;
        }

        return score;
    }

    private void CountLine(int val, int[] c)
    {
        switch (val)
        {
            case 4: c[8]++; break;
            case -4: c[0]++; break;
            case 3: c[7]++; break;
            case -3: c[1]++; break;
            case 2: c[6]++; break;
            case -2: c[2]++; break;
            case 1: c[5]++; break;
            case -1: c[3]++; break;
        }
    }
}
