using System;

namespace ChessGame
{
    // Position evaluation for the AI
    public static class AIEvaluation
    {
        // Piece values for material evaluation
        public static readonly int[] PieceValues = {
            100,    // Pawn
            500,    // Rook
            320,    // Knight
            330,    // Bishop
            900,    // Queen
            20000   // King - high value to ensure king safety
        };
        
        // Mobility bonuses - reward pieces that have more moves available
        public static readonly int[] MobilityBonus = {
            0,      // Pawn (handled separately)
            3,      // Rook
            4,      // Knight
            3,      // Bishop
            2,      // Queen
            0       // King (handled separately)
        };
        
        // Pawn structure evaluation weights
        public static readonly int DOUBLED_PAWN_PENALTY = -10;
        public static readonly int ISOLATED_PAWN_PENALTY = -20;
        public static readonly int PASSED_PAWN_BONUS = 20;
        public static readonly int PROTECTED_PAWN_BONUS = 10;
        
        // King safety evaluation weights
        public static readonly int KING_SHIELD_BONUS = 10;
        public static readonly int KING_OPEN_FILE_PENALTY = -25;
        public static readonly int KING_SEMI_OPEN_FILE_PENALTY = -15;
        
        // Center control bonuses
        public static readonly int CENTER_CONTROL_BONUS = 10;
        public static readonly int EXTENDED_CENTER_CONTROL_BONUS = 5;

        // Piece-square tables for positional evaluation
        // Pawns - encourage center control and advancement
        public static readonly int[,] PawnTable = {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            {  5,  5, 10, 25, 25, 10,  5,  5 },
            {  0,  0,  0, 20, 20,  0,  0,  0 },
            {  5, -5,-10,  0,  0,-10, -5,  5 },
            {  5, 10, 10,-20,-20, 10, 10,  5 },
            {  0,  0,  0,  0,  0,  0,  0,  0 }
        };

        // Knights - encourage knights to stay near the center
        public static readonly int[,] KnightTable = {
            {-50,-40,-30,-30,-30,-30,-40,-50 },
            {-40,-20,  0,  0,  0,  0,-20,-40 },
            {-30,  0, 10, 15, 15, 10,  0,-30 },
            {-30,  5, 15, 20, 20, 15,  5,-30 },
            {-30,  0, 15, 20, 20, 15,  0,-30 },
            {-30,  5, 10, 15, 15, 10,  5,-30 },
            {-40,-20,  0,  5,  5,  0,-20,-40 },
            {-50,-40,-30,-30,-30,-30,-40,-50 }
        };

        // Bishops - encourage bishops to control long diagonals
        public static readonly int[,] BishopTable = {
            {-20,-10,-10,-10,-10,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0, 10, 10, 10, 10,  0,-10 },
            {-10,  5,  5, 10, 10,  5,  5,-10 },
            {-10,  0,  5, 10, 10,  5,  0,-10 },
            {-10,  5,  5,  5,  5,  5,  5,-10 },
            {-10,  0,  5,  0,  0,  5,  0,-10 },
            {-20,-10,-10,-10,-10,-10,-10,-20 }
        };

        // Rooks - encourage rooks to control open files
        public static readonly int[,] RookTable = {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            {  5, 10, 10, 10, 10, 10, 10,  5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            {  0,  0,  0,  5,  5,  0,  0,  0 }
        };

        // Queens - combination of rook and bishop mobility
        public static readonly int[,] QueenTable = {
            {-20,-10,-10, -5, -5,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0,  5,  5,  5,  5,  0,-10 },
            { -5,  0,  5,  5,  5,  5,  0, -5 },
            {  0,  0,  5,  5,  5,  5,  0, -5 },
            {-10,  5,  5,  5,  5,  5,  0,-10 },
            {-10,  0,  5,  0,  0,  0,  0,-10 },
            {-20,-10,-10, -5, -5,-10,-10,-20 }
        };

        // King - encourage king safety in the early/middle game
        public static readonly int[,] KingMiddleTable = {
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-20,-30,-30,-40,-40,-30,-30,-20 },
            {-10,-20,-20,-20,-20,-20,-20,-10 },
            { 20, 20,  0,  0,  0,  0, 20, 20 },
            { 20, 30, 10,  0,  0, 10, 30, 20 }
        };

