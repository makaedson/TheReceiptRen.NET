namespace ReceiptRen;

/// <summary>
/// Contract for exporting a monthly report to a file.
/// Implemented by TextReportGenerator and PdfReportGenerator (polymorphism):
/// the forms only talk to this interface and do not care which format is used.
/// </summary>
public interface IReportGenerator
{
    /// <summary>File extension including the dot, for example ".txt".</summary>
    string FileExtension { get; }

    /// <summary>Filter text for a SaveFileDialog, for example "Text file (*.txt)|*.txt".</summary>
    string FileFilter { get; }

    /// <summary>Writes the report to the given file path.</summary>
    void Generate(MonthlyReport report, MonthComparison comparison, IReadOnlyList<string> insights, string filePath);
}
