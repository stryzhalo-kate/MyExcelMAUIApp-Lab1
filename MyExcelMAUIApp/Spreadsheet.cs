using System.Numerics;
using System.Text.RegularExpressions;

namespace MyExcelMAUIApp;

public class Spreadsheet
{
    private readonly Dictionary<string, Cell> cells = new();
    private readonly HashSet<string> evaluating = new();
    private readonly ExpressionEvaluator evaluator = new();
    private int rowCount = 50;
    private int columnCount = 20;

    public Cell GetCell(string name)
    {
        if (!cells.TryGetValue(name, out var cell))
        {
            cell = new Cell { Name = name };
            cells[name] = cell;
        }
        return cell;
    }

    public void SetExpression(string name, string text)
    {
        var cell = GetCell(name);
        cell.Expression = text;
        cell.Value = null;
        cell.Error = "";
    }

    public void CalculateAll()
    {
        foreach (var cell in cells.Values)
        {
            cell.Value = null;
            cell.Error = "";
        }
        foreach (var cell in cells.Values.ToList())
            EvaluateCell(cell);
    }

    public BigInteger GetNumericValue(string name)
    {
        var cell = GetCell(name);
        var text = cell.Expression.Trim();
        if (text == "") throw new ExpressionException($"Клітинка {name} порожня");
        if (BigInteger.TryParse(text, out var number)) return number;

        EvaluateCell(cell);
        if (cell.Error != "") throw new ExpressionException($"Помилка в клітинці {name}");
        return cell.Value!.Value ? BigInteger.One : BigInteger.Zero;
    }

    private void EvaluateCell(Cell cell)
    {
        var text = cell.Expression.Trim();
        if (text == "" || BigInteger.TryParse(text, out _)) return;
        if (cell.Value.HasValue || cell.Error != "") return;

        if (!evaluating.Add(cell.Name))
            throw new ExpressionException($"Циклічне посилання на {cell.Name}");
        try
        {
            cell.Value = evaluator.Evaluate(text, this);
        }
        catch (ExpressionException ex)
        {
            cell.Error = ex.Message;
        }
        finally
        {
            evaluating.Remove(cell.Name);
        }
    }

    public void AddRow() { rowCount++; }
    public void AddColumn() { columnCount++; }

    public void DeleteRow()
    {
        if (rowCount == 0) return;
        foreach (var key in cells.Keys.Where(k => Split(k).row == rowCount).ToList())
            cells.Remove(key);
        rowCount--;
    }

    public void DeleteColumn()
    {
        if (columnCount == 0) return;
        var name = GetColumnName(columnCount);
        foreach (var key in cells.Keys.Where(k => Split(k).col == name).ToList())
            cells.Remove(key);
        columnCount--;
    }

    public string GetColumnName(int index)
    {
        string result = "";
        while (index > 0)
        {
            index--;
            result = (char)('A' + index % 26) + result;
            index /= 26;
        }
        return result;
    }

    private static (string col, int row) Split(string name)
    {
        var m = Regex.Match(name, @"^([A-Z]+)(\d+)$");
        return (m.Groups[1].Value, int.Parse(m.Groups[2].Value));
    }
    public IEnumerable<Cell> GetAllCells() => cells.Values;

    public void Clear() => cells.Clear();
}
