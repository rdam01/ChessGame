using System;

namespace ChessGame
{
    // Types of transposition table entries
    public enum TranspositionEntryType
    {
        Exact,      // Exact score
        LowerBound, // Alpha cutoff (score is a lower bound)
        UpperBound  // Beta cutoff (score is an upper bound)
    }

    // Entry for transposition table
    public class TranspositionEntry
    {
        public int Score { get; set; }
        public int Depth { get; set; }
        public TranspositionEntryType Type { get; set; }
    }
}
