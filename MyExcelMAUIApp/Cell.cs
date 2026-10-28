namespace MyExcelMAUIApp;

public class Cell
{
    public string Name { get; set; } = "";
    public string Expression { get; set; } = "";
    public bool? Value { get; set; }
    public string Error { get; set; } = "";

    public string GetDisplayText(ViewMode mode)
    {
        if (mode == ViewMode.EXPRESSION) return Expression;
        if (Error != "") return "Помилка";
        if (Value.HasValue) return Value.Value ? "true" : "false";
        return Expression;
    }
}