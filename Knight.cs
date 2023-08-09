// See https://aka.ms/new-console-template for more information

public class Knight : Piece
{
    public override PieceType Type => PieceType.Knight;
    protected override int[,] MoveDirections => new int[,] { { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 }, { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 } };
    protected override int MaxMoveSteps => 1; // 1 jump
    public Knight(IBoard board, Color color) : base(board, color)
    {
        _name = "n";
    }
    public override MoveResultStruct Move(Square toSquare) 
    { 
        MoveResultStruct result = new MoveResultStruct();
        int nrOfColumns = Math.Abs(Square!.Column - toSquare.Column);
        int nrOfRows = Math.Abs(Square.Row - toSquare.Row);

        bool stillOk = (nrOfColumns == 1 && nrOfRows == 2) ||
                       (nrOfRows == 1 && nrOfColumns == 2);
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

