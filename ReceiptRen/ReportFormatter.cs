using System.Globalization;

namespace ReceiptRen;

/// <summary>
/// Builds the lines of text of a monthly report. Both export formats use it,
/// so the text file and the PDF always contain the same information.
/// </summary>
public static class ReportFormatter
{
    /// <summary>Returns the report as a list of text lines (the first line is the title).</summary>
    public static List<string> BuildLines(MonthlyReport report, MonthComparison comparison, IReadOnlyList<string> insights)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        List<string> lines = new List<string>
        {
            "ReceiptRen - Monthly Spending Report",
            ReportService.MonthName(report),
            "",
            "Total spent:        " + report.TotalSpent.ToString("N2", ci),
            "Receipts:           " + report.ReceiptCount,
            "Daily average:      " + report.DailyAverage.ToString("N2", ci),
            "Average receipt:    " + report.AverageReceipt.ToString("N2", ci),
            "Busiest day:        " + (report.BusiestDay.HasValue
                ? report.BusiestDay.Value + " (" + report.BusiestDayTotal.ToString("N2", ci) + ")"
                : "-"),
            "Biggest expense:    " + (report.BiggestExpense != null
                ? report.BiggestExpense.StoreName + " [" + report.BiggestExpense.CategoryName + "] "
                  + report.BiggestExpense.Amount.ToString("N2", ci) + " on "
                  + report.BiggestExpense.ReceiptDate.ToString("yyyy-MM-dd", ci)
                : "-"),
            "",
            "Top 3 categories"
        };

        int rank = 1;
        foreach (CategoryTotal c in report.TopCategories)
        {
            lines.Add("  " + rank++ + ". " + c.CategoryName + " - " + c.Amount.ToString("N2", ci)
                      + " (" + c.Percentage.ToString("0.0", ci) + "%)");
        }

        lines.Add("");
        lines.Add("Category breakdown");
        foreach (CategoryTotal c in report.Breakdown)
        {
            lines.Add("  " + c.CategoryName + ": " + c.Amount.ToString("N2", ci)
                      + " (" + c.Percentage.ToString("0.0", ci) + "%)");
        }

        lines.Add("");
        lines.Add("Comparison with " + ReportService.MonthName(comparison.Previous));
        lines.Add("  Total: " + comparison.Previous.TotalSpent.ToString("N2", ci) + " -> "
                  + comparison.Current.TotalSpent.ToString("N2", ci)
                  + " (" + FormatChange(comparison.TotalChangePercent) + ")");
        foreach (CategoryChange c in comparison.CategoryChanges)
        {
            lines.Add("  " + c.CategoryName + ": " + c.PreviousAmount.ToString("N2", ci) + " -> "
                      + c.CurrentAmount.ToString("N2", ci) + " (" + FormatChange(c.ChangePercent) + ")");
        }

        lines.Add("");
        lines.Add("Insights");
        foreach (string insight in insights)
            lines.Add("  - " + insight);

        return lines;
    }

    /// <summary>Formats a percentage change as "+20.0%", "-5.0%" or "new" when there is no earlier value.</summary>
    public static string FormatChange(double? percent)
    {
        if (!percent.HasValue) return "new";
        return (percent.Value >= 0 ? "+" : "") + percent.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%";
    }
}
