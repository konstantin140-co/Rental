namespace Rental.Data.Constants;

public static class InventoryStatuses
{
    public const string Available = "Свободен";
    public const string Rented = "В аренде";
    public const string Repair = "На ремонте";

    public static readonly string[] All = { Available, Rented, Repair };
}

public static class RentalRecordStatuses
{
    public const string Active = "Активна";
    public const string Completed = "Завершена";
    public const string Overdue = "Просрочена";

    public static readonly string[] All = { Active, Completed, Overdue };
}

public static class TariffTypes
{
    public const string Hour = "Час";
    public const string Day = "День";

    public static readonly string[] All = { Hour, Day };
}
