// See https://aka.ms/new-console-template for more information

public abstract class Piece: IPiece
{
    public abstract PieceType Type { get; }
    protected abstract int[,] MoveDirections { get; }
    protected abstract int MaxMoveSteps { get; } // Max nr of squares the piece can make (with regard to MoveDirection)
    protected String _name = "";
    protected IBoard _board;
    public Square? Square { get; set; }
    public Color Color { get; private set; }
    protected Color _oppositeColor;
    protected bool IsValidMove(Square? toSquare)
    {
        return (toSquare != null && (toSquare.Piece == null || toSquare.Piece.Color == _oppositeColor));
    }
    public abstract MoveResultStruct Move(Square toSquare);
    public virtual void DetermineValidMoves() // regardless of selfcheck
    {
        ValidMoves = 0;

        Square curSquare = Square!;
        for (int i = 0; i < MoveDirections.GetLength(0); i++)
        {
            Column col = Square!.Column + MoveDirections[i, 0];
            int row = Square.Row + MoveDirections[i, 1];
            int nrOfSteps = 0;
            while (nrOfSteps < MaxMoveSteps && Board.IsValidSquare(col, row))
            {
                curSquare = _board.GetSquare(col, row);
                // valid if empty or opposite color
                if (curSquare.Piece == null || curSquare.Piece.Color == _oppositeColor)
                {
                    ValidMoves = BitBoard.SetBit(ValidMoves, row, col);
                }
                nrOfSteps++;
            }
            col += MoveDirections[i, 0];
            row += MoveDirections[i, 1];
        }
    }
    public UInt64 ValidMoves { get; protected set; }

    public Piece(IBoard board, Color color)
    {
        Color = color;
        _board = board;
        _oppositeColor = color == Color.Black ? Color.White : Color.Black;
    }
    public virtual void Draw()
    {
        if (Color == Color.White)
        {
            Console.Write($"{_name.ToUpper()}");
        }
        else
        {
            Console.Write($"{_name.ToLower()}");
        }
    }
    public override string ToString()
    {
        if (Color == Color.White)
            return _name.ToUpper();
        else
            return _name.ToLower();
    }
}

public interface IPiece
{
    public MoveResultStruct Move(Square square);
    public void DetermineValidMoves();
    public void Draw();
}
