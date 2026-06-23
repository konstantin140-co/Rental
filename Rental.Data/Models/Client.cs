using System.ComponentModel.DataAnnotations;

namespace Rental.Data.Models;

public class Client
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите фамилию")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите имя")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите паспорт")]
    [MaxLength(20)]
    public string Passport { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите телефон")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public string FullName => $"{LastName} {FirstName}";

    public ICollection<RentalRecord> Rentals { get; set; } = new List<RentalRecord>();
}
