using System;

public static class SimpleId
{
    private static long lastTimestamp = -1;
    private static long sequence = 0;
    private const int sequenceBits = 12;                // 4096 ID per ms
    private const long maxSequence = (1L << sequenceBits) - 1;

    // Custom epoch to keep IDs shorter
    private const long epoch = 1704067200000L;          // 2024-01-01 in ms

    public static long Next()
    {
        long ts = Timestamp();

        if (ts < lastTimestamp)
        {
            // Clock went backward. Unity, блин.
            ts = lastTimestamp;
        }

        if (ts == lastTimestamp)
        {
            sequence = (sequence + 1) & maxSequence;
            if (sequence == 0)
            {
                // Sequence overflow in SAME millisecond
                ts = WaitNextMs(ts);
            }
        }
        else
        {
            sequence = 0;
        }

        lastTimestamp = ts;

        return ((ts - epoch) << sequenceBits) | sequence;
    }

    private static long Timestamp()
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private static long WaitNextMs(long current)
    {
        long ts;
        do { ts = Timestamp(); }
        while (ts == current);
        return ts;
    }
}