using System.Numerics;
using Antlr4.Runtime;

namespace MyExcelMAUIApp;

public class ExpressionEvaluator
{
    public bool Evaluate(string expression, Spreadsheet spreadsheet)
    {
        var lexer = new LabLexer(new AntlrInputStream(expression));
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new SyntaxErrorListener());

        var parser = new LabParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new SyntaxErrorListener());

        var tree = parser.compileUnit();
        var visitor = new CalcVisitor(spreadsheet);
        return !visitor.Visit(tree).IsZero;
    }
}