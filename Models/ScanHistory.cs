using SQLite;

namespace Fooddrink.Models;

[Table("ScanHistory")]
public class ScanHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [MaxLength(50)]
    public string Barcode { get; set; } = string.Empty;
    [MaxLength(100)]
    public string FoodName { get; set; } = string.Empty;
    [MaxLength(500)]
    public string NutritionInfo { get; set; } = string.Empty;
    public DateTime ScanDate { get; set; } = DateTime.Now;
}
