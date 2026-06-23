using Rental.Data.Constants;

namespace Rental.Data.Helpers;

public static class RentalCalculator
{
    public const decimal FinePercent = 0.5m;

    public static int CalculateHours(DateTime issue, DateTime returnDate)
    {
        var totalHours = (returnDate - issue).TotalHours;
        return Math.Max(1, (int)Math.Ceiling(totalHours));
    }

    public static int CalculateDays(DateTime issue, DateTime returnDate)
    {
        var totalDays = (returnDate.Date - issue.Date).TotalDays;
        return Math.Max(1, (int)Math.Ceiling(totalDays));
    }

    public static decimal CalculateRentalCost(
        DateTime issue,
        DateTime returnDate,
        decimal pricePerHour,
        decimal pricePerDay,
        string tariff)
    {
        if (returnDate <= issue)
            return 0m;

        return tariff == TariffTypes.Day
            ? CalculateDays(issue, returnDate) * pricePerDay
            : CalculateHours(issue, returnDate) * pricePerHour;
    }

    public static bool IsOverdue(DateTime plannedReturn, DateTime? actualReturn, DateTime? asOf = null)
    {
        var checkDate = actualReturn ?? asOf ?? DateTime.Now;
        return checkDate > plannedReturn;
    }

    public static int GetOverdueDays(DateTime plannedReturn, DateTime actualReturn)
    {
        if (actualReturn <= plannedReturn)
            return 0;

        return (int)Math.Ceiling((actualReturn - plannedReturn).TotalDays);
    }

    public static decimal CalculateFine(DateTime plannedReturn, DateTime actualReturn, decimal baseAmount)
    {
        if (!IsOverdue(plannedReturn, actualReturn))
            return 0m;

        return baseAmount * FinePercent;
    }

    public static decimal CalculateReturnTotal(decimal rentalCost, decimal fine)
        => rentalCost + fine;
}
