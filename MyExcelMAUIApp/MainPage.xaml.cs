using Antlr4.Runtime;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using Grid = Microsoft.Maui.Controls.Grid;
namespace MyExcelMAUIApp
{
    public partial class MainPage : ContentPage
    {
        const int CountColumn = 20; 
        private Spreadsheet spreadsheet = new();
        private readonly FileService fileService = new();
        private bool hasUnsavedChanges = false;
        private string FilePath => Path.Combine(FileSystem.AppDataDirectory, "table.json");
        private ViewMode mode = ViewMode.EXPRESSION;
        const int CountRow = 50; 
        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
        }
        private void CreateGrid()
        {

            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries();
        }
        private void AddColumnsAndColumnLabels()
        {
            
            for (int col = 0; col < CountColumn + 1; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                if (col > 0)
                {
                    var label = new Label
                    {
                        Text = GetColumnName(col),

                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center

                    };

                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col);
                    grid.Children.Add(label);

                }
            }
        }
        private void AddRowsAndCellEntries()
        {
            grid.RowDefinitions.Add(new RowDefinition()); 

            for (int row = 0; row < CountRow; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                
                var label = new Label
                {
                    Text = (row + 1).ToString(),

                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center

                };
                Grid.SetRow(label, row + 1);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);
                
                for (int col = 0; col < CountColumn; col++)
                {
                    var entry = new Entry
                    {
                        Text = "",

                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center

                    };

                    entry.Unfocused += Entry_Unfocused;
                    entry.Focused += Entry_Focused;
                    Grid.SetRow(entry, row + 1);
                    Grid.SetColumn(entry, col + 1);
                    grid.Children.Add(entry);

                }
            }
        }
        private string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;

            }
            return columnName;
        }
        
        private string CellName(Entry entry)
    => spreadsheet.GetColumnName(Grid.GetColumn(entry)) + Grid.GetRow(entry);

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            // під час редагування завжди показуємо вираз
            entry.Text = spreadsheet.GetCell(CellName(entry)).Expression;
        }

        private void RefreshGrid()
        {
            foreach (var entry in grid.Children.OfType<Entry>())
            {
                var cell = spreadsheet.GetCell(CellName(entry));
                entry.Text = cell.GetDisplayText(mode);
                entry.TextColor = cell.Error != "" ? Colors.Red : Colors.Black;
                ToolTipProperties.SetText(entry, cell.Error);
            }
        }

        private void ModeButton_Clicked(object sender, EventArgs e)
        {
            mode = mode == ViewMode.EXPRESSION ? ViewMode.VALUE : ViewMode.EXPRESSION;
            ModeButton.Text = mode == ViewMode.EXPRESSION ? "Режим: ВИРАЗ" : "Режим: ЗНАЧЕННЯ";
            RefreshGrid();
        }
        private void Entry_Unfocused(object? sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            var name = CellName(entry);
            var newText = entry.Text ?? "";
            if (newText != spreadsheet.GetCell(name).Expression)
            {
                spreadsheet.SetExpression(name, newText);
                hasUnsavedChanges = true;
                if (mode == ViewMode.VALUE) spreadsheet.CalculateAll();
            }
            RefreshGrid();
        }
        private void CalculateButton_Clicked(object sender, EventArgs e)
        {
            spreadsheet.CalculateAll();
            mode = ViewMode.VALUE;
            ModeButton.Text = "Режим: ЗНАЧЕННЯ";
            RefreshGrid();
        }
        
        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                fileService.Save(spreadsheet, FilePath);
                hasUnsavedChanges = false;
                await DisplayAlert("Збереження", "Таблицю збережено:\n" + FilePath, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }
        private async void ReadButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    await DisplayAlert("Читання", "Збереженого файлу немає", "OK");
                    return;
                }
                spreadsheet = fileService.Load(FilePath);
                hasUnsavedChanges = false;
                RefreshGrid();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", "Не вдалося прочитати файл: " + ex.Message, "OK");
            }
        }
        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            string message = hasUnsavedChanges
    ? "Є незбережені зміни. Вийти без збереження?"
    : "Ви дійсно хочете вийти?";
            bool answer = await DisplayAlertAsync("Підтвердження", message, "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка",
                "Електронна таблиця (варіант 29).\n" +
                "Автор : Стрижало Катерина \n" +
                "У клітинку вводять число або логічний вираз.\n" +
                "Операції: + - * / mod div, mmax(...), mmin(...), <= >= <>, not.\n" +
                "Посилання на клітинки: A1, B2.\n" +
                "«Порахувати» обчислює таблицю, «Режим» перемикає вираз і значення.",
                "OK");
        }
        private Entry CreateEntry(int row, int col)
        {
            var entry = new Entry
            {
                Text = "",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            entry.Unfocused += Entry_Unfocused;
            entry.Focused += Entry_Focused;
            Grid.SetRow(entry, row);
            Grid.SetColumn(entry, col);
            return entry;
        }

        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            int newRow = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition());
            var label = new Label
            {
                Text = newRow.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, newRow);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);
            for (int col = 1; col < grid.ColumnDefinitions.Count; col++)
                grid.Children.Add(CreateEntry(newRow, col));
            spreadsheet.AddRow();
            hasUnsavedChanges = true;
        }

        private void AddColumnButton_Clicked(object sender, EventArgs e)
        {
            int newColumn = grid.ColumnDefinitions.Count;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var label = new Label
            {
                Text = spreadsheet.GetColumnName(newColumn),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, newColumn);
            grid.Children.Add(label);
            for (int row = 1; row < grid.RowDefinitions.Count; row++)
                grid.Children.Add(CreateEntry(row, newColumn));
            spreadsheet.AddColumn();
            hasUnsavedChanges = true;
        }

        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            if (grid.RowDefinitions.Count <= 2) return;
            int last = grid.RowDefinitions.Count - 1;
            var toRemove = grid.Children.OfType<View>().Where(v => Grid.GetRow(v) == last).ToList();
            foreach (var v in toRemove) grid.Children.Remove(v);
            grid.RowDefinitions.RemoveAt(last);
            spreadsheet.DeleteRow();
            hasUnsavedChanges = true;
        }

        private void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            if (grid.ColumnDefinitions.Count <= 2) return;
            int last = grid.ColumnDefinitions.Count - 1;
            var toRemove = grid.Children.OfType<View>().Where(v => Grid.GetColumn(v) == last).ToList();
            foreach (var v in toRemove) grid.Children.Remove(v);
            grid.ColumnDefinitions.RemoveAt(last);
            spreadsheet.DeleteColumn();
            hasUnsavedChanges = true;
        }
    }
}