namespace MyExcelMAUIApp;

public class ExpressionException : Exception
{
    public int Position { get; }
    public ExpressionException(string message, int position = 0) : base(message)
    {
        Position = position;
    }
}
