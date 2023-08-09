// See https://aka.ms/new-console-template for more information

public class King : Piece
{
    public override PieceType Type => PieceType.King;
    protected override int[,] MoveDirections => new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { 1, 1 }, { 1, -1 }, { -1, 0 }, { -1, 1 }, { -1, -1 } };
    protected override int MaxMoveSteps => 1;
    private Square shortCastleSquare, longCastleSquare;
    public King(IBoard board, Color color) : base(board, color) 
    { 
        _name = "k";
        if (Color == Color.White)
        {
            shortCastleSquare = _board.GetSquare(Column.g, 0);
            longCastleSquare = _board.GetSquare(Column.c, 0);
        }
        else
        {
            shortCastleSquare = _board.GetSquare(Column.g, 7);
            longCastleSquare = _board.GetSquare(Column.c, 7);
        }
    }
    public override MoveResultStruct Move(Square toSquare)
    {
        MoveResultStruct result = new MoveResultStruct();
        result.MoveResult = MoveResult.Invalid;        

        int nrOfColumns = Math.Abs(Square!.Column - toSquare.Column);
        int nrOfRows = Math.Abs(Square!.Row - toSquare.Row);
        bool stillOk = (nrOfColumns == 1 || nrOfRows == 1);
        stillOk = stillOk && (toSquare.Piece == null || toSquare.Piece.Color == _oppositeColor);

        if (stillOk)  // "normal" move
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
        else // castling move?
        {
            if (toSquare == shortCastleSquare && BitBoard.IsBitSet(ValidMoves, shortCastleSquare.Row, shortCastleSquare.Column))
            {
                result.MoveResult = MoveResult.CastleShort;
            }
            else if (toSquare == longCastleSquare && BitBoard.IsBitSet(ValidMoves, longCastleSquare.Row, longCastleSquare.Column))
            {
                result.MoveResult = MoveResult.CastleLong;
            }
        }
        return result; 
    }
    // true if no pieces between king and rook and no checks
    private bool CanCastle(Square castlingSquare) 
    {
        bool result = false;
        if (!_board.HasPieceBetween(Square!, castlingSquare))
        {
            bool isCheck = false;
            int direction = Square!.Column < castlingSquare.Column? 1: -1;
            Square square = _board.GetSquare(Square.Column, Square.Row);
            do
            {
                square = _board.GetSquare(square.Column + direction, square.Row);
                isCheck = _board.IsSquareInCheck(square, Color);                
            }
            while (!isCheck && square != castlingSquare) ;
            result = !isCheck;
        }        
        return result;
    }
    public override void DetermineValidMoves()
    {
        base.DetermineValidMoves();

        // Castling
        CheckState checkState = _board.GetCheckState();
        if ((Color == Color.Black && checkState != CheckState.Black) ||
            (Color == Color.White && checkState != CheckState.White))
        {
            CastlingRight castlingRight = _board.GetCastlingRight(Color);
            if (castlingRight == CastlingRight.Both || castlingRight == CastlingRight.Short)
            {
                if (CanCastle(shortCastleSquare))
                {
                    ValidMoves = BitBoard.SetBit(ValidMoves, shortCastleSquare.Row, shortCastleSquare.Column);
                }
            }
            if (castlingRight == CastlingRight.Both || castlingRight == CastlingRight.Long)
            {
                if (CanCastle(longCastleSquare))
                {
                    ValidMoves = BitBoard.SetBit(ValidMoves, longCastleSquare.Row, longCastleSquare.Column);
                }
            }
        }
    }
}

