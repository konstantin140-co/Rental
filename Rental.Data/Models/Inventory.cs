using System.ComponentModel.DataAnnotations;

namespace Rental.Data.Models;

public class Inventory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите категорию")]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите инвентарный номер")]
    [MaxLength(50)]
    public string InventoryNumber { get; set; } = string.Empty;

    [Range(0, 999999, ErrorMessage = "Цена за час должна быть неотрицательной")]
    public decimal PricePerHour { get; set; }

    [Range(0, 999999, ErrorMessage = "Цена за сутки должна быть неотрицательной")]
    public decimal PricePerDay { get; set; }

    [Range(0, 999999, ErrorMessage = "Залог должен быть неотрицательным")]
    public decimal Deposit { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = Constants.InventoryStatuses.Available;

    public ICollection<RentalRecord> Rentals { get; set; } = new List<RentalRecord>();
}
