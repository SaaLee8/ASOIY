using System;
using System.IO;
using System.Text;

// ══════════════════════════════════════════════════════════
// Точка входа — консольное меню
// ══════════════════════════════════════════════════════════

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Пути к файлам
string dbPath = "restaurant.db";
string restaurantsCsv = "restaurants.csv";
string menuItemsCsv = "menu_items.csv";

// Создаём CSV файлы программно, если их нет
if (!File.Exists(restaurantsCsv))
{
    File.WriteAllText(restaurantsCsv, 
        "rest_id;rest_name\n1;Итальянский дворик\n2;Японская кухня Сакура\n3;Грузинский дворик\n4;Французская пекарня", 
        Encoding.UTF8);
    Console.WriteLine("Создан restaurants.csv");
}

if (!File.Exists(menuItemsCsv))
{
    File.WriteAllText(menuItemsCsv, 
        "item_id;rest_id;item_name;price\n1;1;Пицца Маргарита;450\n2;1;Паста Карбонара;380\n3;1;Тирамису;250\n4;2;Роллы Филадельфия;550\n5;2;Суши Лосось;320\n6;2;Рамен;480\n7;3;Хачапури по-аджарски;350\n8;3;Хинкали;280\n9;3;Шашлык из свинины;450\n10;4;Круассан;150\n11;4;Багет;80\n12;4;Эклер;120", 
        Encoding.UTF8);
    Console.WriteLine("Создан menu_items.csv");
}

// Создаём менеджер БД и инициализируем данные
var db = new DatabaseManager(dbPath);
db.InitializeDatabase(restaurantsCsv, menuItemsCsv);

Console.WriteLine();

// Главный цикл меню
string choice;
do
{
    Console.WriteLine("╔═══════════════════════════════════════╗");
    Console.WriteLine("║        УПРАВЛЕНИЕ МЕНЮ РЕСТОРАНОВ     ║");
    Console.WriteLine("╠═══════════════════════════════════════╣");
    Console.WriteLine("║  1 — Показать все рестораны           ║");
    Console.WriteLine("║  2 — Показать все блюда               ║");
    Console.WriteLine("║  3 — Добавить блюдо                   ║");
    Console.WriteLine("║  4 — Редактировать блюдо              ║");
    Console.WriteLine("║  5 — Удалить блюдо                    ║");
    Console.WriteLine("║  6 — Отчёты                           ║");
    Console.WriteLine("║  7 — Фильтр по ресторану [ГРУППА Г]   ║");
    Console.WriteLine("║  8 — Экспорт в CSV [ГРУППА Б]         ║");
    Console.WriteLine("║  0 — Выход                            ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");
    
    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();
    
    switch (choice)
    {
        case "1": ShowRestaurants(db); break;
        case "2": ShowMenuItems(db); break;
        case "3": AddMenuItem(db); break;
        case "4": EditMenuItem(db); break;
        case "5": DeleteMenuItem(db); break;
        case "6": ReportsMenu(db); break;
        case "7": FilterByRestaurant(db); break;
        case "8": db.ExportToCsv("restaurants_export.csv", "menu_items_export.csv"); break;
        case "0": Console.WriteLine("До свидания!"); break;
        default: Console.WriteLine("Неверный пункт."); break;
    }
    Console.WriteLine();
}
while (choice != "0");

// ==========================================================
// Функции пунктов меню
// ==========================================================

static void ShowRestaurants(DatabaseManager db)
{
    Console.WriteLine("---- РЕСТОРАНЫ ----");
    var restaurants = db.GetAllRestaurants();
    foreach (var r in restaurants)
        Console.WriteLine($"  {r}");
    Console.WriteLine($"Итого: {restaurants.Count}");
}

static void ShowMenuItems(DatabaseManager db)
{
    Console.WriteLine("---- БЛЮДА ----");
    var items = db.GetAllMenuItems();
    foreach (var i in items)
        Console.WriteLine($"  {i}");
    Console.WriteLine($"Итого: {items.Count}");
}

