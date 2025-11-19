using UnityEngine;

public class MTD : IAConnect4Base
{
    private const int MAX_DEPTH = 6;
    private const int INF = 10000000;
    private readonly int[] columnOrder = { 3, 4, 2, 5, 1, 6, 0 };

    protected override Vector2Int GetBestMoveInternal(Board board)
    {
        int[,] grid = board.CopyBoard();
        int currentPlayer = BoardIsRedTurn(board) ? -1 : 1;

        int guess = 0;
        int bestScore = int.MinValue;
        int bestCol = -1;

        foreach (int col in columnOrder)
        {
            int row = GetPlayableRow(grid, col);
            if (row == -1) continue;

            grid[row, col] = currentPlayer;
            int score = MTDf(grid, MAX_DEPTH - 1, guess, currentPlayer);
            grid[row, col] = 0;

            if (score > bestScore || bestCol == -1)
            {
                bestScore = score;
                bestCol = col;
                guess = score;
            }
        }

        return new Vector2Int(board.GetRow(bestCol), bestCol);
    }

    private int MTDf(int[,] grid, int depth, int guess, int player)
    {
        int g = guess;
        int upperBound = INF;
        int lowerBound = -INF;

        while (lowerBound < upperBound)
        {
            int beta = (g == lowerBound) ? g + 1 : g;
            g = -NegamaxABf(grid, depth, -beta, -Mathf.Max(lowerBound, -INF), -player);
            if (g < beta) upperBound = g;
            else lowerBound = g;
        }

        return g;
    }

    private int NegamaxABf(int[,] grid, int depth, int alpha, int beta, int player)
    {
        NodesVisited++;
        if (depth == 0 || IsTerminal(grid)) return Evaluate(grid, player);

        int bestValue = int.MinValue;
        foreach (int col in columnOrder)
        {
            int row = GetPlayableRow(grid, col);
            if (row == -1) continue;

            grid[row, col] = player;
            int value = -NegamaxABf(grid, depth - 1, -beta, -alpha, -player);
            grid[row, col] = 0;

            if (value > bestValue) bestValue = value;
            if (bestValue > alpha) alpha = bestValue;
            if (alpha >= beta) break;
        }
        return bestValue;
    }
}

