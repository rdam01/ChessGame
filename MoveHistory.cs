// See https://aka.ms/new-console-template for more information

public class MoveHistory
{
    private readonly List<MoveResultStruct> _moves;
    private readonly List<string> _moveNotations;
    public String Notation { get; private set; }
    public MoveHistory()
    {
        _moves = new List<MoveResultStruct>();
        _moveNotations = new List<string>();
        Notation = "";
    }
    private static String GetCaptureNotation(MoveResultStruct move)
    {
        String result = "";
        return result;
    }
    private String GetNotation(MoveResultStruct move)
    {
        String result = "";
        // move nr
        if (_moves.Count % 2 == 1)
        {
            int moveNr = _moves.Count / 2 + 1;
            result = $"{moveNr}. ";
        }
        else
        {
            result = "..";
        }
        // piece + move
        String pieceName =  move.Piece.ToString().ToUpper();
        switch (move.Piece.Type)
        {
            case PieceType.Pawn:
                if (move.CapturedPiece == null)
                {
                    result = $"{result}{move.To}";
                }
                else
                {
                    result = $"{result}{move.From.Column}x{move.To}";
                }
                // promotion?    
                if (move.To.Piece!.Type != PieceType.Pawn)
                {
                    result = $"{result}={move.To.Piece}";
                }
                break;
            case PieceType.King:
                if (move.MoveResult == MoveResult.CastleShort)
                {
                    result = $"{result}O-O";
                }
                else if (move.MoveResult == MoveResult.CastleLong)
                {
                    result = $"{result}O-O-O";
                }
                else if (move.CapturedPiece == null)
                {
                    result = $"{result}{move.Piece}{move.To}";
                }
                else
                {
                    result = $"{result}{move.Piece}x{move.To}";
                }
                break;
            default:
                if (move.CapturedPiece == null)
                {
                    result = $"{result}{move.Piece}{move.To}";
                }
                else
                {
                    result = $"{result}{move.Piece}{move.From.Column}x{move.To}";
                }
                break;
        }
        switch (move.MoveResult)
        {
            case MoveResult.Check:
                result = $"{result}+";
                break;
            case MoveResult.Checkmate:
                result = $"{result}#";
                break;
        }
        result = $"{result} ";

        return result;
    }
    public void AddMove(MoveResultStruct move)
    {
        _moves.Add(move);
        _moveNotations.Add(GetNotation(move));
        Notation = Notation + _moveNotations[_moveNotations.Count - 1];
    }
}