        // King - encourage king activity in the endgame
        public static readonly int[,] KingEndTable = {
            {-50,-40,-30,-20,-20,-30,-40,-50 },
            {-30,-20,-10,  0,  0,-10,-20,-30 },
            {-30,-10, 20, 30, 30, 20,-10,-30 },
            {-30,-10, 30, 40, 40, 30,-10,-30 },
            {-30,-10, 30, 40, 40, 30,-10,-30 },
            {-30,-10, 20, 30, 30, 20,-10,-30 },
            {-30,-30,  0,  0,  0,  0,-30,-30 },
            {-50,-30,-30,-30,-30,-30,-30,-50 }
        };

        // Get position value for a piece
        public static int GetPositionValue(Piece piece, Column col, int row)
        {
            int tableRow = piece.Color == Color.White ? 7 - row : row;
            int tableCol = (int)col;
            
            switch (piece.Type)
            {
                case PieceType.Pawn:
                    return PawnTable[tableRow, tableCol];
                case PieceType.Knight:
                    return KnightTable[tableRow, tableCol];
                case PieceType.Bishop:
                    return BishopTable[tableRow, tableCol];
                case PieceType.Rook:
                    return RookTable[tableRow, tableCol];
                case PieceType.Queen:
                    return QueenTable[tableRow, tableCol];
                default:
                    return 0;
            }
        }

