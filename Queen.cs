// See https://aka.ms/new-console-template for more information

public class Queen : Piece
{
    public override PieceType Type => PieceType.Queen;
    protected override int[,] MoveDirections => new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { 1, 1 }, { 1, -1 }, { -1, 0 }, { -1, 1 }, { -1, -1 } };
    protected override int MaxMoveSteps => 8;
    public Queen(IBoard board, Color color) : base(board, color) 
    {
        _name = "q";
    }
    public override MoveResultStruct Move(Square toSquare)
    {
        MoveResultStruct result = new MoveResultStruct();
        bool stillOk = Square.IsVertical(Square!, toSquare) || 
                       Square.IsHorizontal(Square!, toSquare) ||
                       Square.IsDiagonal(Square!, toSquare);
        stillOk = stillOk && !_board.HasPieceBetween(Square!, toSquare);
        stillOk = stillOk && (toSquare.Piece == null || toSquare.Piece.Color == _oppositeColor);
        if (stillOk)
        {
            if (toSquare.HasPiece()) // capture
            {
                result.MoveResult = MoveResult.Capture;
                result.CapturedPiece = toSquare.Piece; // TODO: move to base class?
            }
            else
            {
                result.MoveResult = MoveResult.OK;
            }
        }
        return result;
    }
}

