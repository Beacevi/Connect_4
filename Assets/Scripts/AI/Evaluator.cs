using UnityEngine;

public class Evaluator
{
    private const int WIN_SCORE = 1000000;

    public static int Evaluate(int[,] g)
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

    private static void CountLine(int val, int[] c)
    {
        if (val == 4) c[8]++;
        else if (val == -4) c[0]++;
        else if (val == 3) c[7]++;
        else if (val == -3) c[1]++;
        else if (val == 2) c[6]++;
        else if (val == -2) c[2]++;
        else if (val == 1) c[5]++;
        else if (val == -1) c[3]++;
    }
    

}
