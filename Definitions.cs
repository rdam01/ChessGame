using System;

public record struct MoveResultStruct
{
    public MoveResult MoveResult;
    public Piece Piece;
    public Piece? CapturedPiece;
    public Square From, To;
}

public record struct CastlingState
{
    public CastlingRight CastlingRight;
    public int NrOfKingMoves, NrOfShortCastleMoves, NrOfLongCastleMoves;
}

public enum Column { a = 0, b, c, d, e, f, g, h }
public enum Color { White, Black }
public enum MoveResult { Invalid, OK, Capture, Check, Checkmate, Stalemate, Promotion, CastleShort, CastleLong }
public enum GameResult { Undecided, WhiteWins, BlackWins, WhiteStalemate, BlackStalemate, Draw }
public enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King }
public enum CheckState { None, White, Black }
public enum CastlingRight { None, Both, Short, Long }
public enum RookSide { None, KingSide, QueenSide }
