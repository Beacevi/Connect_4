using System.Collections.Generic;
using UnityEngine;

public class NegaMaxAB : IAConnect4
{
    private const int WIN_SCORE = 1000000;
    private int maxDepth = 5;  // You can push depth higher now with Alpha-Beta
    private readonly int[] moveOrder = { 3, 2, 4, 1, 5, 0, 6 };
    public Vector2Int GetBestMove(Board board)
    {
        int[,] grid = board.CopyBoard();

        int bestMove = -1;
        int bestScore = int.MinValue;

        foreach (int col in moveOrder)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, 1); // AI = +1
            int score = -negaMaxAB(grid, maxDepth - 1, -1, board, int.MinValue + 1, int.MaxValue - 1);
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

    private int negaMaxAB(int[,] grid, int depth, int player, Board board, int alpha, int beta)
    {
        int eval = Evaluate(grid);

        if (Mathf.Abs(eval) == WIN_SCORE || depth == 0)
            return eval * player; // Perspective flip

        int best = -99999999;

        for (int col = 0; col < BoardCapacity.cols; col++)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, player);
            int val = -negaMaxAB(grid, depth - 1, -player, board, -beta, -alpha);
            board.Undo(grid, col, row);

            if (val > best) best = val;
            if (best > alpha) alpha = best;
            if (alpha >= beta) break; // Poda
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

        //  Vertical 
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows - 3; r++)
                CountLine(g[r, c] + g[r + 1, c] + g[r + 2, c] + g[r + 3, c], count);

        //  Diagonal  
        for (int r = 0; r < rows - 3; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r + 1, c + 1] + g[r + 2, c + 2] + g[r + 3, c + 3], count);

        //  Diagonal 
        for (int r = 3; r < rows; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r - 1, c + 1] + g[r - 2, c + 2] + g[r - 3, c + 3], count);


        if (count[8] > 0) return WIN_SCORE;     // 4 en línea IA
        if (count[0] > 0) return -WIN_SCORE;    // 4 en línea jugador

        // Heurística 
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
        if      (val ==  4) c[8]++;
        else if (val == -4) c[0]++;
        else if (val ==  3) c[7]++;
        else if (val == -3) c[1]++;
        else if (val ==  2) c[6]++;
        else if (val == -2) c[2]++;
        else if (val ==  1) c[5]++;
        else if (val == -1) c[3]++;
    }
}