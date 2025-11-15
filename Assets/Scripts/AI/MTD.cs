using System.Collections.Generic;
using UnityEngine;

public class MTD : IAConnect4
{
    private const int WIN_SCORE = 1000000;
    private int maxDepth = 6;

    private readonly ulong[,] zobristTable = new ulong[BoardCapacity.rows * BoardCapacity.cols, 2];
    private ulong currentHash;

    private readonly Dictionary<ulong, int> transpositionTable = new Dictionary<ulong, int>();

    private readonly int[] moveOrder = { 3, 2, 4, 1, 5, 0, 6 };
    private System.Random rand = new System.Random();

    public MTD()
    {
        for (int i = 0; i < BoardCapacity.rows * BoardCapacity.cols; i++)
        {
            zobristTable[i, 0] = RandomULong(rand); // jugador 1
            zobristTable[i, 1] = RandomULong(rand); // jugador -1
        }
    }

    private ulong RandomULong(System.Random rand)
    {
        ulong high = (ulong)rand.Next(0, int.MaxValue);
        ulong low = (ulong)rand.Next(0, int.MaxValue);
        return (high << 32) | low;
    }

    public Vector2Int GetBestMove(Board board)
    {
        int[,] grid = board.CopyBoard();
        currentHash = BoardEvaluator.ComputeZobristHash(grid, zobristTable);

        // Revisa victoria inmediata del jugador o bloqueo del rival
        for (int col = 0; col < BoardCapacity.cols; col++)
        {
            int row = BoardEvaluator.GetPlayableRow(grid, col);
            if (row == -1) continue;

            // Si la IA gana ahora, mueve
            grid[row, col] = 1;
            if (BoardEvaluator.IsWinningMove(grid, row, col, 1))
                return new Vector2Int(row, col);
            grid[row, col] = 0;

            // Si el rival ganaría, bloquea
            grid[row, col] = -1;
            if (BoardEvaluator.IsWinningMove(grid, row, col, -1))
                return new Vector2Int(row, col);
            grid[row, col] = 0;
        }

        int bestMove = -1;
        int bestScore = int.MinValue;

        List<int> cols = new List<int>();
        foreach (int col in moveOrder)
            if (board.CanPlay(col)) cols.Add(col);

        cols.Sort((a, b) =>
        {
            int scoreA = BoardEvaluator.EvaluateColumn(grid, a, 1);
            int scoreB = BoardEvaluator.EvaluateColumn(grid, b, 1);
            return scoreB.CompareTo(scoreA);
        });

        foreach (int col in cols)
        {
            int row = board.Play(col, grid, 1);
            currentHash ^= zobristTable[row * BoardCapacity.cols + col, 0];

            int guess = BoardEvaluator.Evaluate(grid);
            int score = -MTDf(grid, -guess, maxDepth - 1, -1, board);

            board.Undo(grid, col, row);
            currentHash ^= zobristTable[row * BoardCapacity.cols + col, 0];

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }
        }

        if (bestMove == -1) return new Vector2Int(-1, -1);
        int dropRow = BoardEvaluator.GetPlayableRow(grid, bestMove);
        return new Vector2Int(dropRow, bestMove);
    }

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
        if (transpositionTable.TryGetValue(currentHash, out int cached))
            return cached;

        int eval = BoardEvaluator.Evaluate(grid);
        if (Mathf.Abs(eval) >= WIN_SCORE || depth == 0)
        {
            transpositionTable[currentHash] = eval * player;
            return eval * player;
        }

        int best = int.MinValue;

        List<int> cols = new List<int>();
        foreach (int col in moveOrder)
            if (BoardEvaluator.GetPlayableRow(grid, col) != -1) cols.Add(col);

        cols.Sort((a, b) =>
        {
            int scoreA = BoardEvaluator.EvaluateColumn(grid, a, player);
            int scoreB = BoardEvaluator.EvaluateColumn(grid, b, player);
            return scoreB.CompareTo(scoreA);
        });

        foreach (int col in cols)
        {
            int row = board.Play(col, grid, player);
            currentHash ^= zobristTable[row * BoardCapacity.cols + col, player == 1 ? 0 : 1];

            if (BoardEvaluator.IsWinningMove(grid, row, col, player))
            {
                board.Undo(grid, col, row);
                currentHash ^= zobristTable[row * BoardCapacity.cols + col, player == 1 ? 0 : 1];
                transpositionTable[currentHash] = WIN_SCORE * player;
                return WIN_SCORE * player;
            }

            int val = -NegaMaxAB(grid, depth - 1, -player, board, -beta, -alpha);

            board.Undo(grid, col, row);
            currentHash ^= zobristTable[row * BoardCapacity.cols + col, player == 1 ? 0 : 1];

            if (val > best) best = val;
            if (best > alpha) alpha = best;
            if (alpha >= beta) break;
        }

        transpositionTable[currentHash] = best;
        return best;
    }
}

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

    private static int CountImmediateWins(int[,] g, int player)
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

    private static bool CreatesDoubleThreat(int[,] g, int row, int col, int player)
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

