// See https://aka.ms/new-console-template for more information

public class BitBoard
{
    static readonly int[] index64 = {
    0, 47,  1, 56, 48, 27,  2, 60,
   57, 49, 41, 37, 28, 16,  3, 61,
   54, 58, 35, 52, 50, 42, 21, 44,
   38, 32, 29, 23, 17, 11,  4, 62,
   46, 55, 26, 59, 40, 36, 15, 53,
   34, 51, 20, 43, 31, 22, 10, 45,
   25, 39, 14, 33, 19, 30,  9, 24,
   13, 18,  8, 12,  7,  6,  5, 63
};

    /**
     * bitScanForward
     * @author Kim Walisch (2012)
     * @param bb bitboard to scan
     * @precondition bb != 0
     * @return index (0..63) of least significant one bit
     */
    public static int bitScanForward(UInt64 bb)
    {
        const UInt64 debruijn64 = (UInt64)(0x03f79d71b4cb0a89);
        if (bb != 0)
        {
            return index64[((bb ^ (bb - 1)) * debruijn64) >> 58];
        }
        else
        {
            throw new Exception("BitBoard may not be 0!");
        }
    }
    
    public static UInt64 SetBit(UInt64 bb, int row, Column col)
    {
        bb |= (ulong)1 << (char)(row * 8) + (char)col;
        return bb;
    }
    
    public static bool IsBitSet(UInt64 bb, int row, Column col)
    {
        return ((bb & (ulong)1 << (char)(row * 8) + (char)col) != 0);
    }
    
    // Count the number of set bits in a bitboard (population count)
    public static int PopCount(UInt64 bb)
    {
        int count = 0;
        while (bb != 0)
        {
            count++;
            bb &= bb - 1; // Clear the least significant bit set
        }
        return count;
    }
}
