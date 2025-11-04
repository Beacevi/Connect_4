using System.Collections.Generic;
using UnityEngine;

public class MTD : IAConnect4
{
    private const int WIN_SCORE = 1000000;
    private int maxDepth = 6;

    // Transposition table (cache)
    private readonly Dictionary<string, int> transpositionTable = new Dictionary<string, int>();

    // Orden ideal para poda alfa-beta
    private readonly int[] moveOrder = { 3, 2, 4, 1, 5, 0, 6 };

    public Vector2Int GetBestMove(Board board)
    {
        int[,] grid = board.CopyBoard();
        int bestMove = -1;
        int bestScore = int.MinValue;

        // Iteramos las columnas con orden heurístico
        foreach (int col in moveOrder)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, 1); // IA = +1

            // --- MTD(f) search ---
            int guess = 0; // valor inicial (puedes usar la evaluación actual o 0)
            int score = -MTDf(grid, -guess, maxDepth - 1, -1, board);

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


    //  MTD(f) core implementation
    private int MTDf(int[,] grid, int firstGuess, int depth, int player, Board board)
    {
        int g = firstGuess;
        int upperBound = WIN_SCORE;
        int lowerBound = -WIN_SCORE;

        while (lowerBound < upperBound)
        {
            int beta = (g == lowerBound) ? g + 1 : g;
            g = NegaMaxAB(grid, depth, player, board, beta - 1, beta);
            if (g < beta) upperBound = g;
            else lowerBound = g;
        }

        return g;
    }


    private int NegaMaxAB(int[,] grid, int depth, int player, Board board, int alpha, int beta)
    {
        string key = GetHash(grid, depth, player);

        if (transpositionTable.TryGetValue(key, out int cached))
            return cached;

        int eval = Evaluate(grid);
        if (Mathf.Abs(eval) == WIN_SCORE || depth == 0)
        {
            transpositionTable[key] = eval * player;
            return eval * player;
        }

        int best = int.MinValue;

        foreach (int col in moveOrder)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, player);
            int val = -NegaMaxAB(grid, depth - 1, -player, board, -beta, -alpha);
            board.Undo(grid, col, row);

            if (val > best) best = val;
            if (best > alpha) alpha = best;
            if (alpha >= beta) break;
        }

        transpositionTable[key] = best;
        return best;
    }


    //  Board evaluation
    private int Evaluate(int[,] g)
    {
        int[] count = new int[9];
        int rows = BoardCapacity.rows;
        int cols = BoardCapacity.cols;


        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r, c + 1] + g[r, c + 2] + g[r, c + 3], count);

        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows - 3; r++)
                CountLine(g[r, c] + g[r + 1, c] + g[r + 2, c] + g[r + 3, c], count);

        for (int r = 0; r < rows - 3; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r + 1, c + 1] + g[r + 2, c + 2] + g[r + 3, c + 3], count);

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

    //  Simple hashing (for caching)
    private string GetHash(int[,] grid, int depth, int player)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(depth).Append('_').Append(player).Append('_');
        for (int r = 0; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols; c++)
                sb.Append(grid[r, c]);
        return sb.ToString();
    }
}
