using System.Numerics;

namespace MyExcelMAUIApp;

public class CalcVisitor : LabBaseVisitor<BigInteger>
{
    private readonly Spreadsheet spreadsheet;

    public CalcVisitor(Spreadsheet spreadsheet)
    {
        this.spreadsheet = spreadsheet;
    }

    public override BigInteger VisitCompileUnit(LabParser.CompileUnitContext ctx)
    => Visit(ctx.logical());

    public override BigInteger VisitNotExpr(LabParser.NotExprContext ctx)
        => Visit(ctx.logical()).IsZero ? BigInteger.One : BigInteger.Zero;

    public override BigInteger VisitParenLogicalExpr(LabParser.ParenLogicalExprContext ctx)
        => Visit(ctx.logical());

    public override BigInteger VisitCompareExpr(LabParser.CompareExprContext ctx)
    {
        var l = Visit(ctx.arith(0));
        var r = Visit(ctx.arith(1));
        bool result = ctx.op.Type switch
        {
            LabParser.LE => l <= r,
            LabParser.GE => l >= r,
            _ => l != r
        };
        return result ? BigInteger.One : BigInteger.Zero;
    }

    public override BigInteger VisitParenExpr(LabParser.ParenExprContext ctx)
        => Visit(ctx.arith());

    public override BigInteger VisitMulDivExpr(LabParser.MulDivExprContext ctx)
    {
        var l = Visit(ctx.arith(0));
        var r = Visit(ctx.arith(1));
        if (ctx.op.Type == LabParser.MULTIPLY) return l * r;
        if (r.IsZero) throw new ExpressionException("Ділення на нуль", ctx.op.StartIndex);
        return ctx.op.Type == LabParser.MOD ? BigInteger.Remainder(l, r) : BigInteger.Divide(l, r);
    }

    public override BigInteger VisitAddSubExpr(LabParser.AddSubExprContext ctx)
    {
        var l = Visit(ctx.arith(0));
        var r = Visit(ctx.arith(1));
        return ctx.op.Type == LabParser.ADD ? l + r : l - r;
    }

    public override BigInteger VisitMinMaxExpr(LabParser.MinMaxExprContext ctx)
    {
        var values = ctx.arglist().arith().Select(a => Visit(a)).ToList();
        return ctx.op.Type == LabParser.MMAX
            ? values.Aggregate((a, b) => BigInteger.Max(a, b))
            : values.Aggregate((a, b) => BigInteger.Min(a, b));
    }

    public override BigInteger VisitNumberExpr(LabParser.NumberExprContext ctx)
        => BigInteger.Parse(ctx.NUMBER().GetText());

    public override BigInteger VisitCellRefExpr(LabParser.CellRefExprContext ctx)
        => spreadsheet.GetNumericValue(ctx.IDENTIFIER().GetText());
}