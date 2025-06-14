using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessGame
{
    // Search algorithms for the AI
    public static class AISearch
    {
        // Alpha-Beta pruning algorithm
        public static int AlphaBeta(IBoard board, Color currentPlayerColor, int alpha, int beta, int depth, int ply, 
                                   Dictionary<ulong, TranspositionEntry> transpositionTable,
                                   int[,] killerMoves, int[,] historyTable,
                                   int nullMoveReduction, bool useNullMovePruning,
                                   int lateMovePruningDepth, int historyPruningThreshold,
                                   Random random)
        {
            // Check transposition table
            ulong positionHash = AIZobrist.GetPositionHash(board, currentPlayerColor);
            if (transpositionTable.TryGetValue(positionHash, out TranspositionEntry entry) && entry.Depth >= depth)
            {
                if (entry.Type == TranspositionEntryType.Exact)
                {
                    return entry.Score;
                }
                else if (entry.Type == TranspositionEntryType.LowerBound)
                {
                    alpha = Math.Max(alpha, entry.Score);
                }
                else if (entry.Type == TranspositionEntryType.UpperBound)
                {
                    beta = Math.Min(beta, entry.Score);
                }
                
                if (alpha >= beta)
                {
                    return entry.Score;
                }
            }
            
            // Base case: leaf node
            if (depth <= 0)
            {
                return Quiescence(board, currentPlayerColor, alpha, beta, 0, transpositionTable);
            }
            
            // Check for draw by repetition or insufficient material
            if (AIHelpers.IsDrawnPosition(board))
            {
                return 0; // Draw score
            }
            
            // Null move pruning (skip your turn to see if position is still good)
            if (useNullMovePruning && depth >= 3 && !AIHelpers.IsInCheck(board, currentPlayerColor) && 
                !AIHelpers.IsEndgamePosition(board, AIEvaluation.PieceValues))
            {
                // Skip turn and search with reduced depth
                Color oppositeColor = currentPlayerColor == Color.White ? Color.Black : Color.White;
                int nullMoveScore = -AlphaBeta(board, oppositeColor, -beta, -beta + 1, depth - 1 - nullMoveReduction, 
                                             ply + 1, transpositionTable, killerMoves, historyTable,
                                             nullMoveReduction, useNullMovePruning, 
                                             lateMovePruningDepth, historyPruningThreshold, random);
                
                // If skipping our turn still gives a cutoff, we're probably winning by a lot
                if (nullMoveScore >= beta)
                {
                    return beta; // Fail-high
                }
            }
            
            // Generate valid moves
            List<MoveResultStruct> moves = GenerateValidMoves(board, currentPlayerColor);
            
            // Check for checkmate or stalemate
            if (moves.Count == 0)
            {
                // If in check, it's checkmate
                if (board.GetCheckState() != CheckState.None)
                {
                    return -20000 + ply; // Prefer checkmate in fewer moves
                }
                else
                {
                    return 0; // Stalemate
                }
            }
            
            // Order moves for better pruning
            moves = OrderMoves(moves, ply, killerMoves, historyTable);
            
            // Principal Variation Search (PVS)
            bool searchPV = true;
            
            int bestScore = int.MinValue;
            TranspositionEntryType entryType = TranspositionEntryType.UpperBound;
            MoveResultStruct bestMove = new MoveResultStruct();
            
            for (int i = 0; i < moves.Count; i++)
            {
                MoveResultStruct move = moves[i];
                int moveIndex = AIHelpers.GetMoveIndex(move);
                
                // Late Move Reduction (LMR)
                // Skip less promising moves at deeper depths
                if (i >= 4 && depth >= lateMovePruningDepth && !AIHelpers.IsCapture(move) && 
                    move.MoveResult != MoveResult.Check && move.MoveResult != MoveResult.Promotion &&
                    historyTable[moveIndex / 64, moveIndex % 64] < historyPruningThreshold)
                {
                    // Skip this move with some probability
                    if (random.Next(100) < 20) // 20% chance to skip
                    {
                        continue;
                    }
                }
                
                // Make the move
                AIHelpers.MakeMove(move);
                
                int score;
                Color oppositeColor = currentPlayerColor == Color.White ? Color.Black : Color.White;
                
                // Principal Variation Search optimization
                if (searchPV)
                {
                    // Full window search for first move
                    score = -AlphaBeta(board, oppositeColor, -beta, -alpha, depth - 1, ply + 1, 
                                     transpositionTable, killerMoves, historyTable,
                                     nullMoveReduction, useNullMovePruning, 
                                     lateMovePruningDepth, historyPruningThreshold, random);
                }
                else
                {
                    // Reduced window search for remaining moves
                    score = -AlphaBeta(board, oppositeColor, -alpha - 1, -alpha, depth - 1, ply + 1, 
                                     transpositionTable, killerMoves, historyTable,
                                     nullMoveReduction, useNullMovePruning, 
                                     lateMovePruningDepth, historyPruningThreshold, random);
                    
                    // If the reduced search indicates this might be a better move,
                    // do a full re-search
                    if (score > alpha && score < beta)
                    {
                        score = -AlphaBeta(board, oppositeColor, -beta, -alpha, depth - 1, ply + 1, 
                                         transpositionTable, killerMoves, historyTable,
                                         nullMoveReduction, useNullMovePruning, 
                                         lateMovePruningDepth, historyPruningThreshold, random);
                    }
                }
                
                // Undo the move
                AIHelpers.UndoMove(move);
                
                // Update best score
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                
                // Update alpha
                if (score > alpha)
                {
                    alpha = score;
                    entryType = TranspositionEntryType.Exact;
                    searchPV = false; // No longer searching PV
                    
                    // Update history table for good moves
                    historyTable[moveIndex / 64, moveIndex % 64] += depth * depth;
                    
                    // Store killer move if it's a good non-capture move
                    if (move.CapturedPiece == null && move.MoveResult != MoveResult.Promotion)
                    {
                        killerMoves[ply, 1] = killerMoves[ply, 0];
                        killerMoves[ply, 0] = moveIndex;
                    }
                }
                
                // Beta cutoff
                if (alpha >= beta)
                {
                    entryType = TranspositionEntryType.LowerBound;
                    
                    // Update history table for cutoff moves
                    historyTable[moveIndex / 64, moveIndex % 64] += depth * depth;
                    
                    // Store killer move if it's a good non-capture move
                    if (move.CapturedPiece == null && move.MoveResult != MoveResult.Promotion)
                    {
                        killerMoves[ply, 1] = killerMoves[ply, 0];
                        killerMoves[ply, 0] = moveIndex;
                    }
                    
                    break;
                }
            }
            
            // Store position in transposition table
            transpositionTable[positionHash] = new TranspositionEntry
            {
                Score = bestScore,
                Depth = depth,
                Type = entryType
            };
            
            return bestScore;
        }

        // Quiescence search to avoid horizon effect
        public static int Quiescence(IBoard board, Color currentPlayerColor, int alpha, int beta, int depth = 0,
                                    Dictionary<ulong, TranspositionEntry> transpositionTable = null)
        {
            // Check transposition table
            if (transpositionTable != null)
            {
                ulong positionHash = AIZobrist.GetPositionHash(board, currentPlayerColor);
                if (transpositionTable.TryGetValue(positionHash, out TranspositionEntry entry) && entry.Depth >= depth)
                {
                    if (entry.Type == TranspositionEntryType.Exact)
                    {
                        return entry.Score;
                    }
                    else if (entry.Type == TranspositionEntryType.LowerBound)
                    {
                        alpha = Math.Max(alpha, entry.Score);
                    }
                    else if (entry.Type == TranspositionEntryType.UpperBound)
                    {
                        beta = Math.Min(beta, entry.Score);
                    }
                    
                    if (alpha >= beta)
                    {
                        return entry.Score;
                    }
                }
            }
            
            // Stand-pat score
            int standPat = AIEvaluation.EvaluatePosition(board, currentPlayerColor);
            
            // Beta cutoff
            if (standPat >= beta)
            {
                return beta;
            }
            
            // Update alpha
            if (standPat > alpha)
            {
                alpha = standPat;
            }
            
            // Maximum quiescence depth to avoid excessive searching
            if (depth >= 8) // Configurable max depth
            {
                return standPat;
            }
            
            // Check for checks at shallow depths
            if (depth < 2 && AIHelpers.IsInCheck(board, currentPlayerColor))
            {
                // If in check, we need to consider all moves, not just captures
                List<MoveResultStruct> allMoves = GenerateValidMoves(board, currentPlayerColor);
                
                // Order moves
                allMoves = OrderMoves(allMoves, depth);
                
                foreach (MoveResultStruct move in allMoves)
                {
                    // Make the move
                    AIHelpers.MakeMove(move);
                    
                    // Recursively evaluate the position
                    Color oppositeColor = currentPlayerColor == Color.White ? Color.Black : Color.White;
                    int score = -Quiescence(board, oppositeColor, -beta, -alpha, depth + 1, transpositionTable);
                    
                    // Undo the move
                    AIHelpers.UndoMove(move);
                    
                    // Update alpha
                    if (score > alpha)
                    {
                        alpha = score;
                    }
                    
                    // Beta cutoff
                    if (alpha >= beta)
                    {
                        return beta;
                    }
                }
                
                return alpha;
            }
            
            // Generate capture moves only
            List<MoveResultStruct> captureMoves = GenerateValidMoves(board, currentPlayerColor)
                .Where(m => m.CapturedPiece != null).ToList();
            
            // Order moves by MVV-LVA (Most Valuable Victim - Least Valuable Aggressor)
            captureMoves = captureMoves.OrderByDescending(m => 
                (m.CapturedPiece != null ? AIEvaluation.PieceValues[(int)m.CapturedPiece.Type] : 0) - 
                (m.Piece != null ? AIEvaluation.PieceValues[(int)m.Piece.Type] / 10 : 0)
            ).ToList();
            
            foreach (MoveResultStruct move in captureMoves)
            {
                // Make the move
                AIHelpers.MakeMove(move);
                
                // Recursively evaluate the position
                Color oppositeColor = currentPlayerColor == Color.White ? Color.Black : Color.White;
                int score = -Quiescence(board, oppositeColor, -beta, -alpha, depth + 1, transpositionTable);
                
                // Undo the move
                AIHelpers.UndoMove(move);
                
                // Update alpha
                if (score > alpha)
                {
                    alpha = score;
                }
                
                // Beta cutoff
                if (alpha >= beta)
                {
                    return beta;
                }
            }
            
            return alpha;
        }

        // Generate all valid moves for the current player
        public static List<MoveResultStruct> GenerateValidMoves(IBoard board, Color color)
        {
            List<MoveResultStruct> validMoves = new List<MoveResultStruct>();
            
            // Get all pieces of the current player's color
            List<Piece> pieces = new List<Piece>();
            
            for (Column col = Column.a; col <= Column.h; col++)
            {
                for (int row = 0; row <= 7; row++)
                {
                    Square square = board.GetSquare(col, row);
                    if (square.Piece != null && square.Piece.Color == color)
                    {
                        pieces.Add(square.Piece);
                    }
                }
            }
            
            // For each piece, find all valid moves
            foreach (Piece piece in pieces)
            {
                piece.DetermineValidMoves();
                ulong validMovesBitboard = piece.ValidMoves;
                
                while (validMovesBitboard != 0)
                {
                    // Find the least significant bit (first valid move)
                    int index = BitBoard.bitScanForward(validMovesBitboard);
                    
                    // Convert index to square coordinates
                    int targetRow = index / 8;
                    Column targetCol = (Column)(index % 8);
                    
                    // Get the target square
                    Square targetSquare = board.GetSquare(targetCol, targetRow);
                    
                    // Try the move
                    MoveResultStruct moveResult = piece.Move(targetSquare);
                    
                    // If the move is valid, add it to the list
                    if (moveResult.MoveResult != MoveResult.Invalid)
                    {
                        moveResult.Piece = piece;
                        moveResult.From = piece.Square!;
                        moveResult.To = targetSquare;
                        
                        // Check if the move would put the player in check
                        AIHelpers.MakeMove(moveResult);
                        bool selfCheck = board.IsSquareInCheck(
                            board.GetSquare(targetCol, targetRow), 
                            color
                        );
                        AIHelpers.UndoMove(moveResult);
                        
                        if (!selfCheck)
                        {
                            validMoves.Add(moveResult);
                        }
                    }
                    
                    // Clear the least significant bit
                    validMovesBitboard &= validMovesBitboard - 1;
                }
            }
            
            return validMoves;
        }

        // Order moves for better alpha-beta pruning
        public static List<MoveResultStruct> OrderMoves(List<MoveResultStruct> moves, int ply = 0, 
                                                      int[,] killerMoves = null, int[,] historyTable = null)
        {
            return moves.OrderByDescending(move => 
            {
                int score = 0;
                
                // Captures
                if (move.CapturedPiece != null)
                {
                    // MVV-LVA (Most Valuable Victim - Least Valuable Aggressor)
                    score += 10 * AIEvaluation.PieceValues[(int)move.CapturedPiece.Type] - 
                             AIEvaluation.PieceValues[(int)move.Piece.Type] / 10;
                }
                
                // Promotions
                if (move.MoveResult == MoveResult.Promotion)
                {
                    score += 900; // Assume queen promotion
                }
                
                // Check
                if (move.MoveResult == MoveResult.Check || move.MoveResult == MoveResult.Checkmate)
                {
                    score += 50;
                }
                
                // Killer moves
                if (killerMoves != null)
                {
                    int moveIndex = AIHelpers.GetMoveIndex(move);
                    if (moveIndex == killerMoves[ply, 0])
                    {
                        score += 30;
                    }
                    else if (moveIndex == killerMoves[ply, 1])
                    {
                        score += 20;
                    }
                }
                
                // History heuristic
                if (historyTable != null)
                {
                    int moveIndex = AIHelpers.GetMoveIndex(move);
                    score += historyTable[moveIndex / 64, moveIndex % 64] / 100;
                }
                
                return score;
            }).ToList();
        }
    }
}
