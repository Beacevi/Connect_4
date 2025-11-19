using UnityEngine;
using System;

public class ZobristKeyConnect4
{
    private ulong[,,] table; // [row][col][player]
    public ulong Key { get; private set; }

    public ZobristKeyConnect4()
    {
        System.Random rng = new System.Random();
        table = new ulong[6, 7, 2];

        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 7; c++)
                for (int p = 0; p < 2; p++)
                    table[r, c, p] = RandomUInt64(rng);

        Key = 0;
    }

    private ulong RandomUInt64(System.Random rng)
    {
        byte[] buffer = new byte[8];
        rng.NextBytes(buffer);
        return BitConverter.ToUInt64(buffer, 0);
    }

    public void Reset()
    {
        Key = 0;
    }

    public void ApplyMove(int row, int col, int player)
    {
        int idx = (player == -1) ? 0 : 1;
        Key ^= table[row, col, idx];
    }

    public void UndoMove(int row, int col, int player)
    {
        ApplyMove(row, col, player);
    }
}


