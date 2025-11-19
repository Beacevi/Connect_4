using UnityEngine;
using System;

public class MTD : IAConnect4Base
{
    private const int MAX_DEPTH = 6;
    private const int INF = 10000000;
    private readonly int[] columnOrder = { 3, 4, 2, 5, 1, 6, 0 };

    // Transposition table settings
    private TranspositionTableConnect4 TT;
    private ZobristKeyConnect4 Zob;
    private int ttMB = 32; // tamaño en MB por defecto (ajusta si quieres)

    public MTD(int ttMegabytes = 32)
    {
        ttMB = ttMegabytes;
        TT = new TranspositionTableConnect4(ttMB);
        Zob = new ZobristKeyConnect4();
    }

    protected override Vector2Int GetBestMoveInternal(Board board)
    {

        int[,] grid = board.CopyBoard();

        // Inicializar Zobrist key desde tablero actual
        Zob.Reset();
        for (int r = 0; r < BoardCapacity.rows; r++)
            for (int c = 0; c < BoardCapacity.cols; c++)
                if (grid[r, c] != 0)
                    Zob.ApplyMove(r, c, grid[r, c]);

        int player = BoardIsRedTurn(board) ? -1 : 1;

        // Comprueba jugadas ganadoras inmediatas
        for (int c = 0; c < BoardCapacity.cols; c++)
        {
            int r = GetPlayableRow(grid, c);
            if (r == -1) continue;
            grid[r, c] = player;
            if (EvaluateWindowForWin(grid, player, r, c))
            {
                grid[r, c] = 0;
                return new Vector2Int(board.GetRow(c), c);
            }
            grid[r, c] = 0;
        }

        // Comprueba bloqueo inmediato
        int opp = -player;
        for (int c = 0; c < BoardCapacity.cols; c++)
        {
            int r = GetPlayableRow(grid, c);
            if (r == -1) continue;
            grid[r, c] = opp;
            if (EvaluateWindowForWin(grid, opp, r, c))
            {
                grid[r, c] = 0;
                return new Vector2Int(board.GetRow(c), c);
            }
            grid[r, c] = 0;
        }

        // Iterative deepening
        int bestCol = -1;
        int bestScore = int.MinValue;

        NodesVisited = 0;

        for (int depth = 1; depth <= MAX_DEPTH; depth++)
        {
            int localBestScore = int.MinValue;
            int localBestCol = -1;


            foreach (int col in columnOrder)
            {
                int row = GetPlayableRow(grid, col);
                if (row == -1) continue;

                // make
                grid[row, col] = player;
                Zob.ApplyMove(row, col, player);

                int score = -AlphaBetaWithTT(grid, depth - 1, -INF, INF, -player);

                // undo
                Zob.UndoMove(row, col, player);
                grid[row, col] = 0;

                if (score > localBestScore || localBestCol == -1)
                {
                    localBestScore = score;
                    localBestCol = col;
                }
            }

            if (localBestCol != -1)
            {
                bestCol = localBestCol;
                bestScore = localBestScore;
            }

            // Si encontramos una victoria absoluta, podemos cortar antes
            if (Mathf.Abs(localBestScore) >= WIN_SCORE) break;
        }

        // Después de ID ya tenemos TT poblada y una buena mejor jugada. Podemos refinar con MTD(f) en la raíz
        if (bestCol == -1) return new Vector2Int(-1, -1);

        // Final refinement por MTD(f) en cada movimiento del root
        int finalBestCol = bestCol;
        int finalBestScore = bestScore;

        // Calcula guess inicial desde TT o bestScore
        int initialGuess = bestScore;

        foreach (int col in columnOrder)
        {
            int row = GetPlayableRow(grid, col);
            if (row == -1) continue;

            grid[row, col] = player;
            Zob.ApplyMove(row, col, player);

            int val = -MTDfRoot(grid, Zob.Key, MAX_DEPTH - 1, initialGuess, -player);

            Zob.UndoMove(row, col, player);
            grid[row, col] = 0;

            if (val > finalBestScore || finalBestCol == -1)
            {
                finalBestScore = val;
                finalBestCol = col;
            }

            initialGuess = finalBestScore;
        }

        return finalBestCol == -1 ? new Vector2Int(-1, -1) : new Vector2Int(board.GetRow(finalBestCol), finalBestCol);
    }

