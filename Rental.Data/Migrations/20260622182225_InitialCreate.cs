using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rental.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Passport = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InventoryNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PricePerHour = table.Column<decimal>(type: "TEXT", nullable: false),
                    PricePerDay = table.Column<decimal>(type: "TEXT", nullable: false),
                    Deposit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RentalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InventoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlannedReturnDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualReturnDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Tariff = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Fine = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DepositAmount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalRecords_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalRecords_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "FirstName", "LastName", "Passport", "Phone" },
                values: new object[,]
                {
                    { 1, "Пётр", "Иванов", "4010 123456", "+7 (900) 111-22-33" },
                    { 2, "Анна", "Петрова", "4010 234567", "+7 (900) 222-33-44" },
                    { 3, "Олег", "Сидоров", "4010 345678", "+7 (900) 333-44-55" },
                    { 4, "Мария", "Козлова", "4010 456789", "+7 (900) 444-55-66" },
                    { 5, "Дмитрий", "Новиков", "4010 567890", "+7 (900) 555-66-77" }
                });

            migrationBuilder.InsertData(
                table: "Inventories",
                columns: new[] { "Id", "Category", "Deposit", "InventoryNumber", "Name", "PricePerDay", "PricePerHour", "Status" },
                values: new object[,]
                {
                    { 1, "Велосипеды", 3000m, "INV-001", "Горный велосипед", 800m, 150m, "Свободен" },
                    { 2, "Лыжи", 5000m, "INV-002", "Горные лыжи", 1000m, 200m, "Свободен" },
                    { 3, "Сноуборды", 4500m, "INV-003", "Сноуборд", 900m, 180m, "В аренде" },
                    { 4, "Кемпинг", 2000m, "INV-004", "Палатка 4-местная", 600m, 100m, "Свободен" },
                    { 5, "Водный спорт", 6000m, "INV-005", "Каяк двухместный", 1200m, 250m, "В аренде" }
                });

            migrationBuilder.InsertData(
                table: "RentalRecords",
                columns: new[] { "Id", "ActualReturnDate", "ClientId", "DepositAmount", "Fine", "InventoryId", "IssueDate", "PlannedReturnDate", "Status", "Tariff", "TotalAmount" },
                values: new object[,]
                {
                    { 1, null, 1, 6000m, 0m, 5, new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Активна", "День", 0m },
                    { 2, new DateTime(2025, 6, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 3000m, 400m, 1, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Завершена", "День", 2400m },
                    { 3, new DateTime(2025, 6, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 5000m, 600m, 2, new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Завершена", "Час", 1200m },
                    { 4, null, 4, 4500m, 0m, 3, new DateTime(2025, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Активна", "День", 0m },
                    { 5, new DateTime(2025, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 2000m, 0m, 4, new DateTime(2025, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Завершена", "День", 3000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_InventoryNumber",
                table: "Inventories",
                column: "InventoryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalRecords_ClientId",
                table: "RentalRecords",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalRecords_InventoryId",
                table: "RentalRecords",
                column: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalRecords");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Inventories");
        }
    }
}
