using System;

public struct TTEntry
{
    public int depth;
    public int lowerBound;
    public int upperBound;
    public int score;
    public int bestMove;
}

public class TranspositionTableConnect4
{
    private TTEntry[] table;
    private ulong[] keys;
    private int size;

    public TranspositionTableConnect4(int mbSize = 32)
    {
        size = (mbSize * 1024 * 1024) / 40; 
        table = new TTEntry[size];
        keys = new ulong[size];
    }

    private int Index(ulong key)
    {
        return (int)(key % (ulong)size);
    }

    public bool TryGet(ulong key, out TTEntry entry)
    {
        int idx = Index(key);
        if (keys[idx] == key)
        {
            entry = table[idx];
            return true;
        }
        entry = default;
        return false;
    }

    public void Store(ulong key, TTEntry entry)
    {
        int idx = Index(key);

        // Replace only if deeper
        if (keys[idx] != key || entry.depth >= table[idx].depth)
        {
            keys[idx] = key;
            table[idx] = entry;
        }
    }
}