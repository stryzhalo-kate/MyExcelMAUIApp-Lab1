using Antlr4.Runtime;

namespace MyExcelMAUIApp;

public class SyntaxErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
 
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
        int line, int charPositionInLine, string msg, RecognitionException e)
    {
        var token = offendingSymbol?.Text == "<EOF>"
            ? "кінець виразу"
            : $"'{offendingSymbol?.Text}'";
        throw new ExpressionException(
            $"Синтаксична помилка (позиція {charPositionInLine + 1}): неочікуваний {token}",
            charPositionInLine);
    }

    
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol,
        int line, int charPositionInLine, string msg, RecognitionException e)
    {
        throw new ExpressionException(
            $"Недопустимий символ (позиція {charPositionInLine + 1})",
            charPositionInLine);
    }
}