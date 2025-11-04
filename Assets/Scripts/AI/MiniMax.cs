using UnityEngine;

public class Minimax : IAConnect4
{
    private const int WIN_SCORE = 1000000;
    private int maxDepth = 4;
    // Get the best move for AI
    public Vector2Int GetBestMove(Board board)
    {
        int[,] grid = board.CopyBoard();

        int bestMove = -1;
        int bestScore = -99999999;

        for (int col = 0; col < BoardCapacity.cols; col++)
        {
            if (!board.CanPlay(col)) continue;

            int row = board.Play(col, grid, 1); // 1 = AI (Yellow)
            int score = miniMax(grid, maxDepth - 1, false, board);  // Recursively evaluate AI's possible moves
            board.Undo(grid, col, row);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }
        }

        if (bestMove == -1) return new Vector2Int(-1, -1); // No valid move found

        // Convert the column index to the corresponding row (where AI would drop the token)
        int dropRow = board.GetRow(bestMove);
        return new Vector2Int(dropRow, bestMove);
    }

    // Minimax algorithm with recursion
    private int miniMax(int[,] grid, int depth, bool maximizing, Board board)
    {
        
        int eval = Evaluate(grid);
        if (Mathf.Abs(eval) == WIN_SCORE || depth == 0) // If win or max depth reached, stop recursion
            return eval;

        if (maximizing)  // AI's turn (Maximizing player)
        {
            int best = -99999999;
            for (int col = 0; col < BoardCapacity.cols; col++)
            {
                if (!board.CanPlay(col)) continue;
                int row = board.Play(col, grid, 1);  // Play for AI
                best = Mathf.Max(best, miniMax(grid, depth - 1, false, board)); // Minimize for opponent
                board.Undo(grid, col, row);
            }
            return best;
        }
        else  // Player's turn (Minimizing player)
        {
            int best = 99999999;
            for (int col = 0; col < BoardCapacity.cols; col++)
            {
                if (!board.CanPlay(col)) continue;
                int row = board.Play(col, grid, -1);  // Play for Player (Red)
                best = Mathf.Min(best, miniMax(grid, depth - 1, true, board)); // Maximize for AI
                board.Undo(grid, col, row);
            }
            return best;
        }
    }

    // Evaluate the board for AI's move (positive values favor AI, negative favor player)
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

        // Diagonal \
        for (int r = 0; r < rows - 3; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r + 1, c + 1] + g[r + 2, c + 2] + g[r + 3, c + 3], count);

        // Diagonal /
        for (int r = 3; r < rows; r++)
            for (int c = 0; c < cols - 3; c++)
                CountLine(g[r, c] + g[r - 1, c + 1] + g[r - 2, c + 2] + g[r - 3, c + 3], count);

        if (count[8] > 0) return WIN_SCORE;
        if (count[0] > 0) return -WIN_SCORE;

        return -count[1] * 5 - count[2] * 2 - count[3]
               + count[7] * 5 + count[6] * 2 + count[5];
    }

    // Increment counts based on the line value
    private void CountLine(int val, int[] c)
    {
        // Centered around 0 (so we handle both AI and Player scores in the same array)
        if      (val ==  4) c[8]++;   // 4 AI tokens in a row
        else if (val == -4) c[0]++;   // 4 Player tokens in a row
        else if (val ==  3) c[7]++;   // 3 AI tokens + 1 empty space
        else if (val == -3) c[1]++;   // 3 Player tokens + 1 empty space
        else if (val ==  2) c[6]++;   // 2 AI tokens + 2 empty spaces
        else if (val == -2) c[2]++;   // 2 Player tokens + 2 empty spaces
        else if (val ==  1) c[5]++;   // 1 AI token + 3 empty spaces
        else if (val == -1) c[3]++;   // 1 Player token + 3 empty spaces
    }
}
