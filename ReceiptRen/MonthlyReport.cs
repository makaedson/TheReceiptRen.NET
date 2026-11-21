namespace ReceiptRen;

/// <summary>
/// Amount and share of the month's spending for one category.
/// </summary>
public class CategoryTotal
{
    /// <summary>Name of the category.</summary>
    public string CategoryName { get; set; } = "";

    /// <summary>Total spent in this category during the month.</summary>
    public decimal Amount { get; set; }

    /// <summary>Share of the month's total spending, from 0 to 100.</summary>
    public double Percentage { get; set; }
}

/// <summary>
/// Holds the results of the monthly calculations: total, category breakdown,
/// top categories, biggest expense, daily average and busiest day.
/// </summary>
public class MonthlyReport
{
    /// <summary>Month number, 1 to 12.</summary>
    public int Month { get; set; }

    /// <summary>Four-digit year.</summary>
    public int Year { get; set; }

    /// <summary>Sum of all receipts in the month.</summary>
    public decimal TotalSpent { get; set; }

    /// <summary>Number of receipts in the month.</summary>
    public int ReceiptCount { get; set; }

    /// <summary>Average amount of one receipt (0 when there are no receipts).</summary>
    public decimal AverageReceipt { get; set; }

    /// <summary>Total divided by the number of days in the month.</summary>
    public decimal DailyAverage { get; set; }

    /// <summary>Amount and percentage per category, highest spending first.</summary>
    public List<CategoryTotal> Breakdown { get; set; } = new List<CategoryTotal>();

    /// <summary>The three categories with the highest spending.</summary>
    public List<CategoryTotal> TopCategories => Breakdown.Take(3).ToList();

    /// <summary>The single largest receipt of the month, or null when there are none.</summary>
    public Receipt? BiggestExpense { get; set; }

    /// <summary>Day of the week with the highest total spending, or null when there are no receipts.</summary>
    public DayOfWeek? BusiestDay { get; set; }

    /// <summary>Total spent on the busiest day of the week.</summary>
    public decimal BusiestDayTotal { get; set; }

    /// <summary>First day of the reported month.</summary>
    public DateTime MonthStart => new DateTime(Year, Month, 1);
}

/// <summary>
/// Change in one category's spending between the previous month and the current month.
/// </summary>
public class CategoryChange
{
    /// <summary>Name of the category.</summary>
    public string CategoryName { get; set; } = "";

    /// <summary>Amount spent in the current month.</summary>
    public decimal CurrentAmount { get; set; }

    /// <summary>Amount spent in the previous month.</summary>
    public decimal PreviousAmount { get; set; }

    /// <summary>Percentage increase (positive) or decrease (negative); null when the previous amount was zero.</summary>
    public double? ChangePercent { get; set; }
}

/// <summary>
/// Comparison of the current month with the previous month.
/// </summary>
public class MonthComparison
{
    /// <summary>Report of the current month.</summary>
    public MonthlyReport Current { get; set; } = new MonthlyReport();

    /// <summary>Report of the previous month.</summary>
    public MonthlyReport Previous { get; set; } = new MonthlyReport();

    /// <summary>Percentage change of the total; null when the previous total was zero.</summary>
    public double? TotalChangePercent { get; set; }

    /// <summary>Change per category, largest absolute change first.</summary>
    public List<CategoryChange> CategoryChanges { get; set; } = new List<CategoryChange>();
}
