using System.Globalization;

namespace ReceiptRen;

/// <summary>
/// Turns raw receipts into the monthly report, the month-to-month comparison and the
/// plain-language insights. All calculations use LINQ (Where, GroupBy, Sum, Average, OrderByDescending).
/// </summary>
public class ReportService
{
    private readonly ReceiptController _receipts;

    /// <summary>Creates the service with the controller used to load receipts.</summary>
    public ReportService(ReceiptController receipts)
    {
        _receipts = receipts;
    }

    /// <summary>Builds the report (totals, breakdown, biggest expense, busiest day...) for one month.</summary>
    public MonthlyReport GenerateReport(int month, int year)
    {
        List<Receipt> receipts = _receipts.GetByMonth(month, year);

        MonthlyReport report = new MonthlyReport { Month = month, Year = year };

        report.ReceiptCount = receipts.Count;
        report.TotalSpent = receipts.Sum(r => r.Amount);
        report.AverageReceipt = receipts.Count > 0 ? receipts.Average(r => r.Amount) : 0m;
        report.DailyAverage = report.TotalSpent / DateTime.DaysInMonth(year, month);

        // Category breakdown: amount and percentage per category, highest first.
        report.Breakdown = receipts
            .GroupBy(r => r.CategoryName)
            .Select(g => new CategoryTotal
            {
                CategoryName = g.Key,
                Amount = g.Sum(r => r.Amount),
                Percentage = report.TotalSpent > 0 ? (double)(g.Sum(r => r.Amount) / report.TotalSpent * 100m) : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Single largest receipt of the month.
        report.BiggestExpense = receipts.OrderByDescending(r => r.Amount).FirstOrDefault();

        // Day of the week with the highest total spending.
        var busiest = receipts
            .GroupBy(r => r.ReceiptDate.DayOfWeek)
            .Select(g => new { Day = g.Key, Total = g.Sum(r => r.Amount) })
            .OrderByDescending(x => x.Total)
            .FirstOrDefault();

        if (busiest != null)
        {
            report.BusiestDay = busiest.Day;
            report.BusiestDayTotal = busiest.Total;
        }

        return report;
    }

    /// <summary>
    /// Compares the given month with the month before it: percentage change of the total
    /// and of every category (categories that appear in either month are included).
    /// </summary>
    public MonthComparison CompareMonths(MonthlyReport current)
    {
        DateTime previousMonth = current.MonthStart.AddMonths(-1);
        MonthlyReport previous = GenerateReport(previousMonth.Month, previousMonth.Year);

        MonthComparison comparison = new MonthComparison
        {
            Current = current,
            Previous = previous,
            TotalChangePercent = PercentChange(current.TotalSpent, previous.TotalSpent)
        };

        IEnumerable<string> names = current.Breakdown.Select(c => c.CategoryName)
            .Union(previous.Breakdown.Select(c => c.CategoryName));

        comparison.CategoryChanges = names
            .Select(name =>
            {
                decimal now = current.Breakdown.Where(c => c.CategoryName == name).Sum(c => c.Amount);
                decimal before = previous.Breakdown.Where(c => c.CategoryName == name).Sum(c => c.Amount);
                return new CategoryChange
                {
                    CategoryName = name,
                    CurrentAmount = now,
                    PreviousAmount = before,
                    ChangePercent = PercentChange(now, before)
                };
            })
            .OrderByDescending(c => Math.Abs(c.CurrentAmount - c.PreviousAmount))
            .ToList();

        return comparison;
    }

    /// <summary>Converts the calculated numbers into friendly sentences.</summary>
    public List<string> GetInsights(MonthlyReport report, MonthComparison comparison)
    {
        List<string> insights = new List<string>();

        if (report.ReceiptCount == 0)
        {
            insights.Add("No receipts were recorded for " + MonthName(report) + ".");
            return insights;
        }

        CategoryTotal top = report.Breakdown[0];
        insights.Add(top.CategoryName + " was " + top.Percentage.ToString("0.#", CultureInfo.InvariantCulture)
                     + "% of your spending this month.");

        if (report.BusiestDay.HasValue)
            insights.Add("You spent the most on " + report.BusiestDay.Value + "s.");

        if (report.BiggestExpense != null)
        {
            Receipt big = report.BiggestExpense;
            insights.Add("Your biggest single expense was " + big.CategoryName + " (" + big.StoreName
                         + ", " + big.Amount.ToString("N2", CultureInfo.InvariantCulture) + ") on the "
                         + Ordinal(big.ReceiptDate.Day) + ".");
        }

        // Comparison sentences are only meaningful when last month had spending.
        if (comparison.Previous.ReceiptCount > 0)
        {
            if (comparison.TotalChangePercent.HasValue)
                insights.Add("Your total spending " + Direction(comparison.TotalChangePercent.Value)
                             + " compared to last month.");

            foreach (CategoryChange change in comparison.CategoryChanges
                         .Where(c => c.ChangePercent.HasValue && Math.Round(c.ChangePercent.Value) != 0)
                         .Take(3))
            {
                insights.Add(change.CategoryName + " " + Direction(change.ChangePercent!.Value)
                             + " compared to last month.");
            }

            foreach (CategoryChange change in comparison.CategoryChanges
                         .Where(c => c.PreviousAmount == 0 && c.CurrentAmount > 0))
            {
                insights.Add(change.CategoryName + " is new this month (no spending last month).");
            }
        }
        else
        {
            insights.Add("There are no receipts for last month, so there is nothing to compare with yet.");
        }

        return insights;
    }

    /// <summary>Percentage change from "before" to "now"; null when "before" is zero.</summary>
    public static double? PercentChange(decimal now, decimal before)
    {
        if (before == 0) return null;
        return (double)((now - before) / before * 100m);
    }

    /// <summary>Formats a number as 1st, 2nd, 3rd, 4th...</summary>
    public static string Ordinal(int number)
    {
        if (number % 100 is 11 or 12 or 13) return number + "th";

        return (number % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th"
        };
    }

    /// <summary>Returns text such as "April 2026" for a report.</summary>
    public static string MonthName(MonthlyReport report)
    {
        return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(report.Month) + " " + report.Year;
    }

    /// <summary>Returns "increased by 20%" or "decreased by 20%".</summary>
    private static string Direction(double percent)
    {
        string amount = Math.Abs(percent).ToString("0.#", CultureInfo.InvariantCulture) + "%";
        return percent >= 0 ? "increased by " + amount : "decreased by " + amount;
    }
}
