using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Models;

namespace Rental.Data.Context;

public class AppDbContext : DbContext
{
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<RentalRecord> RentalRecords => Set<RentalRecord>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=rental.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => i.InventoryNumber)
            .IsUnique();

        modelBuilder.Entity<RentalRecord>()
            .HasOne(r => r.Inventory)
            .WithMany(i => i.Rentals)
            .HasForeignKey(r => r.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalRecord>()
            .HasOne(r => r.Client)
            .WithMany(c => c.Rentals)
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventory>().HasData(
            new Inventory { Id = 1, Name = "Горный велосипед", Category = "Велосипеды", InventoryNumber = "INV-001", PricePerHour = 150, PricePerDay = 800, Deposit = 3000, Status = InventoryStatuses.Available },
            new Inventory { Id = 2, Name = "Горные лыжи", Category = "Лыжи", InventoryNumber = "INV-002", PricePerHour = 200, PricePerDay = 1000, Deposit = 5000, Status = InventoryStatuses.Available },
            new Inventory { Id = 3, Name = "Сноуборд", Category = "Сноуборды", InventoryNumber = "INV-003", PricePerHour = 180, PricePerDay = 900, Deposit = 4500, Status = InventoryStatuses.Rented },
            new Inventory { Id = 4, Name = "Палатка 4-местная", Category = "Кемпинг", InventoryNumber = "INV-004", PricePerHour = 100, PricePerDay = 600, Deposit = 2000, Status = InventoryStatuses.Available },
            new Inventory { Id = 5, Name = "Каяк двухместный", Category = "Водный спорт", InventoryNumber = "INV-005", PricePerHour = 250, PricePerDay = 1200, Deposit = 6000, Status = InventoryStatuses.Rented }
        );

        modelBuilder.Entity<Client>().HasData(
            new Client { Id = 1, LastName = "Иванов", FirstName = "Пётр", Passport = "4010 123456", Phone = "+7 (900) 111-22-33" },
            new Client { Id = 2, LastName = "Петрова", FirstName = "Анна", Passport = "4010 234567", Phone = "+7 (900) 222-33-44" },
            new Client { Id = 3, LastName = "Сидоров", FirstName = "Олег", Passport = "4010 345678", Phone = "+7 (900) 333-44-55" },
            new Client { Id = 4, LastName = "Козлова", FirstName = "Мария", Passport = "4010 456789", Phone = "+7 (900) 444-55-66" },
            new Client { Id = 5, LastName = "Новиков", FirstName = "Дмитрий", Passport = "4010 567890", Phone = "+7 (900) 555-66-77" }
        );

        var pastIssue = new DateTime(2025, 6, 15);
        var pastReturn = new DateTime(2025, 6, 17);

        modelBuilder.Entity<RentalRecord>().HasData(
            new RentalRecord
            {
                Id = 1,
                InventoryId = 5,
                ClientId = 1,
                IssueDate = pastIssue,
                PlannedReturnDate = pastReturn,
                Tariff = TariffTypes.Day,
                TotalAmount = 0,
                Fine = 0,
                Status = RentalRecordStatuses.Active,
                DepositAmount = 6000
            },
            new RentalRecord
            {
                Id = 2,
                InventoryId = 1,
                ClientId = 2,
                IssueDate = new DateTime(2025, 6, 1),
                PlannedReturnDate = new DateTime(2025, 6, 4),
                ActualReturnDate = new DateTime(2025, 6, 5),
                Tariff = TariffTypes.Day,
                TotalAmount = 2400,
                Fine = 400,
                Status = RentalRecordStatuses.Completed,
                DepositAmount = 3000
            },
            new RentalRecord
            {
                Id = 3,
                InventoryId = 2,
                ClientId = 3,
                IssueDate = new DateTime(2025, 6, 8),
                PlannedReturnDate = new DateTime(2025, 6, 11),
                ActualReturnDate = new DateTime(2025, 6, 12),
                Tariff = TariffTypes.Hour,
                TotalAmount = 1200,
                Fine = 600,
                Status = RentalRecordStatuses.Completed,
                DepositAmount = 5000
            },
            new RentalRecord
            {
                Id = 4,
                InventoryId = 3,
                ClientId = 4,
                IssueDate = new DateTime(2025, 6, 16),
                PlannedReturnDate = new DateTime(2025, 6, 19),
                Tariff = TariffTypes.Day,
                TotalAmount = 0,
                Fine = 0,
                Status = RentalRecordStatuses.Active,
                DepositAmount = 4500
            },
            new RentalRecord
            {
                Id = 5,
                InventoryId = 4,
                ClientId = 5,
                IssueDate = new DateTime(2025, 5, 20),
                PlannedReturnDate = new DateTime(2025, 5, 24),
                ActualReturnDate = new DateTime(2025, 5, 25),
                Tariff = TariffTypes.Day,
                TotalAmount = 3000,
                Fine = 0,
                Status = RentalRecordStatuses.Completed,
                DepositAmount = 2000
            }
        );
    }
}
