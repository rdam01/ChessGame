// See https://aka.ms/new-console-template for more information
using ChessGame;

public class Game: IGame
{
    readonly Player _player1;
    readonly Player _player2;
    Player _currentPlayer;
    readonly Board _board;
    private GameResult _result;
    private MoveHistory _moveHistory;
    private bool _useAI;
    private int _aiSearchDepth;

    public Game(bool useAI = false, int aiSearchDepth = 7)
    {
        _useAI = useAI;
        _aiSearchDepth = aiSearchDepth;
        _board = new Board(this);
        
        if (_useAI)
        {
            // Human player is white, AI is black
            _player1 = new Player
            {
                Color = Color.White
            };
            _player2 = new AI(_board, Color.Black, _aiSearchDepth);
        }
        else
        {
            // Regular two-player game
            _player1 = new Player
            {
                Color = Color.White
            };
            _player2 = new Player
            {
                Color = Color.Black
            };
        }
        
        _currentPlayer = _player1;
        _result = GameResult.Undecided;
        _moveHistory = new MoveHistory();
    }
    
    public void Play()
    {
        MoveResultStruct moveResult;
        while (_result == GameResult.Undecided)
        {
            _board.Draw();
            
            if (_currentPlayer is AI aiPlayer)
            {
                // AI's turn
                Console.WriteLine($"AI ({aiPlayer.Color}) is thinking...");
                moveResult = aiPlayer.GetBestMove();
                
                // Make the move on the board
                if (moveResult.MoveResult != MoveResult.Invalid)
                {
                    moveResult = _board.Move(aiPlayer.Color, moveResult.From, moveResult.To);
                }
            }
            else
            {
                // Human player's turn
                moveResult = _board.Move(_currentPlayer.Color);
            }
            
            _moveHistory.AddMove(moveResult);

            DetermineGameResult(moveResult);

            if (_result == GameResult.Undecided)
            {
                _currentPlayer = NextPlayer();
            }
            else
            {
                _board.Draw();
                ShowGameResult();
            }
        }
    }
    
    private Player NextPlayer() 
    {
        return _currentPlayer == _player1? _player2 : _player1;
    }
    
    private void DetermineGameResult(MoveResultStruct moveResult)
    {
        switch (moveResult.MoveResult)
        {
            case MoveResult.Checkmate:
                _result = _board.CheckState == CheckState.White ? GameResult.BlackWins : GameResult.WhiteWins;
                break;
            case MoveResult.Stalemate:
                _result = GameResult.Draw;
                break;
        }
    }
    
    private void ShowGameResult() 
    {
        switch (_result) 
        {
            case GameResult.BlackWins:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Black wins by checkmate!");
                break;
            case GameResult.WhiteWins:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("White wins by checkmate!");
                break;
            case GameResult.Draw:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("The game is a draw!");
                break;
        }
    }
    
    public void DrawStatus()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        // Notation
        if (_moveHistory.Notation.Trim() != "")
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine(_moveHistory.Notation);
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine($"{_currentPlayer.Color} is to move");
        
        if (_currentPlayer is AI)
        {
            Console.WriteLine("AI is thinking...");
        }
    }
}

public interface IGame
{
    public void DrawStatus();
}
