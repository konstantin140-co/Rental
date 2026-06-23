using Rental.Data.Constants;
using Rental.Data.Helpers;

namespace Rental.Tests;

[TestFixture]
public class RentalCalculatorTests
{
    [Test]
    public void CalculateFine_ReturnedOnTime_ReturnsZero()
    {
        var due = new DateTime(2025, 6, 10, 12, 0, 0);
        var returned = new DateTime(2025, 6, 10, 11, 0, 0);

        var fine = RentalCalculator.CalculateFine(due, returned, 1000m);

        Assert.That(fine, Is.EqualTo(0m));
    }

    [Test]
    public void CalculateFine_Overdue_Returns50Percent()
    {
        var due = new DateTime(2025, 6, 10);
        var returned = new DateTime(2025, 6, 12);

        var fine = RentalCalculator.CalculateFine(due, returned, 1000m);

        Assert.That(fine, Is.EqualTo(500m));
    }

    [TestCase(1000, 500)]
    [TestCase(2000, 1000)]
    [TestCase(100, 50)]
    public void CalculateFine_VariousBaseAmounts_CorrectPercent(decimal baseAmount, decimal expectedFine)
    {
        var due = new DateTime(2025, 6, 1);
        var returned = new DateTime(2025, 6, 2);

        var fine = RentalCalculator.CalculateFine(due, returned, baseAmount);

        Assert.That(fine, Is.EqualTo(expectedFine));
    }

    [Test]
    public void CalculateRentalCost_HourTariff_ReturnsHourlyPrice()
    {
        var issue = new DateTime(2025, 6, 10, 10, 0, 0);
        var returned = new DateTime(2025, 6, 10, 12, 30, 0);

        var cost = RentalCalculator.CalculateRentalCost(issue, returned, 100m, 500m, TariffTypes.Hour);

        Assert.That(cost, Is.EqualTo(300m));
    }

    [Test]
    public void CalculateRentalCost_DayTariff_ReturnsDailyPrice()
    {
        var issue = new DateTime(2025, 6, 10);
        var returned = new DateTime(2025, 6, 13);

        var cost = RentalCalculator.CalculateRentalCost(issue, returned, 100m, 500m, TariffTypes.Day);

        Assert.That(cost, Is.EqualTo(1500m));
    }

    [Test]
    public void CalculateRentalCost_SameMoment_ReturnsZero()
    {
        var moment = new DateTime(2025, 6, 10, 10, 0, 0);

        var cost = RentalCalculator.CalculateRentalCost(moment, moment, 100m, 500m, TariffTypes.Hour);

        Assert.That(cost, Is.EqualTo(0m));
    }

    [Test]
    public void IsOverdue_BeforeDueDate_ReturnsFalse()
    {
        var due = DateTime.Today.AddDays(1);
        var check = DateTime.Today;

        Assert.That(RentalCalculator.IsOverdue(due, check), Is.False);
    }

    [Test]
    public void IsOverdue_AfterDueDate_ReturnsTrue()
    {
        var due = DateTime.Today.AddDays(-1);
        var check = DateTime.Today;

        Assert.That(RentalCalculator.IsOverdue(due, check), Is.True);
    }

    [Test]
    public void GetOverdueDays_NoOverdue_ReturnsZero()
    {
        var due = new DateTime(2025, 6, 10);
        var returned = new DateTime(2025, 6, 9);

        Assert.That(RentalCalculator.GetOverdueDays(due, returned), Is.EqualTo(0));
    }

    [Test]
    public void GetOverdueDays_ThreeDaysOverdue_ReturnsThree()
    {
        var due = new DateTime(2025, 6, 10);
        var returned = new DateTime(2025, 6, 13);

        Assert.That(RentalCalculator.GetOverdueDays(due, returned), Is.EqualTo(3));
    }

    [Test]
    public void CalculateReturnTotal_SumsCostAndFine()
    {
        Assert.That(RentalCalculator.CalculateReturnTotal(1000m, 500m), Is.EqualTo(1500m));
    }

    [Test]
    public void CalculateHours_MinimumOneHour()
    {
        var issue = new DateTime(2025, 6, 10, 10, 0, 0);
        var returned = new DateTime(2025, 6, 10, 10, 15, 0);

        Assert.That(RentalCalculator.CalculateHours(issue, returned), Is.EqualTo(1));
    }
}
