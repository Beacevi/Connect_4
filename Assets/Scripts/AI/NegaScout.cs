using UnityEngine;

public class NegaScout : IAConnect4Base
{
    private const int MAX_DEPTH = 6;
    private const int INF = 10000000;
    private readonly int[] columnOrder = { 3, 4, 2, 5, 1, 6, 0 };

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
            int score = -NegaScoutf(grid, MAX_DEPTH - 1, -currentPlayer, -INF, INF);
            grid[row, col] = 0;

            if (score > bestScore || bestCol == -1)
            {
                bestScore = score;
                bestCol = col;
            }
        }

        return bestCol == -1 ? new Vector2Int(-1, -1) : new Vector2Int(board.GetRow(bestCol), bestCol);
    }

    private int NegaScoutf(int[,] grid, int depth, int player, int alpha, int beta)
    {
        NodesVisited++;
        if (depth == 0 || IsTerminal(grid)) return Evaluate(grid, player);

        int bestValue = int.MinValue;
        int b = beta;
        bool first = true;

        foreach (int col in columnOrder)
        {
            int row = GetPlayableRow(grid, col);
            if (row == -1) continue;

            grid[row, col] = player;
            int val;

            if (first)
            {
                val = -NegaScoutf(grid, depth - 1, -player, -b, -alpha);
            }
            else
            {
                val = -NegaScoutf(grid, depth - 1, -player, -alpha - 1, -alpha);
                if (val > alpha && val < beta)
                    val = -NegaScoutf(grid, depth - 1, -player, -beta, -val);
            }

            grid[row, col] = 0;

            if (val > bestValue) bestValue = val;
            if (bestValue > alpha) alpha = bestValue;
            if (alpha >= beta) break;

            first = false;
            b = alpha + 1;
        }

        return bestValue;
    }
}


