using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Построитель отчётов с использованием паттерна Fluent Interface.
/// Промежуточные методы возвращают this (цепочка вызовов).
/// Терминальные методы выполняют запрос и формируют результат.
/// </summary>
class ReportBuilder
{
    private DatabaseManager _db;
    private string _sql = "";
    private string _title = "";
    private string[] _headers = Array.Empty<string>();
    private int[] _widths = Array.Empty<int>();
    private bool _numbered = false;      // [ГРУППА А]
    private string _footer = "";         // [ГРУППА В]
    
    /// <summary>Конструктор. Принимает DatabaseManager для выполнения запросов.</summary>
    public ReportBuilder(DatabaseManager db)
    {
        _db = db;
    }
    
    // ========== Промежуточные методы (возвращают this) ==========
    
    public ReportBuilder Query(string sql) { _sql = sql; return this; }
    public ReportBuilder Title(string title) { _title = title; return this; }
    public ReportBuilder Header(params string[] cols) { _headers = cols; return this; }
    public ReportBuilder ColumnWidths(params int[] widths) { _widths = widths; return this; }
    public ReportBuilder Numbered() { _numbered = true; return this; }
    public ReportBuilder Footer(string label) { _footer = label; return this; }
    
    // ========== Терминальные методы ==========
    
    public string Build()
    {
        var (columns, rows) = _db.ExecuteQuery(_sql);
        var sb = new StringBuilder();
        
        if (_title.Length > 0)
            sb.AppendLine($"\n=== {_title} ===");
        
        var displayHeaders = _headers.Length > 0 ? _headers : columns;
        int colCount = displayHeaders.Length;
        
        var widths = new int[colCount];
        for (int i = 0; i < colCount; i++)
            widths[i] = (_widths.Length > i) ? _widths[i] : 20;
        
        int numWidth = _numbered ? 5 : 0;
        
        if (_numbered)
            sb.Append("№".PadRight(numWidth));
        for (int i = 0; i < colCount; i++)
            sb.Append(displayHeaders[i].PadRight(widths[i]));
        sb.AppendLine();
        
        int totalWidth = numWidth;
        for (int i = 0; i < colCount; i++)
            totalWidth += widths[i];
        sb.AppendLine(new string('-', totalWidth));
        
        for (int r = 0; r < rows.Count; r++)
        {
            if (_numbered)
                sb.Append((r + 1).ToString().PadRight(numWidth));
            for (int c = 0; c < rows[r].Length && c < colCount; c++)
                sb.Append(rows[r][c].PadRight(widths[c]));
            sb.AppendLine();
        }
        
        if (_footer.Length > 0)
        {
            sb.AppendLine(new string('-', totalWidth));
            sb.AppendLine($"{_footer}: {rows.Count}");
        }
        
        return sb.ToString();
    }
    
    public void Print() => Console.WriteLine(Build());
    public void SaveToFile(string path) => File.WriteAllText(path, Build());
}