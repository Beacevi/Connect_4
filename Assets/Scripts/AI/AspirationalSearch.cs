using UnityEngine;

public class AspirationalSearch : IAConnect4Base
{
    private const int MAX_DEPTH = 6;
    private const int INF = 10000000;
    private const int DEFAULT_WINDOW = 50;
    private readonly int[] columnOrder = { 3, 4, 2, 5, 1, 6, 0 };

    private int previousScore = 0;

    protected override Vector2Int GetBestMoveInternal(Board board)
    {
        int[,] grid = board.CopyBoard();
        int currentPlayer = BoardIsRedTurn(board) ? -1 : 1;
        int bestScore = int.MinValue;
        int bestCol = -1;

        foreach (int col in columnOrder)
        {
            int row = GetPlayableRow(grid, col);
            if (row == -1) continue;

            grid[row, col] = currentPlayer;

            int alpha, beta;
            if (previousScore != 0)
            {
                alpha = previousScore - DEFAULT_WINDOW;
                beta = previousScore + DEFAULT_WINDOW;
            }
            else
            {
                alpha = -INF;
                beta = INF;
            }

            int score;
            while (true)
            {
                score = -AspirationalSearchf(grid, MAX_DEPTH - 1, -currentPlayer, -beta, -alpha);
                if (score <= alpha) alpha = -INF;
                else if (score >= beta) beta = INF;
                else break;
            }

            previousScore = score;
            grid[row, col] = 0;

            if (score > bestScore || bestCol == -1)
            {
                bestScore = score;
                bestCol = col;
            }
        }

        return new Vector2Int(board.GetRow(bestCol), bestCol);
    }

    private int AspirationalSearchf(int[,] grid, int depth, int player, int alpha, int beta)
    {
        NodesVisited++;
        if (depth == 0 || IsTerminal(grid))
            return Evaluate(grid, player);

        int bestValue = int.MinValue;

        foreach (int col in columnOrder)
        {
            int row = GetPlayableRow(grid, col);
            if (row == -1) continue;

            grid[row, col] = player;
            int value = -AspirationalSearchf(grid, depth - 1, -player, -beta, -alpha);
            grid[row, col] = 0;

            if (value > bestValue) bestValue = value;
            if (bestValue > alpha) alpha = bestValue;
            if (alpha >= beta) break;
        }

        return bestValue;
    }
}