    // Root-level MTD(f) wrapper: usa AlphaBetaWithTT para búsquedas de ventana nula
    private int MTDfRoot(int[,] grid, ulong key, int depth, int firstGuess, int player)
    {
        int g = firstGuess;
        int lower = -INF;
        int upper = INF;

        while (lower < upper)
        {
            int beta = (g == lower) ? g + 1 : g;
            int val = AlphaBetaWithTT(grid, depth, beta - 1, beta, player);
            g = val;
            if (g < beta) upper = g;
            else lower = g;
        }

        return g;
    }

    // AlphaBeta con TT que utiliza TTEntry
    private int AlphaBetaWithTT(int[,] grid, int depth, int alpha, int beta, int player)
    {
        NodesVisited++;

        if (depth == 0 || IsTerminal(grid))
            return Evaluate(grid, player);

        ulong key = Zob.Key;

        // Probar tabla de transposición
        if (TT.TryGet(key, out TTEntry ttEntry) && ttEntry.depth >= depth)
        {
            // Si el intervalo guardado ya lo decide, devolvemos
            if (ttEntry.lowerBound >= beta) return ttEntry.lowerBound;
            if (ttEntry.upperBound <= alpha) return ttEntry.upperBound;

            // Ajusta alpha/beta con bounds almacenados
            alpha = Mathf.Max(alpha, ttEntry.lowerBound);
            beta = Mathf.Min(beta, ttEntry.upperBound);
        }

        int origAlpha = alpha;
        int origBeta = beta;

        int bestVal = int.MinValue;
        int bestMove = -1;

        // Move ordering: intenta primero el bestMove guardado en TT si existe
        int ttMove = (ttEntry.bestMove >= 0 && ttEntry.bestMove < BoardCapacity.cols) ? ttEntry.bestMove : -1;
        if (ttMove != -1)
        {
            int r = GetPlayableRow(grid, ttMove);
            if (r != -1)
            {
                grid[r, ttMove] = player;
                Zob.ApplyMove(r, ttMove, player);

                int val = -AlphaBetaWithTT(grid, depth - 1, -beta, -alpha, -player);

                Zob.UndoMove(r, ttMove, player);
                grid[r, ttMove] = 0;

                if (val > bestVal)
                {
                    bestVal = val;
                    bestMove = ttMove;
                }

                alpha = Mathf.Max(alpha, bestVal);
                if (alpha >= beta)
                {
                    // fail-high, guardar y retornar
                    TTEntry storeEntryHi = new TTEntry
                    {
                        depth = depth,
                        lowerBound = val,
                        upperBound = INF,
                        score = val,
                        bestMove = bestMove
                    };
                    TT.Store(key, storeEntryHi);
                    return val;
                }
            }
        }

        // Recorre movimientos en orden (columnOrder), saltando ttMove ya probado
        foreach (int col in columnOrder)
        {
            if (col == ttMove) continue;
            int r = GetPlayableRow(grid, col);
            if (r == -1) continue;

            grid[r, col] = player;
            Zob.ApplyMove(r, col, player);

            int val = -AlphaBetaWithTT(grid, depth - 1, -beta, -alpha, -player);

            Zob.UndoMove(r, col, player);
            grid[r, col] = 0;

            if (val > bestVal)
            {
                bestVal = val;
                bestMove = col;
            }

            alpha = Mathf.Max(alpha, bestVal);
            if (alpha >= beta) break;
        }

        // Decide bounds para almacenar en TT usando ventana original
        TTEntry toStore = new TTEntry
        {
            depth = depth,
            bestMove = bestMove,
            score = bestVal,
            lowerBound = int.MinValue,
            upperBound = int.MaxValue
        };

        if (bestVal <= origAlpha)
        {
            // fail-low: valor <= alpha original -> upperBound = bestVal
            toStore.upperBound = bestVal;
        }
        else if (bestVal >= origBeta)
        {
            // fail-high: valor >= beta original -> lowerBound = bestVal
            toStore.lowerBound = bestVal;
        }
        else
        {
            // exact
            toStore.lowerBound = bestVal;
            toStore.upperBound = bestVal;
        }

        TT.Store(key, toStore);
        return bestVal;
    }
}


