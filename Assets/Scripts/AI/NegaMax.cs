using System.Collections.Generic;
using UnityEngine;


public class NegaMax 
{
    private const int INF = 100000;
    private int maxDepth = 5;

    public Vector2Int GetBestMove(Board board, Color aiColor)
    {
        Color[,] boardColors = board.CopyBoardColors(); // logical copy
        List<Vector2Int> moves = GetValidMoves(boardColors);

        if (moves.Count == 0) return new Vector2Int(-1, -1);

        int bestScore = -INF;
        Vector2Int bestMove = moves[0];

        foreach (var move in moves)
        {
            Color[,] copy = CopyBoard(boardColors);
            copy[move.x, move.y] = aiColor;

            int score = -Negamax(copy, Opponent(aiColor), 1, aiColor);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Negamax(Color[,] boardColors, Color player, int depth, Color aiColor)
    {
        List<Vector2Int> moves = GetValidMoves(boardColors);
        if (moves.Count == 0 || depth >= maxDepth)
            return 0; // no heuristic, non-winning positions score 0

        int best = -INF;

        foreach (var move in moves)
        {
            Color[,] copy = CopyBoard(boardColors);
            copy[move.x, move.y] = player;

            if (CheckConnection(copy, move.x, move.y, player))
                return INF - depth; // immediate win

            int score = -Negamax(copy, Opponent(player), depth + 1, aiColor);
            if (score > best) best = score;
        }

        return best;
    }

    // --- Helper functions ---

    private Color Opponent(Color color)
    {
        return color == Colors.RED ? Colors.YELLOW : Colors.RED;
    }

    private List<Vector2Int> GetValidMoves(Color[,] boardColors)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        for (int c = 0; c < 7; c++)
        {
            for (int r = 5; r >= 0; r--)
            {
                if (boardColors[r, c] == Colors.BLUE)
                {
                    moves.Add(new Vector2Int(r, c));
                    break;
                }
            }
        }
        return moves;
    }

    private bool CheckConnection(Color[,] boardColors, int row, int col, Color color)
    {
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1)
        };

        foreach (var dir in directions)
        {
            int count = 1;
            count += CountDirection(boardColors, row, col, dir.x, dir.y, color);
            count += CountDirection(boardColors, row, col, -dir.x, -dir.y, color);
            if (count >= 4) return true;
        }

        return false;
    }

    private int CountDirection(Color[,] boardColors, int row, int col, int rowStep, int colStep, Color color)
    {
        int count = 0;
        while (true)
        {
            row += rowStep;
            col += colStep;
            if (row < 0 || row >= 6 || col < 0 || col >= 7) break;
            if (boardColors[row, col] != color) break;
            count++;
        }
        return count;
    }

    private Color[,] CopyBoard(Color[,] boardColors)
    {
        Color[,] copy = new Color[6, 7];
        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 7; c++)
                copy[r, c] = boardColors[r, c];
        return copy;
    }
}

