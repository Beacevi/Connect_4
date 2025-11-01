using System.Collections.Generic;
using UnityEngine;


public class NegaMax : IAConnect4
{
    private const int INF = 100000;
    private int maxDepth = 5;

    public Vector2Int GetBestMove(Board board, Color aiColor)
    {
        Color[,] boardColors = board.CopyBoardColors(); // logical copy
        List<Vector2Int> moves = board.GetValidMoves(boardColors);

        if (moves.Count == 0) return new Vector2Int(-1, -1);

        int bestScore = -INF;
        Vector2Int bestMove = moves[0];

        foreach (var move in moves)
        {
            Color[,] copy = board.CopyBoard(boardColors);
            copy[move.x, move.y] = aiColor;

            int score = -Negamax(board, copy, Opponent(aiColor), 1, aiColor);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Negamax(Board board, Color[,] boardColors, Color player, int depth, Color aiColor)
    {
        List<Vector2Int> moves = board.GetValidMoves(boardColors);
        if (moves.Count == 0 || depth >= maxDepth)
            return 0; // no heuristic, non-winning positions score 0

        int best = -INF;

        foreach (var move in moves)
        {
            Color[,] copy = board.CopyBoard(boardColors);
            copy[move.x, move.y] = player;

            if (board.CheckConnection(copy, move.x, move.y, player))
                return INF - depth; // immediate win

            int score = -Negamax(board, copy, Opponent(player), depth + 1, aiColor);
            if (score > best) best = score;
        }

        return best;
    }

    private Color Opponent(Color color)
    {
        return color == Colors.RED ? Colors.YELLOW : Colors.RED;
    }
}

