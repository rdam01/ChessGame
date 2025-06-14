using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessGame
{
    public class AI : Player
    {
        private readonly IBoard _board;
        private readonly int _searchDepth;
        private readonly Random _random;
        private readonly int _maxQuiescenceDepth;
        private readonly bool _useNullMovePruning;
        private readonly int _nullMoveReduction;
        private readonly int _lateMovePruningDepth;
        private readonly int _historyPruningThreshold;
        
        // History heuristic for move ordering
        private readonly int[,] _historyTable;

        // Transposition table for caching evaluated positions
        private readonly Dictionary<ulong, TranspositionEntry> _transpositionTable;

        // Killer moves for move ordering
        private readonly int[,] _killerMoves;

        public AI(IBoard board, Color color, int searchDepth = 7)
        {
            _board = board;
            Color = color;
            _searchDepth = searchDepth;
            _random = new Random();
            _transpositionTable = new Dictionary<ulong, TranspositionEntry>(1000000); // Larger table for ELO 2000
            _killerMoves = new int[_searchDepth + 4, 2]; // Store 2 killer moves per ply, with extra space for quiescence
            _historyTable = new int[64, 64]; // From-To square history heuristic
            _maxQuiescenceDepth = 8; // Deeper quiescence search for tactical awareness
            _useNullMovePruning = true; // Enable null move pruning
            _nullMoveReduction = 3; // R=3 for null move pruning
            _lateMovePruningDepth = 3; // Start late move pruning at this depth
            _historyPruningThreshold = 50; // Threshold for history-based pruning
        }

        // Get the best move for the current position
        public MoveResultStruct GetBestMove()
        {
            Console.WriteLine($"AI is thinking (depth {_searchDepth})...");
            
            // Clear killer moves for new search
            Array.Clear(_killerMoves, 0, _killerMoves.Length);
            
            // Get all valid moves for the AI
            List<MoveResultStruct> validMoves = AISearch.GenerateValidMoves(_board, Color);
            
            if (validMoves.Count == 0)
            {
                // No valid moves, return an invalid move
                return new MoveResultStruct { MoveResult = MoveResult.Invalid };
            }
            
            // Randomize move order slightly to avoid predictability
            validMoves = validMoves.OrderBy(x => _random.Next()).ToList();
            
            int bestScore = int.MinValue;
            MoveResultStruct bestMove = validMoves[0];
            
            // Iterative deepening
            for (int currentDepth = 1; currentDepth <= _searchDepth; currentDepth++)
            {
                int alpha = int.MinValue;
                int beta = int.MaxValue;
                
                foreach (MoveResultStruct move in validMoves)
                {
                    // Make the move
                    /* AIHelpers.MakeMove(move); */
                    MoveResultStruct moveResult = _board.Move(Color, move.From, move.To);

                    if (moveResult.MoveResult != MoveResult.Invalid)
                    {
                        // Evaluate the position after the move
                        Color oppositeColor = Color == Color.White ? Color.Black : Color.White;
                        int score = -AISearch.AlphaBeta(_board, oppositeColor, -beta, -alpha, currentDepth - 1, 1,
                                                      _transpositionTable, _killerMoves, _historyTable,
                                                      _nullMoveReduction, _useNullMovePruning,
                                                      _lateMovePruningDepth, _historyPruningThreshold, _random);

                        // Undo the move
                        /* AIHelpers.UndoMove(move); */
                        _board.UndoMove(moveResult);

                        // Update best move if this move is better
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }

                        // Update alpha
                        alpha = Math.Max(alpha, score);
                    }
                }
                
                // Reorder moves based on previous iteration
                validMoves = AISearch.OrderMoves(validMoves, 0, _killerMoves, _historyTable);
                
                Console.WriteLine($"Depth {currentDepth} completed. Best move: {AIHelpers.GetMoveNotation(bestMove)} (score: {bestScore})");
            }
            
            return bestMove;
        }
    }
}
