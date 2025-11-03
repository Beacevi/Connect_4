using System.Collections.Generic;
using UnityEngine;

public class NegaMaxAB : IAConnect4
{
    private const int INFINITY = 100000;
    private int _maxDepth = 5;
    private Vector2Int _bestMove;

    public Vector2Int GetBestMove(Board board, Color playerColor)
    {
        _bestMove = new Vector2Int(-1, -1);

        if (board.IsBoardFull()) return _bestMove;

        // Revisión de victorias inmediatas
        Vector2Int immediateWin = CheckImmediateWin(board, playerColor);
        if (immediateWin.y != -1) return immediateWin;

        // Bloquear amenazas inmediatas del oponente
        Color opponentColor = (playerColor == Colors.RED) ? Colors.YELLOW : Colors.RED;
        Vector2Int blockOpponent = CheckImmediateWin(board, opponentColor);
        if (blockOpponent.y != -1) return blockOpponent;

        int guess = Evaluate(board, playerColor);
        int lowerBound = -INFINITY;
        int upperBound = INFINITY;

        while (lowerBound < upperBound)
        {
            int beta = (guess == lowerBound) ? guess + 1 : guess;
            int score = NegamaxSearch(board, playerColor, 0, beta - 1, beta);

            if (score < beta)
                upperBound = score;
            else
                lowerBound = score;

            guess = score;
        }

        return _bestMove;
    }

    private Vector2Int CheckImmediateWin(Board board, Color color)
    {
        Color[,] boardCopy = board.CopyBoardColors();
        List<Vector2Int> validMoves = board.GetValidMoves(boardCopy);

        foreach (var move in validMoves)
        {
            boardCopy[move.x, move.y] = color;
            if (board.CheckConnection(boardCopy, move.x, move.y, color))
            {
                return move;
            }
            boardCopy[move.x, move.y] = Colors.BLUE;
        }

        return new Vector2Int(-1, -1);
    }

    private int NegamaxSearch(Board board, Color playerColor, int depth, int alpha, int beta)
    {
        if (depth >= _maxDepth || board.IsBoardFull())
            return Evaluate(board, playerColor);

        int bestScore = -INFINITY;
        Vector2Int bestMoveLocal = new Vector2Int(-1, -1);
        Color opponentColor = (playerColor == Colors.RED) ? Colors.YELLOW : Colors.RED;

        Color[,] boardCopy = board.CopyBoardColors();
        List<Vector2Int> validMoves = board.GetValidMoves(boardCopy);

        // Ordenar movimientos por cercanía al centro para mejorar poda alfa-beta
        validMoves.Sort((a, b) => Mathf.Abs(3 - a.y).CompareTo(Mathf.Abs(3 - b.y)));

        foreach (Vector2Int move in validMoves)
        {
            boardCopy[move.x, move.y] = playerColor;

            int score;
            if (board.CheckConnection(boardCopy, move.x, move.y, playerColor))
            {
                score = INFINITY - depth;
            }
            else
            {
                score = -NegamaxSearch(board, opponentColor, depth + 1, -beta, -alpha);
            }

            boardCopy[move.x, move.y] = Colors.BLUE;

            if (score > bestScore)
            {
                bestScore = score;
                bestMoveLocal = move;
            }

            alpha = Mathf.Max(alpha, score);
            if (alpha >= beta) break;
        }

        if (depth == 0) _bestMove = bestMoveLocal;

        return bestScore;
    }

    private int Evaluate(Board board, Color playerColor)
    {
        Color opponentColor = (playerColor == Colors.RED) ? Colors.YELLOW : Colors.RED;
        int score = 0;
        Color[,] boardColors = board.CopyBoardColors();

        // Evaluación de ventanas de 4
        for (int r = 0; r < 6; r++)
        {
            for (int c = 0; c < 7; c++)
            {
                if (boardColors[r, c] == Colors.BLUE) continue;

                score += EvaluateWindow(boardColors, r, c, 0, 1, playerColor, opponentColor);   // Horizontal
                score += EvaluateWindow(boardColors, r, c, 1, 0, playerColor, opponentColor);   // Vertical
                score += EvaluateWindow(boardColors, r, c, 1, 1, playerColor, opponentColor);   // Diagonal \
                score += EvaluateWindow(boardColors, r, c, 1, -1, playerColor, opponentColor);  // Diagonal /
            }
        }

        // Bonus por controlar columnas centrales
        for (int r = 0; r < 6; r++)
        {
            if (boardColors[r, 3] == playerColor) score += 3;
            else if (boardColors[r, 3] == opponentColor) score -= 3;
        }

        return score;
    }

    private int EvaluateWindow(Color[,] board, int row, int col, int rowDir, int colDir, Color player, Color opponent)
    {
        int playerCount = 0;
        int opponentCount = 0;
        int emptyCount = 0;

        for (int i = 0; i < 4; i++)
        {
            int r = row + i * rowDir;
            int c = col + i * colDir;

            if (r < 0 || r >= 6 || c < 0 || c >= 7) break;

            if (board[r, c] == player) playerCount++;
            else if (board[r, c] == opponent) opponentCount++;
            else emptyCount++;
        }

        // Asignar puntuación estratégica
        if (playerCount == 4) return 100000;
        if (playerCount == 3 && emptyCount == 1) return 100;
        if (playerCount == 2 && emptyCount == 2) return 10;

        if (opponentCount == 4) return -100000;
        if (opponentCount == 3 && emptyCount == 1) return -90;
        if (opponentCount == 2 && emptyCount == 2) return -10;

        return 0;
    }
}