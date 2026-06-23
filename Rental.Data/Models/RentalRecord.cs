using System.ComponentModel.DataAnnotations;

namespace Rental.Data.Models;

public class RentalRecord
{
    public int Id { get; set; }

    public int InventoryId { get; set; }
    public Inventory Inventory { get; set; } = null!;

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public DateTime IssueDate { get; set; }
    public DateTime PlannedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Tariff { get; set; } = Constants.TariffTypes.Hour;

    [Range(0, 9999999)]
    public decimal TotalAmount { get; set; }

    [Range(0, 9999999)]
    public decimal Fine { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = Constants.RentalRecordStatuses.Active;

    public decimal DepositAmount { get; set; }
}
