using System.Collections.Generic;
using UnityEngine;

public class NegaMaxAB : IAConnect4
{
    private const int INF = 100000;
    private int maxDepth = 5;

    public Vector2Int GetBestMove(Board board, Color aiColor)
    {
        Color[,] boardColors = board.CopyBoardColors();
        List<Vector2Int> moves = board.GetValidMoves(boardColors);

        if (moves.Count == 0)
            return new Vector2Int(-1, -1);

        // Priorizar el centro antes de evaluar (mejor apertura)
        moves.Sort((a, b) => Mathf.Abs(3 - a.y).CompareTo(Mathf.Abs(3 - b.y)));

        int bestScore = -INF;
        Vector2Int bestMove = moves[0];
        int alpha = -INF, beta = INF;

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

            alpha = Mathf.Max(alpha, score);
            if (alpha >= beta) break;
        }

        return bestMove;
    }

    private int NegamaxAB(Board board, Color[,] boardColors, Color player, int depth, int alpha, int beta, Color aiColor)
    {
        List<Vector2Int> moves = board.GetValidMoves(boardColors);

        // Terminal states
        if (moves.Count == 0 || depth >= maxDepth)
            return EvaluateBoard(boardColors, aiColor);

        int best = -INF;

        foreach (var move in moves)
        {
            Color[,] copy = board.CopyBoard(boardColors);
            copy[move.x, move.y] = player;

            if (board.CheckConnection(copy, move.x, move.y, player))
            {
                if (player == aiColor)
                    return INF - depth; // AI wins fast
                else
                    return -INF + depth; // Opponent wins
            }

            int score = -NegamaxAB(board, copy, Opponent(player), depth + 1, -beta, -alpha, aiColor);
            best = Mathf.Max(best, score);
            alpha = Mathf.Max(alpha, score);

            if (alpha >= beta)
                break;
        }

        return best;
    }

    private int EvaluateBoard(Color[,] boardColors, Color aiColor)
    {
        int score = 0;
        Color opponent = Opponent(aiColor);

        // Prioriza el centro
        for (int r = 0; r < 6; r++)
        {
            if (boardColors[r, 3] == aiColor)
                score += 6;
            else if (boardColors[r, 3] == opponent)
                score -= 6;
        }

        for (int r = 0; r < 6; r++)
        {
            for (int c = 0; c < 7; c++)
            {
                // Horizontal
                if (c <= 3)
                    score += EvaluateWindow(GetWindow(boardColors, r, c, 0, 1), aiColor, opponent);
                // Vertical
                if (r <= 2)
                    score += EvaluateWindow(GetWindow(boardColors, r, c, 1, 0), aiColor, opponent);
                // Diagonal 
                if (r <= 2 && c <= 3)
                    score += EvaluateWindow(GetWindow(boardColors, r, c, 1, 1), aiColor, opponent);
                // Diagonal 
                if (r <= 2 && c >= 3)
                    score += EvaluateWindow(GetWindow(boardColors, r, c, 1, -1), aiColor, opponent);
            }
        }

        return score;
    }

    private List<Color> GetWindow(Color[,] boardColors, int startRow, int startCol, int deltaRow, int deltaCol)
    {
        List<Color> window = new List<Color>();
        for (int i = 0; i < 4; i++)
        {
            int r = startRow + deltaRow * i;
            int c = startCol + deltaCol * i;
            window.Add(boardColors[r, c]);
        }
        return window;
    }

    private int EvaluateWindow(List<Color> window, Color aiColor, Color opponent)
    {
        int aiCount = 0, oppCount = 0, empty = 0;
        foreach (var cell in window)
        {
            if (cell == aiColor) aiCount++;
            else if (cell == opponent) oppCount++;
            else if (cell == Colors.BLUE) empty++;
        }

        int score = 0;

        // IA heurística
        if (aiCount == 4) score += 100000;
        else if (aiCount == 3 && empty == 1) score += 600;
        else if (aiCount == 2 && empty == 2) score += 100;

        // Defensa fuerte
        if (oppCount == 3 && empty == 1) score -= 800;
        else if (oppCount == 2 && empty == 2) score -= 80;

        return score;
    }

    private Color Opponent(Color c)
    {
        return (c == Colors.RED) ? Colors.YELLOW : Colors.RED;
    }
}