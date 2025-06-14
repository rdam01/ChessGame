using System;

namespace ChessGame
{
    // Zobrist hashing for chess positions
    public static class AIZobrist
    {
        // Zobrist keys for position hashing
        private static readonly ulong[,,] ZobristKeys = InitializeZobristKeys();
        private static readonly ulong ZobristSideToMove = (ulong)new Random(1).NextInt64();

        // Initialize Zobrist keys
        private static ulong[,,] InitializeZobristKeys()
        {
            Random random = new Random(42); // Fixed seed for reproducibility
            ulong[,,] keys = new ulong[64, 6, 2]; // 64 squares, 6 piece types, 2 colors
            
            for (int square = 0; square < 64; square++)
            {
                for (int piece = 0; piece < 6; piece++)
                {
                    for (int color = 0; color < 2; color++)
                    {
                        keys[square, piece, color] = (ulong)random.NextInt64();
                    }
                }
            }
            
            return keys;
        }

        // Get a simple hash for the current position
        public static ulong GetPositionHash(IBoard board, Color sideToMove)
        {
            ulong hash = 0;
            
            for (Column col = Column.a; col <= Column.h; col++)
            {
                for (int row = 0; row <= 7; row++)
                {
                    Square square = board.GetSquare(col, row);
                    if (square.Piece != null)
                    {
                        int piece = (int)square.Piece.Type;
                        int color = (int)square.Piece.Color;
                        int position = (int)col * 8 + row;
                        
                        hash ^= ZobristKeys[position, piece, color];
                    }
                }
            }
            
            // Include side to move in the hash
            if (sideToMove == Color.White)
            {
                hash ^= ZobristSideToMove;
            }
            
            return hash;
        }
    }
}
