using System.Text;

namespace ReceiptRen;

/// <summary>
/// Exports the monthly report as a plain text file.
/// </summary>
public class TextReportGenerator : IReportGenerator
{
    /// <inheritdoc />
    public string FileExtension => ".txt";

    /// <inheritdoc />
    public string FileFilter => "Text file (*.txt)|*.txt";

    /// <inheritdoc />
    public void Generate(MonthlyReport report, MonthComparison comparison, IReadOnlyList<string> insights, string filePath)
    {
        List<string> lines = ReportFormatter.BuildLines(report, comparison, insights);
        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }
}
