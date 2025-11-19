public static class BoardEvaluator
{
    public const int WIN_SCORE = 1000000;

    public static int Evaluate(int[,] g)
    {
        int score = 0;
        for (int r = 0; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols - 3; c++)
                score += EvaluateWindow(r, c, 0, 1, g);

        for (int c = 0; c < BoardCapacity.cols; c++)
            for (int r = 0; r < BoardCapacity.rows - 3; r++)
                score += EvaluateWindow(r, c, 1, 0, g);

        for (int r = 0; r < BoardCapacity.rows - 3; r++)
            for (int c = 0; c < BoardCapacity.cols - 3; c++)
                score += EvaluateWindow(r, c, 1, 1, g);

        for (int r = 3; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols - 3; c++)
                score += EvaluateWindow(r, c, -1, 1, g);

        return score;
    }

    private static int EvaluateWindow(int r, int c, int dr, int dc, int[,] g)
    {
        int ai = 0, opp = 0, empty = 0;
        int emptyRow = -1, emptyCol = -1;

        for (int i = 0; i < 4; i++)
        {
            int val = g[r + dr * i, c + dc * i];
            if (val == 1) ai++;
            else if (val == -1) opp++;
            else { empty++; emptyRow = r + dr * i; emptyCol = c + dc * i; }
        }

        if (ai == 4) return WIN_SCORE;
        if (opp == 4) return -WIN_SCORE;

        if (ai == 3 && empty == 1 && CanDrop(emptyRow, emptyCol, g))
            return WIN_SCORE - 10;

        if (opp == 3 && empty == 1 && CanDrop(emptyRow, emptyCol, g))
            return -WIN_SCORE + 10;

        if (empty == 1 && CanDrop(emptyRow, emptyCol, g))
        {
            if (ai >= 1 && CreatesDoubleThreat(g, emptyRow, emptyCol, 1))
                return WIN_SCORE - 5;
            if (opp >= 1 && CreatesDoubleThreat(g, emptyRow, emptyCol, -1))
                return -WIN_SCORE + 5;
        }

        return ai * ai * ai - opp * opp * opp;
    }

    public static int CountImmediateWins(int[,] g, int player)
    {
        int wins = 0;
        for (int c = 0; c < BoardCapacity.cols; c++)
        {
            int r = GetPlayableRow(g, c);
            if (r == -1) continue;
            g[r, c] = player;
            if (IsWinningMove(g, r, c, player)) wins++;
            g[r, c] = 0;
        }
        return wins;
    }

    public static bool CreatesDoubleThreat(int[,] g, int row, int col, int player)
    {
        g[row, col] = player;
        int cnt = CountImmediateWins(g, player);
        g[row, col] = 0;
        return cnt >= 2;
    }

    public static int GetPlayableRow(int[,] g, int col)
    {
        for (int r = BoardCapacity.rows - 1; r >= 0; r--)
            if (g[r, col] == 0) return r;
        return -1;
    }

    public static bool IsWinningMove(int[,] g, int row, int col, int player)
    {
        return CountDirection(g, row, col, 1, 0, player) + CountDirection(g, row, col, -1, 0, player) >= 3 ||
               CountDirection(g, row, col, 0, 1, player) + CountDirection(g, row, col, 0, -1, player) >= 3 ||
               CountDirection(g, row, col, 1, 1, player) + CountDirection(g, row, col, -1, -1, player) >= 3 ||
               CountDirection(g, row, col, 1, -1, player) + CountDirection(g, row, col, -1, 1, player) >= 3;
    }

    private static int CountDirection(int[,] g, int r, int c, int dr, int dc, int player)
    {
        int cnt = 0;
        for (int i = 1; i < 4; i++)
        {
            int nr = r + dr * i;
            int nc = c + dc * i;
            if (nr < 0 || nr >= BoardCapacity.rows || nc < 0 || nc >= BoardCapacity.cols) break;
            if (g[nr, nc] != player) break;
            cnt++;
        }
        return cnt;
    }

    private static bool CanDrop(int row, int col, int[,] g)
    {
        if (row == BoardCapacity.rows - 1) return true;
        return g[row + 1, col] != 0;
    }

    public static int EvaluateColumn(int[,] g, int col, int player)
    {
        int row = GetPlayableRow(g, col);
        if (row == -1) return 0;
        g[row, col] = player;
        int score = Evaluate(g);
        g[row, col] = 0;
        return score;
    }

    public static int CountSeparatedThreats(int[,] g, int player)
    {
        int threats = 0;
        for (int r = 0; r < BoardCapacity.rows; r++)
        {
            for (int c = 0; c < BoardCapacity.cols - 2; c++)
            {
                if (g[r, c] == player && g[r, c + 2] == player && g[r, c + 1] == 0)
                {
                    int er = r, ec = c + 1;
                    if (CanDrop(er, ec, g)) threats++;
                }
            }
        }

        for (int c = 0; c < BoardCapacity.cols; c++)
        {
            for (int r = 0; r < BoardCapacity.rows - 2; r++)
            {
                if (g[r, c] == player && g[r + 2, c] == player && g[r + 1, c] == 0)
                {
                    int er = r + 1, ec = c;
                    if (CanDrop(er, ec, g)) threats++;
                }
            }
        }

        for (int r = 0; r < BoardCapacity.rows - 2; r++)
        {
            for (int c = 0; c < BoardCapacity.cols - 2; c++)
            {
                if (g[r, c] == player && g[r + 2, c + 2] == player && g[r + 1, c + 1] == 0)
                {
                    int er = r + 1, ec = c + 1;
                    if (CanDrop(er, ec, g)) threats++;
                }
                if (g[r + 2, c] == player && g[r, c + 2] == player && g[r + 1, c + 1] == 0)
                {
                    int er = r + 1, ec = c + 1;
                    if (CanDrop(er, ec, g)) threats++;
                }
            }
        }

        return threats;
    }

    public static ulong ComputeZobristHash(int[,] grid, ulong[,] table)
    {
        ulong h = 0;
        for (int r = 0; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols; c++)
                if (grid[r, c] != 0)
                    h ^= table[r * BoardCapacity.cols + c, grid[r, c] == 1 ? 0 : 1];
        return h;
    }
}
