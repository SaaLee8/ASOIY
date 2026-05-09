using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Управление базой данных SQLite.
/// Инкапсулирует все операции с БД: создание таблиц,
/// импорт CSV, CRUD-операции, выполнение запросов для отчётов.
/// </summary>
class DatabaseManager
{
    private string _connectionString;
    
    /// <summary>
    /// Конструктор. Принимает путь к файлу БД.
    /// </summary>
    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }
    
    // ========== Инициализация ==========
    
    /// <summary>
    /// Создаёт таблицы (если не существуют) и загружает CSV при первом запуске
    /// </summary>
    public void InitializeDatabase(string restaurantsCsvPath, string menuItemsCsvPath)
    {
        CreateTables();
        
        if (GetAllRestaurants().Count == 0 && File.Exists(restaurantsCsvPath))
        {
            ImportRestaurantsFromCsv(restaurantsCsvPath);
            Console.WriteLine($"[OK] Загружены рестораны из {restaurantsCsvPath}");
        }
        
        if (GetAllMenuItems().Count == 0 && File.Exists(menuItemsCsvPath))
        {
            ImportMenuItemsFromCsv(menuItemsCsvPath);
            Console.WriteLine($"[OK] Загружены блюда из {menuItemsCsvPath}");
        }
    }
    
    /// <summary>Создание таблиц</summary>
    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS restaurants (
                rest_id INTEGER PRIMARY KEY AUTOINCREMENT,
                rest_name TEXT NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS menu_items (
                item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                rest_id INTEGER NOT NULL,
                item_name TEXT NOT NULL,
                price DECIMAL(10,2) NOT NULL,
                FOREIGN KEY (rest_id) REFERENCES restaurants(rest_id)
            );
        ";
        cmd.ExecuteNonQuery();
    }
    
    /// <summary>Импорт ресторанов из CSV</summary>
    private void ImportRestaurantsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO restaurants (rest_id, rest_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }
    
    /// <summary>Импорт блюд из CSV</summary>
    private void ImportMenuItemsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO menu_items (item_id, rest_id, item_name, price)
                VALUES (@id, @restId, @name, @price)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@restId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@price", decimal.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }
    }
    
    // ========== Чтение данных ==========
    
    /// <summary>Получить все рестораны</summary>
    public List<Restaurant> GetAllRestaurants()
    {
        var result = new List<Restaurant>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT rest_id, rest_name FROM restaurants ORDER BY rest_id";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Restaurant(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }
    
    /// <summary>Получить все блюда</summary>
    public List<MenuItem> GetAllMenuItems()
    {
        var result = new List<MenuItem>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT item_id, rest_id, item_name, price FROM menu_items ORDER BY item_id";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MenuItem(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDecimal(3)));
        }
        return result;
    }
    
    /// <summary>Получить блюдо по Id</summary>
    public MenuItem GetMenuItemById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT item_id, rest_id, item_name, price FROM menu_items WHERE item_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new MenuItem(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDecimal(3));
        }
        return null;
    }
    
    /// <summary>Получить блюда конкретного ресторана (фильтр по категории)</summary>
    public List<MenuItem> GetMenuItemsByRestaurant(int restId)
    {
        var result = new List<MenuItem>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT item_id, rest_id, item_name, price 
            FROM menu_items WHERE rest_id = @restId ORDER BY item_name";
        cmd.Parameters.AddWithValue("@restId", restId);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MenuItem(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDecimal(3)));
        }
        return result;
    }
    
    // ========== Изменение данных ==========
    
    /// <summary>Добавить блюдо (Id генерируется автоматически)</summary>
    public void AddMenuItem(MenuItem item)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO menu_items (rest_id, item_name, price)
            VALUES (@restId, @name, @price)";
        cmd.Parameters.AddWithValue("@restId", item.RestaurantId);
        cmd.Parameters.AddWithValue("@name", item.Name);
        cmd.Parameters.AddWithValue("@price", item.Price);
        cmd.ExecuteNonQuery();
    }
    
    /// <summary>Обновить данные блюда</summary>
    public void UpdateMenuItem(MenuItem item)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE menu_items 
            SET rest_id = @restId, item_name = @name, price = @price 
            WHERE item_id = @id";
        cmd.Parameters.AddWithValue("@id", item.Id);
        cmd.Parameters.AddWithValue("@restId", item.RestaurantId);
        cmd.Parameters.AddWithValue("@name", item.Name);
        cmd.Parameters.AddWithValue("@price", item.Price);
        cmd.ExecuteNonQuery();
    }
    
    /// <summary>Удалить блюдо по Id</summary>
    public void DeleteMenuItem(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM menu_items WHERE item_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
    
    // ========== Выполнение произвольного запроса (для отчётов) ==========
    
    /// <summary>
    /// Выполняет SQL-запрос и возвращает имена столбцов и строки результата.
    /// Используется классом ReportBuilder.
    /// </summary>
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        
        using var reader = cmd.ExecuteReader();
        
        // Имена столбцов
        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);
        
        // Строки данных
        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        
        return (columns, rows);
    }
    
    // ========== Экспорт в CSV ==========
    
    /// <summary>Экспорт обеих таблиц в CSV-файлы</summary>
    public void ExportToCsv(string restaurantsPath, string menuItemsPath)
    {
        // Экспорт ресторанов
        var restLines = new List<string>();
        restLines.Add("rest_id;rest_name");
        foreach (var rest in GetAllRestaurants())
            restLines.Add($"{rest.Id};{rest.Name}");
        File.WriteAllLines(restaurantsPath, restLines);
        
        // Экспорт блюд
        var itemLines = new List<string>();
        itemLines.Add("item_id;rest_id;item_name;price");
        foreach (var item in GetAllMenuItems())
            itemLines.Add($"{item.Id};{item.RestaurantId};{item.Name};{item.Price}");
        File.WriteAllLines(menuItemsPath, itemLines);
        
        Console.WriteLine($"Экспорт завершён: {restaurantsPath}, {menuItemsPath}");
    }
}