// See https://aka.ms/new-console-template for more information

public class Square
{
    private readonly ConsoleColor _backgroundColor;
    private readonly ConsoleColor _foregroundColor;
    private readonly ConsoleColor _highlightColor = ConsoleColor.Cyan;
    public bool IsHighlighted { get; private set; }
    public Column Column { get; init; }
    public Color Color { get; init; }
    public int Row { get; init; }
    public Piece? Piece { get; private set; }    
    public void SetPiece(Piece piece)
    {
        Piece = piece;
        piece.Square = this;
    }
    public bool HasPiece()
    {
        return (Piece != null);
    }
    public void RemovePiece()
    {
        Piece = null;
    }
    public override String ToString()
    {
        return $"{Column}{Row + 1}";
    }
    public Square(Column column, int row, Color color)
    {
        Column = column;
        Row = row;
        Color = color;
        Piece = null;
        IsHighlighted = false;
        switch (color)
        {
            case Color.Black:
                _foregroundColor = ConsoleColor.White;
                _backgroundColor = ConsoleColor.Black;
                break;
            case Color.White:
                _foregroundColor = ConsoleColor.Black;
                _backgroundColor = ConsoleColor.White;
                break;
        }
    }
    public void Draw()
    {
        Console.ForegroundColor = _foregroundColor;
        if (IsHighlighted)
        {
            Console.BackgroundColor = _highlightColor;
        }
        else
        {
            Console.BackgroundColor = _backgroundColor;
        }
        if (HasPiece())
        {
            Console.Write("   ");
            Piece?.Draw();
            Console.Write("   ");
        }
        else
        {
            DrawEmpty();
        }
    }
    public void DrawEmpty()
    {        
        Console.ForegroundColor = _foregroundColor;
        if (IsHighlighted)
        {
            Console.BackgroundColor = _highlightColor;
        }
        else
        {
            Console.BackgroundColor = _backgroundColor;
        }
        Console.Write("       ");
    }
    public void Highlight(bool isOn)
    {
        IsHighlighted = isOn;
    }
    public static bool IsVertical(Square from, Square to)
    {
        return (from.Column == to.Column);
    }
    public static bool IsHorizontal(Square from, Square to)
    {
        return (from.Row == to.Row);
    }
    public static bool IsDiagonal(Square from, Square to)
    {
        return (Math.Abs(from.Column - to.Column) == Math.Abs(from.Row - to.Row));
    }
}

