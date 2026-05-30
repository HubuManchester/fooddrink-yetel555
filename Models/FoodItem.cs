using SQLite;

namespace Fooddrink.Models;

[Table("FoodItems")]
public class FoodItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Ingredients { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public bool IsFavorite { get; set; }
}