        // Evaluate the current position
        public static int EvaluatePosition(IBoard board, Color currentPlayerColor)
        {
            int score = 0;
            int whiteMaterial = 0;
            int blackMaterial = 0;
            
            // Arrays to track pawn structure
            int[] whitePawnsInFile = new int[8];
            int[] blackPawnsInFile = new int[8];
            
            // Piece counts
            int whiteQueenCount = 0;
            int blackQueenCount = 0;
            
            // Mobility counts
            int whiteMobility = 0;
            int blackMobility = 0;
            
            // Center control
            int whiteCenterControl = 0;
            int blackCenterControl = 0;
            
            // Count pieces and evaluate position
            for (Column col = Column.a; col <= Column.h; col++)
            {
                for (int row = 0; row <= 7; row++)
                {
                    Square square = board.GetSquare(col, row);
                    if (square.Piece != null)
                    {
                        int pieceValue = PieceValues[(int)square.Piece.Type];
                        int positionValue = GetPositionValue(square.Piece, col, row);
                        
                        // Track material
                        if (square.Piece.Color == Color.White)
                        {
                            whiteMaterial += pieceValue;
                            score += pieceValue + positionValue;
                            
                            // Track queens
                            if (square.Piece.Type == PieceType.Queen)
                            {
                                whiteQueenCount++;
                            }
                            
                            // Track pawns for structure evaluation
                            if (square.Piece.Type == PieceType.Pawn)
                            {
                                whitePawnsInFile[(int)col]++;
                                
                                // Passed pawn check
                                bool isPassed = true;
                                for (int checkRow = row + 1; checkRow <= 7; checkRow++)
                                {
                                    // Check if there are any enemy pawns ahead or in adjacent files
                                    for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
                                    {
                                        int checkFile = (int)col + fileOffset;
                                        if (checkFile >= 0 && checkFile < 8)
                                        {
                                            Square checkSquare = board.GetSquare((Column)checkFile, checkRow);
                                            if (checkSquare.Piece != null && 
                                                checkSquare.Piece.Color == Color.Black && 
                                                checkSquare.Piece.Type == PieceType.Pawn)
                                            {
                                                isPassed = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!isPassed) break;
                                }
                                
                                if (isPassed)
                                {
                                    score += PASSED_PAWN_BONUS * (row + 1); // More bonus for advanced pawns
                                }
                                
                                // Protected pawn check
                                bool isProtected = false;
                                for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
                                {
                                    int checkFile = (int)col + fileOffset;
                                    if (checkFile >= 0 && checkFile < 8)
                                    {
                                        Square checkSquare = board.GetSquare((Column)checkFile, row - 1);
                                        if (checkSquare.Piece != null && 
                                            checkSquare.Piece.Color == Color.White && 
                                            checkSquare.Piece.Type == PieceType.Pawn)
                                        {
                                            isProtected = true;
                                            break;
                                        }
                                    }
                                }
                                
                                if (isProtected)
                                {
                                    score += PROTECTED_PAWN_BONUS;
                                }
                            }
                            
                            // Evaluate mobility
                            if (square.Piece.Type != PieceType.Pawn && square.Piece.Type != PieceType.King)
                            {
                                square.Piece.DetermineValidMoves();
                                int moveCount = BitBoard.PopCount(square.Piece.ValidMoves);
                                whiteMobility += moveCount * MobilityBonus[(int)square.Piece.Type];
                            }
                            
                            // Center control
                            if ((int)col >= 2 && (int)col <= 5 && row >= 2 && row <= 5)
                            {
                                if ((int)col >= 3 && (int)col <= 4 && row >= 3 && row <= 4)
                                {
                                    whiteCenterControl += CENTER_CONTROL_BONUS;
                                }
                                else
                                {
                                    whiteCenterControl += EXTENDED_CENTER_CONTROL_BONUS;
                                }
                            }
                        }
                        else // Black pieces
                        {
                            blackMaterial += pieceValue;
                            score -= pieceValue + positionValue;
                            
                            // Track queens
                            if (square.Piece.Type == PieceType.Queen)
                            {
                                blackQueenCount++;
                            }
                            
                            // Track pawns for structure evaluation
                            if (square.Piece.Type == PieceType.Pawn)
                            {
                                blackPawnsInFile[(int)col]++;
                                
                                // Passed pawn check
                                bool isPassed = true;
                                for (int checkRow = row - 1; checkRow >= 0; checkRow--)
                                {
                                    // Check if there are any enemy pawns ahead or in adjacent files
                                    for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
                                    {
                                        int checkFile = (int)col + fileOffset;
                                        if (checkFile >= 0 && checkFile < 8)
                                        {
                                            Square checkSquare = board.GetSquare((Column)checkFile, checkRow);
                                            if (checkSquare.Piece != null && 
                                                checkSquare.Piece.Color == Color.White && 
                                                checkSquare.Piece.Type == PieceType.Pawn)
                                            {
                                                isPassed = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!isPassed) break;
                                }
                                
                                if (isPassed)
                                {
                                    score -= PASSED_PAWN_BONUS * (7 - row + 1); // More bonus for advanced pawns
                                }
                                
                                // Protected pawn check
                                bool isProtected = false;
                                for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
                                {
                                    int checkFile = (int)col + fileOffset;
                                    if (checkFile >= 0 && checkFile < 8)
                                    {
                                        Square checkSquare = board.GetSquare((Column)checkFile, row + 1);
                                        if (checkSquare.Piece != null && 
                                            checkSquare.Piece.Color == Color.Black && 
                                            checkSquare.Piece.Type == PieceType.Pawn)
                                        {
                                            isProtected = true;
                                            break;
                                        }
                                    }
                                }
                                
                                if (isProtected)
                                {
                                    score -= PROTECTED_PAWN_BONUS;
                                }
                            }
                            
                            // Evaluate mobility
                            if (square.Piece.Type != PieceType.Pawn && square.Piece.Type != PieceType.King)
                            {
                                square.Piece.DetermineValidMoves();
                                int moveCount = BitBoard.PopCount(square.Piece.ValidMoves);
                                blackMobility += moveCount * MobilityBonus[(int)square.Piece.Type];
                            }
                            
                            // Center control
                            if ((int)col >= 2 && (int)col <= 5 && row >= 2 && row <= 5)
                            {
                                if ((int)col >= 3 && (int)col <= 4 && row >= 3 && row <= 4)
                                {
                                    blackCenterControl += CENTER_CONTROL_BONUS;
                                }
                                else
                                {
                                    blackCenterControl += EXTENDED_CENTER_CONTROL_BONUS;
                                }
                            }
                        }
                    }
                }
            }
            
            // Add mobility score
            score += whiteMobility - blackMobility;
            
            // Add center control score
            score += whiteCenterControl - blackCenterControl;
            
            // Evaluate pawn structure
            for (int file = 0; file < 8; file++)
            {
                // Doubled pawns
                if (whitePawnsInFile[file] > 1)
                {
                    score += DOUBLED_PAWN_PENALTY * (whitePawnsInFile[file] - 1);
                }
                if (blackPawnsInFile[file] > 1)
                {
                    score -= DOUBLED_PAWN_PENALTY * (blackPawnsInFile[file] - 1);
                }
                
                // Isolated pawns
                if (whitePawnsInFile[file] > 0)
                {
                    bool isIsolated = true;
                    for (int adjacentFile = file - 1; adjacentFile <= file + 1; adjacentFile += 2)
                    {
                        if (adjacentFile >= 0 && adjacentFile < 8 && whitePawnsInFile[adjacentFile] > 0)
                        {
                            isIsolated = false;
                            break;
                        }
                    }
                    if (isIsolated)
                    {
                        score += ISOLATED_PAWN_PENALTY * whitePawnsInFile[file];
                    }
                }
                if (blackPawnsInFile[file] > 0)
                {
                    bool isIsolated = true;
                    for (int adjacentFile = file - 1; adjacentFile <= file + 1; adjacentFile += 2)
                    {
                        if (adjacentFile >= 0 && adjacentFile < 8 && blackPawnsInFile[adjacentFile] > 0)
                        {
                            isIsolated = false;
                            break;
                        }
                    }
                    if (isIsolated)
                    {
                        score -= ISOLATED_PAWN_PENALTY * blackPawnsInFile[file];
                    }
                }
            }
            
            // Determine if we're in endgame (queens gone or material < threshold)
            bool isEndgame = (whiteMaterial + blackMaterial < 3000) || 
                             (whiteQueenCount == 0 && blackQueenCount == 0) ||
                             (whiteMaterial < 1200 || blackMaterial < 1200);
            
            // Evaluate king safety differently in endgame
            for (Column col = Column.a; col <= Column.h; col++)
            {
                for (int row = 0; row <= 7; row++)
                {
                    Square square = board.GetSquare(col, row);
                    if (square.Piece != null && square.Piece.Type == PieceType.King)
                    {
                        if (square.Piece.Color == Color.White)
                        {
                            if (isEndgame)
                            {
                                // In endgame, king should be active
                                score += KingEndTable[7 - row, (int)col];
                                
                                // King centralization in endgame
                                int distanceToCenter = Math.Max(Math.Abs(3 - (int)col), Math.Abs(3 - row));
                                score += (4 - distanceToCenter) * 10;
                            }
                            else
                            {
                                // In middlegame, king should be safe
                                score += KingMiddleTable[7 - row, (int)col];
                                
                                // King safety - pawn shield
                                int pawnShieldCount = 0;
                                for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
                                {
                                    int checkFile = (int)col + fileOffset;
                                    if (checkFile >= 0 && checkFile < 8)
                                    {
                                        for (int rankOffset = 1; rankOffset <= 2; rankOffset++)
                                        {
                                            int checkRank = row + rankOffset;
                                            if (checkRank < 8)
                                            {
                                                Square checkSquare = board.GetSquare((Column)checkFile, checkRank);
                                                if (checkSquare.Piece != null && 
                                                    checkSquare.Piece.Color == Color.White && 
                                                    checkSquare.Piece.Type == PieceType.Pawn)
                                                {
                                                    pawnShieldCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                                score += pawnShieldCount * KING_SHIELD_BONUS;
                                
                                // King safety - open files
                                if (whitePawnsInFile[(int)col] == 0)
                                {
                                    score += KING_OPEN_FILE_PENALTY;
                                }
                                else if (whitePawnsInFile[(int)col] == 1)
                                {
                                    score += KING_SEMI_OPEN_FILE_PENALTY;
                                }
                            }
                        }
                        else // Black king
                        {
                            if (isEndgame)
                            {
                                // In endgame, king should be active
                                score -= KingEndTable[row, (int)col];
                                
                                // King centralization in endgame
                                int distanceToCenter = Math.Max(Math.Abs(3 - (int)col), Math.Abs(3 - row));
                                score -= (4 - distanceToCenter) * 10;
                            }
                            else
                            {
                                // In middlegame, king should be safe
                                score -= KingMiddleTable[row, (int)col];
                                
                                // King safety - pawn shield
                                int pawnShieldCount = 0;
                                for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
                                {
                                    int checkFile = (int)col + fileOffset;
                                    if (checkFile >= 0 && checkFile < 8)
                                    {
                                        for (int rankOffset = 1; rankOffset <= 2; rankOffset++)
                                        {
                                            int checkRank = row - rankOffset;
                                            if (checkRank >= 0)
                                            {
                                                Square checkSquare = board.GetSquare((Column)checkFile, checkRank);
                                                if (checkSquare.Piece != null && 
                                                    checkSquare.Piece.Color == Color.Black && 
                                                    checkSquare.Piece.Type == PieceType.Pawn)
                                                {
                                                    pawnShieldCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                                score -= pawnShieldCount * KING_SHIELD_BONUS;
                                
                                // King safety - open files
                                if (blackPawnsInFile[(int)col] == 0)
                                {
                                    score -= KING_OPEN_FILE_PENALTY;
                                }
                                else if (blackPawnsInFile[(int)col] == 1)
                                {
                                    score -= KING_SEMI_OPEN_FILE_PENALTY;
                                }
                            }
                        }
                    }
                }
            }
            
            // Adjust score based on whose turn it is
            return currentPlayerColor == Color.White ? score : -score;
        }
    }
}
