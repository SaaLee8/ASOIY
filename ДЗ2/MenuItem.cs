using System;

/// <summary>
/// Блюдо в меню (основная таблица, сторона «много»)
/// </summary>
class MenuItem
{
    /// <summary>Идентификатор блюда</summary>
    public int Id { get; set; }
    
    /// <summary>Идентификатор ресторана (внешний ключ)</summary>
    public int RestaurantId { get; set; }
    
    /// <summary>Название блюда</summary>
    public string Name { get; set; }
    
    private decimal _price;
    
    /// <summary>
    /// Цена блюда в рублях (не может быть отрицательной)
    /// </summary>
    public decimal Price
    {
        get => _price;
        set
        {
            if (value < 0)
                throw new ArgumentException("Цена блюда не может быть отрицательной");
            _price = value;
        }
    }
    
    /// <summary>Конструктор с параметрами</summary>
    public MenuItem(int id, int restaurantId, string name, decimal price)
    {
        Id = id;
        RestaurantId = restaurantId;
        Name = name;
        Price = price;
    }
    
    /// <summary>Конструктор по умолчанию</summary>
    public MenuItem() : this(0, 0, "", 0) { }
    
    public override string ToString() => $"[{Id}] {Name}, ресторан #{RestaurantId}, цена: {Price} руб.";
}