using System.Text.Json;

namespace MyExcelMAUIApp;

public class FileService
{
    public void Save(Spreadsheet spreadsheet, string path)
    {
        var data = spreadsheet.GetAllCells()
            .Where(c => c.Expression != "")
            .ToDictionary(c => c.Name, c => c.Expression);
        File.WriteAllText(path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }

    public Spreadsheet Load(string path)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
                   ?? throw new InvalidDataException("Порожній файл");
        var sheet = new Spreadsheet();
        foreach (var pair in data)
            sheet.SetExpression(pair.Key, pair.Value);
        return sheet;
    }
}
