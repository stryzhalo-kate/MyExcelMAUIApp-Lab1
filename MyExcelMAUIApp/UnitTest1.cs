using MyExcelMAUIApp;
using Xunit;

public class SpreadsheetTests
{
    
    [Theory]
    [InlineData("A1 >= B1", true)]
    [InlineData("A1 <= B1", false)]
    [InlineData("A1 mod B1 <> 2", false)]
    [InlineData("mmax(A1, B1, 10) >= 10", true)]
    [InlineData("not (A1 <= B1)", true)]
    public void Calculates_logical_expressions(string expression, bool expected)
    {
        var sheet = new Spreadsheet();
        sheet.SetExpression("A1", "5");
        sheet.SetExpression("B1", "3");
        sheet.SetExpression("C1", expression);
        sheet.CalculateAll();

        var cell = sheet.GetCell("C1");
        Assert.Equal("", cell.Error);
        Assert.Equal(expected, cell.Value);
    }

    [Theory]
    [InlineData("5 / 0 <= 1", "Ділення на нуль")]
    [InlineData("5 + 3", "Синтаксична помилка")]
    [InlineData("Z9 >= 1", "порожня")]
    public void Reports_errors(string expression, string expectedPart)
    {
        var sheet = new Spreadsheet();
        sheet.SetExpression("C1", expression);
        sheet.CalculateAll();

        Assert.Contains(expectedPart, sheet.GetCell("C1").Error);
    }

    
    [Fact]
    public void Detects_cyclic_reference_in_both_cells()
    {
        var sheet = new Spreadsheet();
        sheet.SetExpression("A1", "A2 >= 0");
        sheet.SetExpression("A2", "A1 >= 0");
        sheet.CalculateAll();

        Assert.Contains("Циклічне", sheet.GetCell("A1").Error);
        Assert.Contains("Циклічне", sheet.GetCell("A2").Error);
    }
}