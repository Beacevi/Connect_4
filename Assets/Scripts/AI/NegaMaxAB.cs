using System.Collections.Generic;
using UnityEngine;

public class NegaMaxAB : IAConnect4
{
    private const int INF = 100000;
    private int       maxDepth = 5;

    public Vector2Int GetBestMove(Board board, Color aiColor)
    {
        Color[,] boardColors = board.CopyBoardColors();
        List<Vector2Int> moves = board.GetValidMoves(boardColors);

        if (moves.Count == 0) return new Vector2Int(-1, -1);

        int bestScore = -INF;
        Vector2Int bestMove = moves[0];

        int alpha = -INF; //poda alpha
        int beta  =  INF; //poda beta

        foreach (var move in moves)
        {
            Color[,] copy = board.CopyBoard(boardColors);
            copy[move.x, move.y] = aiColor;

            int score = -NegamaxAB(board, copy, Opponent(aiColor), 1, -beta, -alpha, aiColor);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            alpha = Mathf.Max(alpha, score); //saca el max

            if (alpha >= beta) break; // poda
        }

        return bestMove;
    }

    private int NegamaxAB(Board board, Color[,] boardColors, Color player, int depth, int alpha, int beta, Color aiColor)
    {
        List<Vector2Int> moves = board.GetValidMoves(boardColors);
        if (moves.Count == 0 || depth >= maxDepth)
            return 0;

        int best = -INF;

        foreach (var move in moves)
        {
            Color[,] copy = board.CopyBoard(boardColors);
            copy[move.x, move.y] = player;

            if (board.CheckConnection(copy, move.x, move.y, player))
                return INF - depth; // fast wins are better

            int score = -NegamaxAB(board, copy, Opponent(player), depth + 1, -beta, -alpha, aiColor);

            best  = Mathf.Max(best, score);
            alpha = Mathf.Max(alpha, score);

            if (alpha >= beta) break; // alpha-beta cutoff
        }

        return best;
    }

    private Color Opponent(Color color)
    {
        return color == Colors.RED ? Colors.YELLOW : Colors.RED;
    }
}
