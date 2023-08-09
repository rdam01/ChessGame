// See https://aka.ms/new-console-template for more information

public class Rook : Piece
{
    public override PieceType Type => PieceType.Rook;
    public RookSide RookSide { get; init; }
    protected override int[,] MoveDirections => new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
    protected override int MaxMoveSteps => 8;
    public Rook(IBoard board, Color color, RookSide side) : base(board, color)
    {
        _name = "r";
        RookSide = side;
    }
    public override MoveResultStruct Move(Square toSquare)
    {
        MoveResultStruct result = new MoveResultStruct();
        bool stillOk = Square.IsVertical(Square!, toSquare) || Square.IsHorizontal(Square!, toSquare);
        stillOk = stillOk && !_board.HasPieceBetween(Square!, toSquare);
        stillOk = stillOk && (toSquare.Piece == null || toSquare.Piece.Color == _oppositeColor);
        if (stillOk)
        {
            if (toSquare.HasPiece()) // capture
            {
                result.MoveResult = MoveResult.Capture;
                result.CapturedPiece = toSquare.Piece;
            }
            else
            {
                result.MoveResult = MoveResult.OK;
            }
        }
        return result;
    }
}

