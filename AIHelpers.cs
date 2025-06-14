using System;
using System.Collections.Generic;

namespace ChessGame
{
    // Helper methods for the AI class
    public static class AIHelpers
    {
        // Check if a position is drawn (simplified)
        public static bool IsDrawnPosition(IBoard board)
        {
            // Count material to check for insufficient material
            int pawnCount = 0;
            int minorPieceCount = 0;
            int majorPieceCount = 0;
            
            for (Column col = Column.a; col <= Column.h; col++)
            {
                for (int row = 0; row <= 7; row++)
                {
                    Square square = board.GetSquare(col, row);
                    if (square.Piece != null)
                    {
                        switch (square.Piece.Type)
                        {
                            case PieceType.Pawn:
                                pawnCount++;
                                break;
                            case PieceType.Knight:
                            case PieceType.Bishop:
                                minorPieceCount++;
                                break;
                            case PieceType.Rook:
                            case PieceType.Queen:
                                majorPieceCount++;
                                break;
                        }
                    }
                }
            }
            
            // K vs K
            if (pawnCount == 0 && minorPieceCount == 0 && majorPieceCount == 0)
                return true;
            
            // K+minor vs K
            if (pawnCount == 0 && minorPieceCount == 1 && majorPieceCount == 0)
                return true;
            
            // K+B+B vs K (same color bishops)
            // This is a simplification - would need to check bishop colors
            
            return false;
        }
        
        // Check if a side is in check
        public static bool IsInCheck(IBoard board, Color color)
        {
            try
            {
                // Find the king
                Square? kingSquare = null;
                
                for (Column col = Column.a; col <= Column.h; col++)
                {
                    for (int row = 0; row <= 7; row++)
                    {
                        Square square = board.GetSquare(col, row);
                        if (square != null && square.Piece != null && 
                            square.Piece.Type == PieceType.King && 
                            square.Piece.Color == color)
                        {
                            kingSquare = square;
                            break;
                        }
                    }
                    if (kingSquare != null) break;
                }
                
                if (kingSquare == null)
                    return false; // Should never happen in a valid position
                
                return board.IsSquareInCheck(kingSquare, color);
            }
            catch (Exception)
            {
                // If there's any exception, assume not in check
                return false;
            }
        }
        
        // Check if we're in an endgame position
        public static bool IsEndgamePosition(IBoard board, int[] pieceValues)
        {
            int whiteMaterial = 0;
            int blackMaterial = 0;
            int queenCount = 0;
            
            for (Column col = Column.a; col <= Column.h; col++)
            {
                for (int row = 0; row <= 7; row++)
                {
                    Square square = board.GetSquare(col, row);
                    if (square.Piece != null && square.Piece.Type != PieceType.King)
                    {
                        int value = pieceValues[(int)square.Piece.Type];
                        
                        if (square.Piece.Color == Color.White)
                            whiteMaterial += value;
                        else
                            blackMaterial += value;
                        
                        if (square.Piece.Type == PieceType.Queen)
                            queenCount++;
                    }
                }
            }
            
            // Endgame if:
            // 1. Total material is low
            // 2. One side has very little material
            // 3. No queens on the board
            return (whiteMaterial + blackMaterial < 3000) || 
                   (whiteMaterial < 1200 || blackMaterial < 1200) ||
                   (queenCount == 0);
        }
        
        // Check if a move is a capture
        public static bool IsCapture(MoveResultStruct move)
        {
            return move.CapturedPiece != null;
        }
        
        // Get a unique index for a move (for killer moves)
        public static int GetMoveIndex(MoveResultStruct move)
        {
            return ((int)move.From.Column * 8 + move.From.Row) * 64 + ((int)move.To.Column * 8 + move.To.Row);
        }
        
        // Get algebraic notation for a move
        public static string GetMoveNotation(MoveResultStruct move)
        {
            return $"{move.From.Column}{move.From.Row + 1}-{move.To.Column}{move.To.Row + 1}";
        }
        
        // Make a move on the board
        public static void MakeMove(MoveResultStruct move)
        {
/*
            if (move.From == null || move.To == null || move.Piece == null)
                return;
                
            move.From.RemovePiece();
            
            if (move.CapturedPiece != null && move.CapturedPiece.Square != null)
            {
                move.CapturedPiece.Square.RemovePiece();
            }
            
            move.To.SetPiece(move.Piece);
            
            // Note: Castling is handled by the Board class in the actual game
            // For AI evaluation, we don't need to move the rook as it doesn't affect the evaluation
*/
        }
        
        // Undo a move on the board
        public static void UndoMove(MoveResultStruct move)
        {
            /*
                        if (move.From == null || move.To == null || move.Piece == null)
                            return;

                        // Note: Castling is handled by the Board class in the actual game
                        // For AI evaluation, we don't need to move the rook back as it doesn't affect the evaluation

                        move.To.RemovePiece();
                        move.From.SetPiece(move.Piece);

                        if (move.CapturedPiece != null && move.CapturedPiece.Square != null)
                        {
                            move.CapturedPiece.Square.SetPiece(move.CapturedPiece);
                        }
            */
        }
    }
}
