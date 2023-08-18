// See https://aka.ms/new-console-template for more information

public class Board: IBoard
{
    private readonly IGame _game;
    private readonly Square[,] _squares;
    public Square? EnPassantSquare { get; set; }
    private Piece WhiteKing { get; init; }
    private List<Piece> WhitePawns { get; init; }
    private Piece BlackKing { get; init; }
    private List<Piece> _whitePieces;
    private List<Piece> _blackPieces;
    private List<Piece> BlackPawns { get; init; }
    public CheckState CheckState { get; private set; }
    public CheckState GetCheckState()
    {
        return CheckState;
    }
    public List<Piece>CheckingPieces { get; private set; }
    private int[,] CheckDirections => new int[8, 2] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { 1, 1 }, { 1, -1 }, { -1, 0 }, { -1, 1 }, { -1, -1 } };
    private int[,] CheckKnightDirections => new int[,] { { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 }, { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 } };
    private CastlingState[] CastlingState = new CastlingState[2];    

public Board(IGame game)
    {
        _game = game;
        _squares = new Square[8, 8];        
        CheckingPieces = new List<Piece>();
        Init();
        // Piece may use Board.GetSquare
        WhiteKing = new King(this, Color.White);
        WhitePawns = new List<Piece>();
        BlackKing = new King(this, Color.Black);
        BlackPawns = new List<Piece>();
        _whitePieces = new List<Piece>();
        _blackPieces = new List<Piece>();
        AddPieces();
        //AddPiecesStalemate();
    }
    private void Init()
    {
        CheckState = CheckState.None;
        CastlingState[(int)Color.White].CastlingRight = CastlingRight.Both;
        CastlingState[(int)Color.Black].CastlingRight = CastlingRight.Both;
        Color squareColor = Color.Black;
        for (Column col = Column.a; col <= Column.h; col++)
        {
            for (int row = 0; row <= 7; row++)
            {
                _squares[(int) col, row] = new Square(col, row, squareColor);
                if (row != 7)
                {
                    squareColor = squareColor == Color.White ? Color.Black : Color.White;
                }
            }
        }
    }
    private void AddPiece(Column column, int row, Piece piece)
    {
        if (piece.Color == Color.White)
        {
            _whitePieces.Add(piece);
        }
        else
        {
            _blackPieces.Add(piece);
        }
        _squares[(int)column, row].SetPiece(piece);
    }
    private void AddPieces()
    {
        // White
        for (Column col = Column.a; col <= Column.h; col++)
        {
            WhitePawns.Add(new Pawn(this, Color.White));
            AddPiece(col, 1, WhitePawns[(int) col]);            
        }        
        AddPiece(Column.a, 0, new Rook(this, Color.White, RookSide.QueenSide));
        AddPiece(Column.b, 0, new Knight(this, Color.White));
        AddPiece(Column.c, 0, new Bishop(this, Color.White));
        AddPiece(Column.d, 0, new Queen(this, Color.White));
        AddPiece(Column.e, 0, WhiteKing);
        AddPiece(Column.f, 0, new Bishop(this, Color.White));
        AddPiece(Column.g, 0, new Knight(this, Color.White));
        AddPiece(Column.h, 0, new Rook(this, Color.White, RookSide.KingSide));

        // Black
        for (Column col = Column.a; col <= Column.h; col++)
        {
            BlackPawns.Add(new Pawn(this, Color.Black));
            _squares[(int)col, 6].SetPiece(BlackPawns[(int)col]);
        }
        AddPiece(Column.a, 7, new Rook(this, Color.Black, RookSide.QueenSide));
        AddPiece(Column.b, 7, new Knight(this, Color.Black));
        AddPiece(Column.c, 7, new Bishop(this, Color.Black));
        AddPiece(Column.d, 7, new Queen(this, Color.Black));
        AddPiece(Column.e, 7, BlackKing);
        AddPiece(Column.f, 7, new Bishop(this, Color.Black));
        AddPiece(Column.g, 7, new Knight(this, Color.Black));
        AddPiece(Column.h, 7, new Rook(this, Color.Black, RookSide.KingSide));
    }
    private void AddPiecesStalemate()
    {
        CastlingState[(int)Color.White].CastlingRight = CastlingRight.None;
        CastlingState[(int)Color.Black].CastlingRight = CastlingRight.None;

        AddPiece(Column.h, 7, BlackKing);
        AddPiece(Column.f, 6, WhiteKing);
        AddPiece(Column.g, 5, new Queen(this, Color.White));
    }
    public Square GetSquare(Column col, int row)
    {
        if (IsValidSquare(col, row))
            return _squares[(int)col, row];
        else
            return null;
    }
    public bool IsValidMove(Square from, Square to)
    {
        return true;
    }
    // Valid moves, regardless of checks
    private bool DetermineValidMoves(Color color)
    {
        bool hasValidMoves = false;
        if (color == Color.White)
        {
            foreach (Piece piece in _whitePieces)
            {
                piece.DetermineValidMoves();
                hasValidMoves |= (piece.ValidMoves != 0);
            }
        }
        else
        {
            foreach (Piece piece in _blackPieces)
            {
                piece.DetermineValidMoves();
                hasValidMoves |= (piece.ValidMoves != 0);
            }
        }
        return hasValidMoves;
    }
    public MoveResultStruct Move(Color color, Square fromSquare, Square toSquare)
    {
        MoveResultStruct result = new MoveResultStruct();
        result.MoveResult = MoveResult.Invalid;
        Square? oldEnpassantSquare = EnPassantSquare;

        if ((fromSquare != toSquare) && ((fromSquare.Piece != null) && fromSquare.Piece.Color == color))
        {
            result = fromSquare.Piece.Move(toSquare);

            if (result.MoveResult != MoveResult.Invalid)
            {
                result.Piece = fromSquare.Piece;
                result.From = fromSquare;
                result.To = toSquare;
                result.MoveResult = HandleSelfCheck(result); // make move if not selfcheck
            }
            if (result.MoveResult != MoveResult.Invalid)
            {
                if (result.MoveResult == MoveResult.Promotion)
                {
                    HandlePromotion(color, toSquare);
                }

                // reset enpassant square (unless there is a new one)
                if (oldEnpassantSquare != null && oldEnpassantSquare == EnPassantSquare)
                {
                    EnPassantSquare = null;
                }
                if (DetermineCheck(color == Color.White ? Color.Black : Color.White, setCheckState: true)) // determine for opposite color               
                {
                    if (DetermineCheckmate())
                    {
                        result.MoveResult = MoveResult.Checkmate;
                    }
                    else
                    {
                        result.MoveResult = MoveResult.Check;
                    }
                }
            }
        }
        return result;
    }
    public MoveResultStruct Move(Color color)
    {
        MoveResultStruct result = new MoveResultStruct();
        result.MoveResult = MoveResult.Invalid;

        bool hasValidMoves = DetermineValidMoves(color); // valid moves regardless of check
        if (IsStaleMate(color))
        {
            result.MoveResult = MoveResult.Stalemate;
            return result;
        }

        Square fromSquare = color == Color.White ? _squares[(int)Column.e, 1] : _squares[(int)Column.e, 6];
        Square toSquare = fromSquare;
        Square? oldEnpassantSquare = EnPassantSquare;
        do
        {            
            do
            {
                fromSquare.Highlight(false);
                fromSquare = SelectSquare(fromSquare);
            }
            while ((fromSquare.Piece == null) || fromSquare.Piece.Color != color);

            toSquare = fromSquare;            
            toSquare = SelectSquare(toSquare);

            result = Move(color, fromSquare, toSquare);

            fromSquare.Highlight(false);
            toSquare.Highlight(false);
        }
        while (result.MoveResult == MoveResult.Invalid);

        return result;
    }
    private bool TryMove(Piece piece, Square toSquare)
    {
        bool result = false;

        if (piece.Square != toSquare)
        {
            MoveResultStruct moveResult = piece.Move(toSquare);            
            if (moveResult.MoveResult != MoveResult.Invalid)
            {
                moveResult.Piece = piece;
                moveResult.From = piece.Square!;
                moveResult.To = toSquare;
                result = HandleSelfCheck(moveResult) != MoveResult.Invalid;
                UndoMove(moveResult);
            }
        }
        return result;
    }
    
    // True if there is at least 1 valid move
    private bool TryValidMoves(Piece piece)
    {
        bool result = false;
        UInt64 validMoves = piece.ValidMoves;
        int index = 0;
        Square? square = null;
        while (validMoves != 0 && !result)
        {
            index = BitBoard.bitScanForward(validMoves);
            square = GetSquare(index);
            if ( square != null)
            {
                result = TryMove(piece, square);
            }            
            validMoves &= validMoves-1; // reset bit
        }
        return result;
    }
    private bool DetermineCheck(Color color, bool setCheckState = false)
    {
        Square kingSquare = color == Color.White? WhiteKing.Square! : BlackKing.Square!;
        CheckingPieces.Clear();
        bool result = (IsSquareInCheck(kingSquare, color, CheckingPieces));
        if (setCheckState)
        {
            if (result)
            {
                CheckState = color switch
                {
                    Color.White => CheckState.White,
                    Color.Black => CheckState.Black,
                    _ => CheckState.None
                };
            }
            else
            {
                CheckState = CheckState.None;
            }
        }
        return result;
    }
    public bool DetermineCheckmate() 
    {
        bool result = false;        
        if (CheckState != CheckState.None)
        {
            bool hasValidMove = false;
            Piece? theKing = null;            
            switch (CheckState)
            {
                case CheckState.White:                                        
                    theKing = WhiteKing;
                    break;
                case CheckState.Black:
                    theKing = BlackKing;
                    break;
            }
            //DetermineValidMoves(theKing!.Color);
            theKing!.DetermineValidMoves();
            hasValidMove = theKing!.ValidMoves != 0;
            hasValidMove &= TryValidMoves(theKing!);

            if (!hasValidMove && CheckingPieces.Count == 1) // in case of double check, the king MUST move
            {
                Color oppositeColor = theKing!.Color == Color.White ? Color.Black : Color.White;
                Piece checkingPiece = CheckingPieces[0];                
                List<Piece> attackingPieces = new List<Piece>();
                
                // can be captured?
                if (IsSquareInCheck(checkingPiece.Square!, oppositeColor, attackingPieces))
                {
                    for (int i = 0; i < attackingPieces.Count && !hasValidMove; i++)
                    {
                        hasValidMove = TryMove(attackingPieces[i], checkingPiece.Square!);
                    }
                }
                
                if (!hasValidMove && !(checkingPiece.Type == PieceType.Knight)) // a knight MUST be captured if there are no king moves
                {
                    UInt64 squaresBetween = GetSquaresBetween(theKing!.Square!, checkingPiece!.Square!, false);
                    Square? currentSquare = null;
                    int index = 0;
                    
                    // can be blocked?
                    while (squaresBetween != 0 && !hasValidMove)
                    {
                        index = BitBoard.bitScanForward(squaresBetween);
                        currentSquare = GetSquare(index);
                        if (currentSquare != null)
                        {
                            if (IsSquareInCheck(currentSquare, oppositeColor, attackingPieces, capturesOnly:false))
                            {
                                for (int i = 0; i < attackingPieces.Count && !hasValidMove; i++)
                                {
                                    hasValidMove = TryMove(attackingPieces[i], currentSquare);
                                }
                            }
                        }
                        squaresBetween &= squaresBetween - 1; // reset
                    }
                }
            }
            result = !hasValidMove;
        }
        return result;
    }
    private MoveResult HandleSelfCheck(MoveResultStruct moveResult)
    {
        MoveResult result = moveResult.MoveResult; // if OK, return temp result, otherwise Invalid

        MakeMove(moveResult);
        if (DetermineCheck(moveResult.Piece.Color)) // (still) in check? invalid move        
        {
            // in check after move: invalid
            result = MoveResult.Invalid;
            UndoMove(moveResult);
        }
        return result;
    }
    private bool TryUntilValidMove(Color color)
    {
        bool result = false;
        result |= color == Color.White ? TryValidMoves(WhiteKing) : TryValidMoves(BlackKing);
        if (!result)
        {
            List<Piece> pieces = color == Color.White? _whitePieces : _blackPieces;
            for (int i = 0; i < pieces.Count && !result; i++)
            {
                result |= TryValidMoves(pieces[i]);
            }
        }
        return result;
    }
    private bool IsStaleMate(Color color)
    {
        bool result = false;
        if (CheckState == CheckState.None)
        {
            result = !TryUntilValidMove(color);
        }
        return result;
    }
    private void MakeMove(MoveResultStruct moveResult)
    {
        moveResult.From.RemovePiece();
        if (moveResult.CapturedPiece != null)
        {
            moveResult.CapturedPiece.Square!.RemovePiece();
        }
        moveResult.To.SetPiece(moveResult.Piece);
        
        if (moveResult.MoveResult == MoveResult.CastleShort || moveResult.MoveResult == MoveResult.CastleLong)
        {
            MakeCastlingMove(moveResult);
        }
        UpdateMoveCounts(moveResult);
    }
    private void MakeCastlingMove(MoveResultStruct moveResult)
    {
        Square? rookSquare = null;
        Square? newRookSquare = null;
        switch (moveResult.MoveResult) 
        {
            case MoveResult.CastleShort:
                if (moveResult.Piece.Color == Color.White)
                {
                    rookSquare = GetSquare(Column.h, 0);
                    newRookSquare = GetSquare(Column.f, 0);
                }    
                else
                {
                    rookSquare = GetSquare(Column.h, 7);
                    newRookSquare = GetSquare(Column.f, 7);
                }                
                break;
            case MoveResult.CastleLong:
                if (moveResult.Piece.Color == Color.White)
                {
                    rookSquare = GetSquare(Column.a, 0);
                    newRookSquare = GetSquare(Column.d, 0);
                }
                else
                {
                    rookSquare = GetSquare(Column.a, 7);
                    newRookSquare = GetSquare(Column.d, 7);
                }
                break;
        }
        if (rookSquare != null && rookSquare.Piece != null)
        {
            newRookSquare!.SetPiece(rookSquare.Piece);
            rookSquare.RemovePiece();
            CastlingState[(int)moveResult.Piece.Color].CastlingRight = CastlingRight.None;
        }
    }
    private void UpdateCastlingRight(Color color)
    {
        (int NrOfKingMoves, int NrOfShortCastleMoves, int NrOfLongCastleMoves) value = (CastlingState[(int)color].NrOfKingMoves, CastlingState[(int)color].NrOfShortCastleMoves, CastlingState[(int)color].NrOfLongCastleMoves);
        CastlingState[(int)color].CastlingRight = value switch
        {
            (int nrKing, int nrShort, int nrLong) when nrKing != 0 => CastlingRight.None,
            (int nrKing, int nrShort, int nrLong) when (nrShort == 0 && nrLong == 0) => CastlingRight.Both,
            (int nrKing, int nrShort, int nrLong) when nrShort == 0 => CastlingRight.Short,
            (int nrKing, int nrShort, int nrLong) when nrLong == 0 => CastlingRight.Long,
            _ => CastlingRight.None
        };
    }
    private void UpdateMoveCounts(MoveResultStruct moveResult, bool isUndo = false)
    {
        Color color = moveResult.Piece.Color;
        Color oppositeColor = color == Color.Black ? Color.White : Color.Black;
        int incrementCount = isUndo ? -1 : 1;
        bool isRookCaptured = false;
        
        if (isUndo || CastlingState[(int)color].CastlingRight != CastlingRight.None)
        {
            if (moveResult.Piece.Type == PieceType.King)
            {
                CastlingState[(int)color].NrOfKingMoves += incrementCount;
            }
            else if (moveResult.Piece.Type == PieceType.Rook)
            {
                Rook rook = (Rook)moveResult.Piece;                    
                switch (rook.RookSide)
                {
                    case RookSide.KingSide:
                        CastlingState[(int)color].NrOfShortCastleMoves += incrementCount;
                        break;
                    case RookSide.QueenSide:
                        CastlingState[(int)color].NrOfLongCastleMoves += incrementCount; 
                        break;
                }                    
            }
            if (moveResult.CapturedPiece != null && moveResult.CapturedPiece.Type == PieceType.Rook)
            {
                // check whether one of the castling rooks (of opposite color) was captured.
                // Just increment the move count to prevent "castling".
                Rook rook = (Rook)moveResult.CapturedPiece;
                switch (rook.RookSide)
                { 
                    case RookSide.KingSide:
                        CastlingState[(int)oppositeColor].NrOfShortCastleMoves += incrementCount;
                        isRookCaptured = true;
                        break;
                    case RookSide.QueenSide:
                        CastlingState[(int)oppositeColor].NrOfLongCastleMoves += incrementCount;
                        isRookCaptured = true;
                        break;
                }
                if (isRookCaptured)
                {
                    UpdateCastlingRight(oppositeColor);
                }
            }
            UpdateCastlingRight(color);
        }
    }
    public CastlingRight GetCastlingRight(Color color)
    {
        return CastlingState[(int)color].CastlingRight;
    }
    private void UndoMove(MoveResultStruct moveResult)
    {
        moveResult.From.SetPiece(moveResult.Piece);
        moveResult.To.RemovePiece();
        if (moveResult.CapturedPiece != null)
        {
            moveResult.CapturedPiece.Square!.SetPiece(moveResult.CapturedPiece);
        }        
    }
    private void HandlePromotion(Color color, Square promotionSquare)
    {
        promotionSquare.RemovePiece();
        Console.Write($"{color}, select a promotion (q, r, b, k):");
        String? promo = Console.ReadLine();        
        Piece? promoPiece = null;

        if (promo == null)
            promo = "q";
        switch (promo)
        {
            case "q":
            default: // default to queen
                promoPiece = new Queen(this, color);                
                break;
            case "r":
                promoPiece = new Rook(this, color, RookSide.None);                
                break;
            case "b":
                promoPiece = new Bishop(this, color);
                break;
            case "k":
                promoPiece  = new Knight(this, color);
                break;
        }
        promotionSquare.SetPiece(promoPiece);
    }
    public void RemovePiece(Square square)
    {
        square.RemovePiece();
        // TODO: notation
    }
    private Square SelectSquare(Square fromSquare)
    {
        Square? highlightedSquare = fromSquare.IsHighlighted? fromSquare : null;
        fromSquare.Highlight(true);
        Column selectedColumn = fromSquare.Column;
        int selectedRow = fromSquare.Row;        

        Draw();
        ConsoleKeyInfo keyInfo = Console.ReadKey();
        while (keyInfo.Key != ConsoleKey.Spacebar && keyInfo.Key != ConsoleKey.Enter)
        {            
            switch(keyInfo.Key)
            {
                case ConsoleKey.DownArrow:
                    if (IsValidSquare(fromSquare.Column, selectedRow - 1))
                    {
                        selectedRow--;                                                
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (IsValidSquare(fromSquare.Column, selectedRow + 1))
                    {
                        selectedRow++;
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (IsValidSquare(selectedColumn - 1, fromSquare.Row))
                    {
                        selectedColumn--;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (IsValidSquare(selectedColumn + 1, fromSquare.Row))
                    {
                        selectedColumn++;
                    }
                    break;
            }
            if (highlightedSquare != fromSquare)
            {
                fromSquare.Highlight(false);
            }
            fromSquare = GetSquare(selectedColumn, selectedRow);
            fromSquare.Highlight(true);
            Draw();

            keyInfo = Console.ReadKey();
        }
        return fromSquare;
    }
    public static bool IsValidSquare(Column column, int row)
    {
        return (column >= Column.a && column <= Column.h) && (row >= 0 && row <= 7);
    }
    public bool HasPieceBetween(Square from, Square to)
    {
        bool result = false;
        Square currentSquare = from;
        // Row, Column or diagonal?
        if (from.Column == to.Column) // check row
        {
            int rowDirection = from.Row < to.Row ? 1 : -1;
            int currentRow = from.Row + rowDirection;
            while (!result && currentRow != to.Row)
            {
                currentSquare = GetSquare(from.Column, currentRow);
                result = (result || currentSquare.Piece != null);
                currentRow += rowDirection;
            }
        }
        else if (from.Row == to.Row) // check column
        {
            int columnDirection = from.Column < to.Column ? 1 : -1;
            Column currentColumn = from.Column + columnDirection;
            while (!result && currentColumn != to.Column)
            {
                currentSquare = GetSquare(currentColumn, from.Row);
                result = (result || currentSquare.Piece != null);
                currentColumn += columnDirection;
            }
        }
        else if (Math.Abs(from.Column - to.Column) == Math.Abs(from.Row - to.Row)) // check diagonal
        {
            int rowDirection = from.Row < to.Row ? 1 : -1;
            int currentRow = from.Row + rowDirection;
            int columnDirection = from.Column < to.Column ? 1 : -1;
            Column currentColumn = from.Column + columnDirection;
            while (!result && currentRow != to.Row)
            {
                currentSquare = GetSquare(currentColumn, currentRow);
                result = (result || currentSquare.Piece != null);
                currentRow += rowDirection;
                currentColumn += columnDirection;
            }
        }
        return result;
    }
    // returns a bitboard
    public UInt64 GetSquaresBetween(Square from, Square to, bool upToIncluding)
    {
        UInt64 result = 0;

        if (Square.IsVertical(from, to) ||
            Square.IsHorizontal(from, to) ||
            Square.IsDiagonal(from, to))
        {
            int rowDirection = from.Row == to.Row ? 0 : from.Row < to.Row ? 1 : -1;
            int columnDirection = from.Column == to.Column ? 0 : from.Column < to.Column ? 1 : -1;
            int currentRow = from.Row;
            Column currentColumn = from.Column;
            do
            {
                currentColumn += columnDirection;
                currentRow += rowDirection;
                if (upToIncluding || currentRow != to.Row || currentColumn != to.Column)
                {
                    result = BitBoard.SetBit(result, currentRow, currentColumn);
                }
            }
            while (currentRow != to.Row || currentColumn != to.Column);
        }
        return result;
    }
    // color: the color that is in check. The opposite color is the attacker.
    public bool IsSquareInCheck(Square square, Color color, List<Piece>? attackingPieces, bool capturesOnly = true)
    {
        bool result = false;
        Square curSquare;
        Column col = square.Column;
        int row = square.Row;
        Color oppositeColor = color == Color.White ? Color.Black : Color.White;
        bool done = false;

        for (int i = 0; i < CheckDirections.GetLength(0); i++)
        {            
            done = false;
            col = square.Column + CheckDirections[i, 0];
            row = square.Row + CheckDirections[i, 1];
            while (IsValidSquare(col, row) && !done)
            {
                curSquare = GetSquare(col, row); // does curSquare have an attacker?
                if (curSquare.Piece != null)
                {
                    done = true;
                    if (curSquare.Piece.Color == oppositeColor)
                    {
                        bool isAttackingPiece = false;
                        switch(curSquare.Piece.Type)
                        {
                            case PieceType.Pawn:
                                int pawnOffset = color == Color.White ? 1 : -1;
                                
                                if (!capturesOnly) // include forward steps too?
                                {
                                    isAttackingPiece = BitBoard.IsBitSet(curSquare.Piece.ValidMoves, square.Row, square.Column);
                                }
                                else
                                {
                                    isAttackingPiece = (Square.IsDiagonal(square, curSquare) &&
                                                       (square.Row + pawnOffset == curSquare.Row));
                                }
                                break;
                            case PieceType.Rook:
                                isAttackingPiece = (Square.IsHorizontal(square, curSquare)) ||
                                                   (Square.IsVertical(square, curSquare));
                                break;
                            case PieceType.Bishop:
                                isAttackingPiece = (Square.IsDiagonal(square, curSquare));
                                break;
                            case PieceType.Queen:
                                isAttackingPiece = (Square.IsHorizontal(square, curSquare)) ||
                                                   (Square.IsVertical(square, curSquare)) ||
                                                   (Square.IsDiagonal(square, curSquare));
                                break;
                            case PieceType.King:
                                int nrOfColumns = Math.Abs(square.Column - curSquare.Column);
                                int nrOfRows = Math.Abs(square.Row - curSquare.Row);
                                isAttackingPiece = (nrOfColumns > 0 || nrOfRows > 0) && nrOfColumns <= 1 && nrOfRows <= 1;                                 
                                break;

                        }
                        if (isAttackingPiece)
                        {
                            attackingPieces?.Add(curSquare.Piece);
                            result = true;
                        }
                    }
                }
                col += CheckDirections[i, 0];
                row += CheckDirections[i, 1];
            }
        }
        // Knight check
        for (int i = 0; i < CheckKnightDirections.GetLength(0); i++)
        {
            col = square.Column + CheckKnightDirections[i, 0];
            row = square.Row + CheckKnightDirections[i, 1];
            if (IsValidSquare(col, row))
            {
                curSquare = GetSquare(col, row);
                if (curSquare.Piece != null)
                {
                    if (curSquare.Piece.Color == oppositeColor && curSquare.Piece.Type == PieceType.Knight)
                    {
                        result = true;
                        attackingPieces?.Add(curSquare.Piece);
                    }
                }
            }
        }
        return result;
    }
    public bool IsSquareInCheck(Square square, Color color)
    {
        return IsSquareInCheck(square, color, null, true);
    }
    private Square? GetSquare(int index)
    {
        Square? result = null;
        if (index >=0 && index <= 63)
        {
            Column col = (Column) (index % 8);
            int row = index / 8;
            result = GetSquare(col, row);
        }
        return result;
    }
    public void Draw()
    {
        Console.Clear();
        DrawBorder(); // topleft
        for (Column col = Column.a; col <= Column.h; col++)
        {
            DrawColumn(col); // left border
        }
        DrawBorder(); // topright
        Console.WriteLine();

        for (int row = 7; row >= 0; row--)
        {
            DrawBorder(); // leftt border
            for (Column col = Column.a; col <= Column.h; col++)
            {
                _squares[(int)col, row].DrawEmpty();
            }
            DrawBorder(); // right border
            Console.WriteLine();

            DrawRow(row); // left border
            for (Column col = Column.a; col <= Column.h; col++)
            {
                _squares[(int)col, row].Draw();
            }
            DrawRow(row); // right border
            Console.WriteLine();
            DrawBorder();
            for (Column col = Column.a; col <= Column.h; col++)
            {
                _squares[(int)col, row].DrawEmpty();
            }
            DrawBorder(); // right border
            Console.WriteLine();
        }
        DrawBorder(); // bottomleft
        for (Column col = Column.a; col <= Column.h; col++)
        {
            DrawColumn(col); // border
        }
        DrawBorder(); // bottomright
        Console.WriteLine();

        DrawCheck();

        _game.DrawStatus();
    }
    private void DrawCheck()
    {
        if (CheckState != CheckState.None)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"=== {CheckState} is in check! ===");
        }
    }
    private static void DrawRow(int row)
    {
        Console.BackgroundColor = ConsoleColor.Cyan;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write($"{row + 1}");
    }
    private static void DrawColumn(Column column)
    {
        Console.BackgroundColor = ConsoleColor.Cyan;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write($"   {column}   ");
    }
    private static void DrawBorder()
    {
        Console.BackgroundColor = ConsoleColor.Cyan;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write($" ");
    }
}
public interface IBoard
{
    Square? EnPassantSquare { get; set; }
    public bool IsValidMove(Square from, Square to);
    public Square GetSquare(Column col, int row);
    public void RemovePiece(Square square);
    public bool HasPieceBetween(Square from, Square to);
    public CastlingRight GetCastlingRight(Color color);
    public CheckState GetCheckState();
    public bool IsSquareInCheck(Square square, Color color);
}
