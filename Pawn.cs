// See https://aka.ms/new-console-template for more information

public class Pawn : Piece
{
    public override PieceType Type => PieceType.Pawn;
    protected override int[,] MoveDirections { get => Color == Color.White ? new int[,] { { 0, 1 }, { 1, 1 }, { -1, 1 } }:
                                                                             new int[,] { { 0, -1 }, { 1, -1 }, { -1, -1 } }; }
    protected override int MaxMoveSteps => 1; // except first jump...
    private int _promotionRow;
    public Pawn(IBoard board, Color color) : base(board, color) 
    { 
        _name = "p";
        _promotionRow = Color == Color.Black ? 0 : 7;
    }
    public override MoveResultStruct Move(Square toSquare) 
    {
        MoveResultStruct result = new MoveResultStruct();        
        result.MoveResult = MoveResult.Invalid;        

        int nrOfRows = toSquare.Row - Square!.Row;
        int direction = Color == Color.White ? 1 : -1;
        nrOfRows *= direction;

        switch (nrOfRows)
        {
            case 1:
                if (toSquare.Column == Square.Column) // forward move                    
                {
                    if (toSquare.HasPiece())
                    {
                        result.MoveResult = MoveResult.Invalid;
                    }
                    else if (toSquare.Row == _promotionRow)
                    {
                        result.MoveResult = MoveResult.Promotion;
                    }
                    else
                    {
                        result.MoveResult = MoveResult.OK;
                    }
                }
                else if (Math.Abs(toSquare.Column - Square.Column) == 1) // capture
                {
                    if (toSquare.Piece == null) 
                    {
                        if (toSquare == _board.EnPassantSquare)
                        {
                            result.MoveResult = MoveResult.Capture;
                            result.CapturedPiece = _board.GetSquare(toSquare.Column, toSquare.Row - direction).Piece;                            
                        }
                    }
                    else if (toSquare.Piece.Color == _oppositeColor)
                    {
                        if (toSquare.Row == _promotionRow)
                        {
                            result.MoveResult = MoveResult.Promotion;
                        }
                        else
                        {
                            result.MoveResult = MoveResult.Capture;
                        }
                        result.CapturedPiece = toSquare.Piece;
                    }                    
                }
                break;
            case 2:
                if (Color == Color.White && Square.Row == 1 ||
                    Color == Color.Black && Square.Row == 6)
                {
                    if (!_board.HasPieceBetween(Square, toSquare))
                    {
                        result.MoveResult = MoveResult.OK;
                        _board.EnPassantSquare = _board.GetSquare(toSquare.Column, toSquare.Row - direction);
                    }
                }
                break;
            default:
                result.MoveResult = MoveResult.Invalid;
                break;
        }
        return result; 
    }
    private bool CanCapture(Square? toSquare)
    {
        return ((toSquare != null) &&
                (toSquare.Piece != null && toSquare.Piece.Color == _oppositeColor) ||
                (toSquare == _board.EnPassantSquare));
    }
    public override void DetermineValidMoves()
    {
        ValidMoves = 0;
        int direction = Color == Color.White ? 1 : -1;

        // 1 step forward
        Square? toSquare = _board.GetSquare(Square!.Column, Square!.Row + direction);
        if (toSquare != null && toSquare.Piece == null)
        {
            ValidMoves = BitBoard.SetBit(ValidMoves, toSquare!.Row, toSquare!.Column);
        }

        // 2 steps forward        
        if ((ValidMoves != 0) &&
            ((Color == Color.White && Square!.Row == 1 || Color == Color.Black && Square!.Row == 6)))
        {
            toSquare = _board.GetSquare(Square!.Column, Square!.Row + 2 * direction);
            if (toSquare!.Piece == null) // only valid if no piece on square
            {
                ValidMoves = BitBoard.SetBit(ValidMoves, toSquare!.Row, toSquare!.Column);                    
            }
        }

        // captures + enpassant
        toSquare = _board.GetSquare(Square!.Column + direction, Square!.Row + direction);
        if (toSquare != null && CanCapture(toSquare))
        {
            ValidMoves = BitBoard.SetBit(ValidMoves, toSquare!.Row, toSquare!.Column);
        }
        toSquare = _board.GetSquare(Square!.Column - direction, Square!.Row + direction);
        if (toSquare != null && CanCapture(toSquare))
        {
            ValidMoves = BitBoard.SetBit(ValidMoves, toSquare!.Row, toSquare!.Column);
        }        
    }
}

