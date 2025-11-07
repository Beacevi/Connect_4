using UnityEngine;

public class AspirationalSearch : IAConnect4
{
    private const int WIN_SCORE = 1000000;
    private int maxDepth = 6;
    private int lastScoreGuess = 0;
    private readonly int[] moveOrder = { 3, 2, 4, 1, 5, 0, 6 };
    private int bestCol;

    public Vector2Int GetBestMove(Board board)
    {
        int[,] grid = board.CopyBoard();

        int alpha = lastScoreGuess - 500; // Ventana inicial
        int beta = lastScoreGuess + 500;

        int bestScore = int.MinValue;
        int bestMove = -1;
        int tries = 0;

        // Intentos limitados de expansión de ventana
        while (tries++ < 5)
        {
            int score = SearchWithWindow(board, grid, alpha, beta);

            if (score <= alpha)
            {
                alpha -= 500;
            }
            else if (score >= beta)
            {
                beta += 500;
            }
            else
            {
                bestScore = score;
                bestMove = bestCol;
                lastScoreGuess = score;
                break;
            }
        }

        if (bestMove == -1) bestMove = bestCol;
        int dropRow = board.GetRow(bestMove);
        return new Vector2Int(dropRow, bestMove);
    }

    private int SearchWithWindow(Board board, int[,] grid, int alpha, int beta)
    {
        int bestScore = int.MinValue;
        int bestMoveLocal = -1;

        foreach (int col in moveOrder)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, 1);
            int score = -NegaMaxAB(grid, maxDepth - 1, -1, board, -beta, -alpha);
            board.Undo(grid, col, row);

            if (score > bestScore)
            {
                bestScore = score;
                bestMoveLocal = col;
            }

            if (bestScore > alpha)
                alpha = bestScore;
            if (alpha >= beta)
                break;
        }

        bestCol = bestMoveLocal;
        return bestScore;
    }

    private int NegaMaxAB(int[,] grid, int depth, int player, Board board, int alpha, int beta)
    {
        int eval = Evaluate(grid);
        if (Mathf.Abs(eval) == WIN_SCORE || depth == 0)
            return eval * player;

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

        return best;
    }

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

        return -count[1] * 6 - count[2] * 3 - count[3]
               + count[7] * 6 + count[6] * 3 + count[5];
    }

    private void CountLine(int val, int[] c)
    {
        if (val == 4) c[8]++;
        else if (val == -4) c[0]++;
        else if (val == 3) c[7]++;
        else if (val == -3) c[1]++;
        else if (val == 2) c[6]++;
        else if (val == -2) c[2]++;
        else if (val == 1) c[5]++;
        else if (val == -1) c[3]++;
    }
}