static void AddMenuItem(DatabaseManager db)
{
    Console.WriteLine("---- ДОБАВЛЕНИЕ БЛЮДА ----");
    
    Console.WriteLine("Доступные рестораны:");
    var restaurants = db.GetAllRestaurants();
    foreach (var r in restaurants)
        Console.WriteLine($"  {r}");
    
    Console.Write("ID ресторана: ");
    if (!int.TryParse(Console.ReadLine(), out int restId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    Console.Write("Название блюда: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0)
    {
        Console.WriteLine("Ошибка: название не может быть пустым.");
        return;
    }
    
    Console.Write("Цена (руб.): ");
    if (!decimal.TryParse(Console.ReadLine(), out decimal price))
    {
        Console.WriteLine("Ошибка: введите число.");
        return;
    }
    
    try
    {
        db.AddMenuItem(new MenuItem(0, restId, name, price));
        Console.WriteLine("Блюдо добавлено!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditMenuItem(DatabaseManager db)
{
    Console.WriteLine("---- РЕДАКТИРОВАНИЕ БЛЮДА ----");
    Console.Write("ID блюда: ");
    
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    var item = db.GetMenuItemById(id);
    if (item == null)
    {
        Console.WriteLine("Блюдо не найдено");
        return;
    }
    
    Console.WriteLine($"Текущие данные: {item}");
    Console.WriteLine("(Нажмите Enter, чтобы оставить значение без изменений)");
    
    // Название
    Console.Write($"Название [{item.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0) item.Name = input;
    
    // Ресторан
    Console.Write($"ID ресторана [{item.RestaurantId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newRestId))
        item.RestaurantId = newRestId;
    
    // Цена
    Console.Write($"Цена [{item.Price}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && decimal.TryParse(input, out decimal newPrice))
    {
        try
        {
            item.Price = newPrice;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return;
        }
    }
    
    db.UpdateMenuItem(item);
    Console.WriteLine("Данные обновлены!");
}

static void DeleteMenuItem(DatabaseManager db)
{
    Console.WriteLine("---- УДАЛЕНИЕ БЛЮДА ----");
    Console.Write("ID блюда: ");
    
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    var item = db.GetMenuItemById(id);
    if (item == null)
    {
        Console.WriteLine("Блюдо не найдено");
        return;
    }
    
    Console.Write($"Удалить '{item.Name}'? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
    if (confirm == "да")
    {
        db.DeleteMenuItem(id);
        Console.WriteLine("Блюдо удалено!");
    }
    else
    {
        Console.WriteLine("Удаление отменено.");
    }
}

// ══════════════════════════════════════════════════════════
// Подменю отчётов
// ══════════════════════════════════════════════════════════

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- ОТЧЁТЫ ---");
        Console.WriteLine("  1 - Блюда по ресторанам (полный список)");
        Console.WriteLine("  2 - Количество блюд по ресторанам");
        Console.WriteLine("  3 - Средняя цена блюд по ресторанам");
        Console.WriteLine("  0 - Назад");
        Console.Write("Ваш выбор: ");
        
        choice = Console.ReadLine()?.Trim() ?? "";
        
        switch (choice)
        {
            case "1":
                new ReportBuilder(db)
                    .Query(@"
                        SELECT mi.item_name, r.rest_name, mi.price 
                        FROM menu_items mi
                        JOIN restaurants r ON mi.rest_id = r.rest_id 
                        ORDER BY mi.item_name")
                    .Title("БЛЮДА ПО РЕСТОРАНАМ")
                    .Header("Блюдо", "Ресторан", "Цена (руб.)")
                    .ColumnWidths(25, 22, 12)
                    .Numbered()     // [ГРУППА А] нумерация строк
                    .Footer("Всего блюд")  // [ГРУППА В] итоговая строка
                    .Print();
                break;
            case "2":
                new ReportBuilder(db)
                    .Query(@"
                        SELECT r.rest_name, COUNT(*) AS cnt
                        FROM menu_items mi
                        JOIN restaurants r ON mi.rest_id = r.rest_id
                        GROUP BY r.rest_name
                        ORDER BY r.rest_name")
                    .Title("КОЛИЧЕСТВО БЛЮД ПО РЕСТОРАНАМ")
                    .Header("Ресторан", "Кол-во блюд")
                    .ColumnWidths(25, 12)
                    .Print();
                break;
            case "3":
                new ReportBuilder(db)
                    .Query(@"
                        SELECT r.rest_name, ROUND(AVG(mi.price), 2) AS avg_price
                        FROM menu_items mi
                        JOIN restaurants r ON mi.rest_id = r.rest_id
                        GROUP BY r.rest_name
                        ORDER BY avg_price DESC")
                    .Title("СРЕДНЯЯ ЦЕНА БЛЮД ПО РЕСТОРАНАМ")
                    .Header("Ресторан", "Средняя цена (руб.)")
                    .ColumnWidths(25, 20)
                    .Print();
                break;
        }
        Console.WriteLine();
    }
    while (choice != "0");
}

// [ГРУППА Г] Фильтр по ресторану
static void FilterByRestaurant(DatabaseManager db)
{
    Console.WriteLine("---- ФИЛЬТР ПО РЕСТОРАНУ ----");
    Console.WriteLine("Доступные рестораны:");
    var restaurants = db.GetAllRestaurants();
    foreach (var r in restaurants)
        Console.WriteLine($"  {r}");
    
    Console.Write("Введите ID ресторана: ");
    if (!int.TryParse(Console.ReadLine(), out int restId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    var items = db.GetMenuItemsByRestaurant(restId);
    if (items.Count == 0)
    {
        Console.WriteLine("В этом ресторане нет блюд.");
        return;
    }
    
    var restaurant = restaurants.Find(r => r.Id == restId);
    Console.WriteLine($"\nБлюда ресторана «{restaurant?.Name}»:");
    foreach (var i in items)
        Console.WriteLine($"  {i}");
    Console.WriteLine($"Итого: {items.Count}");
}