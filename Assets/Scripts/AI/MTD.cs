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
        // Inicializa la tabla Zobrist con números aleatorios
        System.Random rand = new System.Random();

        for (int i = 0; i < BoardCapacity.rows * BoardCapacity.cols; i++)
        {
            zobristTable[i, 0] = RandomULong(rand); // jugador 1
            zobristTable[i, 1] = RandomULong(rand); // jugador -1
        }
    }

    // Genera un ulong aleatorio combinando dos enteros de 32 bits
    private ulong RandomULong(System.Random rand)
    {
        ulong high = (ulong)rand.Next(0, int.MaxValue);
        ulong low = (ulong)rand.Next(0, int.MaxValue);
        return (high << 32) | low;
    }


    public Vector2Int GetBestMove(Board board)
    {
        int[,] grid = board.CopyBoard();
        currentHash = ComputeZobristHash(grid);

        int bestMove = -1;
        int bestScore = int.MinValue;

        // Orden dinámico: evaluamos rápido cada columna y ordenamos
        List<int> cols = new List<int>();
        foreach (int col in moveOrder)
            if (board.CanPlay(col)) cols.Add(col);

        cols.Sort((a, b) =>
        {
            int scoreA = EvaluateColumn(grid, a, 1);
            int scoreB = EvaluateColumn(grid, b, 1);
            return scoreB.CompareTo(scoreA); // Descendente
        });

        foreach (int col in cols)
        {
            int row = board.Play(col, grid, 1);
            currentHash ^= zobristTable[row * BoardCapacity.cols + col, 0]; // IA = 1

            // Victoria inmediata?
            if (CheckWin(grid, row, col, 1))
            {
                board.Undo(grid, col, row);
                currentHash ^= zobristTable[row * BoardCapacity.cols + col, 0];
                return new Vector2Int(row, col);
            }

            int guess = Evaluate(grid);
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
        int dropRow = board.GetRow(bestMove);
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

        int eval = Evaluate(grid);
        if (Mathf.Abs(eval) >= WIN_SCORE || depth == 0)
        {
            transpositionTable[currentHash] = eval * player;
            return eval * player;
        }

        int best = int.MinValue;

        // Orden dinámico
        List<int> cols = new List<int>();
        foreach (int col in moveOrder)
            if (board.CanPlay(col)) cols.Add(col);

        cols.Sort((a, b) =>
        {
            int scoreA = EvaluateColumn(grid, a, player);
            int scoreB = EvaluateColumn(grid, b, player);
            return scoreB.CompareTo(scoreA);
        });

        foreach (int col in cols)
        {
            int row = board.Play(col, grid, player);
            currentHash ^= zobristTable[row * BoardCapacity.cols + col, player == 1 ? 0 : 1];

            // Victoria inmediata
            if (CheckWin(grid, row, col, player))
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

    private int Evaluate(int[,] grid)
    {
        int score = 0;
        int rows = BoardCapacity.rows;
        int cols = BoardCapacity.cols;

        // Amenazas y líneas
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols - 3; c++)
                score += EvaluateLine(grid[r, c], grid[r, c + 1], grid[r, c + 2], grid[r, c + 3]);
        }

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows - 3; r++)
                score += EvaluateLine(grid[r, c], grid[r + 1, c], grid[r + 2, c], grid[r + 3, c]);
        }

        for (int r = 0; r < rows - 3; r++)
        {
            for (int c = 0; c < cols - 3; c++)
                score += EvaluateLine(grid[r, c], grid[r + 1, c + 1], grid[r + 2, c + 2], grid[r + 3, c + 3]);
        }

        for (int r = 3; r < rows; r++)
        {
            for (int c = 0; c < cols - 3; c++)
                score += EvaluateLine(grid[r, c], grid[r - 1, c + 1], grid[r - 2, c + 2], grid[r - 3, c + 3]);
        }

        // Centro
        int centerCol = cols / 2;
        for (int r = 0; r < rows; r++)
        {
            if (grid[r, centerCol] == 1) score += 3;
            else if (grid[r, centerCol] == -1) score -= 3;
        }

        return score;
    }

    private int EvaluateLine(int a, int b, int c, int d)
    {
        int[] line = { a, b, c, d };
        int sum = 0;
        int player1 = 0, player2 = 0;

        foreach (int v in line)
        {
            if (v == 1) player1++;
            else if (v == -1) player2++;
        }

        if (player1 > 0 && player2 > 0) return 0; // bloqueada

        if (player1 == 4) return WIN_SCORE;
        if (player2 == 4) return -WIN_SCORE;

        if (player1 == 3 && player2 == 0) return 1000;
        if (player2 == 3 && player1 == 0) return -1000;

        if (player1 == 2 && player2 == 0) return 10;
        if (player2 == 2 && player1 == 0) return -10;

        if (player1 == 1 && player2 == 0) return 1;
        if (player2 == 1 && player1 == 0) return -1;

        return 0;
    }

    private int EvaluateColumn(int[,] grid, int col, int player)
    {
        for (int r = BoardCapacity.rows - 1; r >= 0; r--)
        {
            if (grid[r, col] == 0)
            {
                grid[r, col] = player;
                int score = Evaluate(grid);
                grid[r, col] = 0;
                return score;
            }
        }
        return 0;
    }

    private bool CheckWin(int[,] grid, int row, int col, int player)
    {
        int[][] directions = new int[][] {
            new int[]{1,0}, new int[]{0,1}, new int[]{1,1}, new int[]{1,-1}
        };

        foreach (var dir in directions)
        {
            int count = 1;
            for (int d = 1; d <= 3; d++)
            {
                int r = row + dir[0] * d, c = col + dir[1] * d;
                if (r >= 0 && r < BoardCapacity.rows && c >= 0 && c < BoardCapacity.cols && grid[r, c] == player)
                    count++;
                else break;
            }

            for (int d = 1; d <= 3; d++)
            {
                int r = row - dir[0] * d, c = col - dir[1] * d;
                if (r >= 0 && r < BoardCapacity.rows && c >= 0 && c < BoardCapacity.cols && grid[r, c] == player)
                    count++;
                else break;
            }

            if (count >= 4) return true;
        }

        return false;
    }

    private ulong ComputeZobristHash(int[,] grid)
    {
        ulong h = 0;
        for (int r = 0; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols; c++)
                if (grid[r, c] != 0)
                    h ^= zobristTable[r * BoardCapacity.cols + c, grid[r, c] == 1 ? 0 : 1];
        return h;
    }
}
