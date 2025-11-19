using UnityEngine;

public class NegaMax : IAConnect4Base
{
    private const int MAX_DEPTH = 6;
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
            int score = -Negamaxf(grid, MAX_DEPTH - 1, -currentPlayer);
            grid[row, col] = 0;

            if (score > bestScore || bestCol == -1)
            {
                bestScore = score;
                bestCol = col;
            }
        }

        return new Vector2Int(board.GetRow(bestCol), bestCol);
    }

    private int Negamaxf(int[,] grid, int depth, int player)
    {
        NodesVisited++;
        if (depth == 0 || IsTerminal(grid))
            return Evaluate(grid, player);

        int best = int.MinValue;

        foreach (int col in columnOrder)
        {
            int row = GetPlayableRow(grid, col);
            if (row == -1) continue;

            grid[row, col] = player;
            int score = -Negamaxf(grid, depth - 1, -player);
            grid[row, col] = 0;

            if (score > best) best = score;
        }

        return best;
    }
}

