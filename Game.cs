// See https://aka.ms/new-console-template for more information

public class Game: IGame
{
    readonly Player _player1;
    readonly Player _player2;
    Player _currentPlayer;
    readonly Board _board;
    private GameResult _result;
    private MoveHistory _moveHistory;

    public Game()
    {
        _player1 = new Player
        {
            Color = Color.White
        };
        _player2 = new Player
        {
            Color = Color.Black
        };
        _currentPlayer = _player1;
        _board = new Board(this);
        _result = GameResult.Undecided;
        _moveHistory = new MoveHistory();
    }
    public void Play()
    {
        MoveResultStruct moveResult;
        while (_result == GameResult.Undecided)
        {
            _board.Draw();
            moveResult = _board.Move(_currentPlayer.Color);
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
    }
}
public interface IGame
{
    public void DrawStatus();
}
