// See https://aka.ms/new-console-template for more information

public class Bishop : Piece
{
    public override PieceType Type => PieceType.Bishop;
    protected override int[,] MoveDirections => new int[,] { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
    protected override int MaxMoveSteps => 8;
    public Bishop(IBoard board, Color color) : base(board, color)
    {
        _name = "b";
    }
    public override MoveResultStruct Move(Square toSquare) 
    {
        MoveResultStruct result = new MoveResultStruct();
        bool stillOk = Square.IsDiagonal(Square!, toSquare);
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

