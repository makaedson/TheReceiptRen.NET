using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace ReceiptRen;

/// <summary>
/// Exports the monthly report as a PDF file using PDFsharp.
/// </summary>
public class PdfReportGenerator : IReportGenerator
{
    private static bool _fontsConfigured;

    /// <inheritdoc />
    public string FileExtension => ".pdf";

    /// <inheritdoc />
    public string FileFilter => "PDF file (*.pdf)|*.pdf";

    /// <inheritdoc />
    public void Generate(MonthlyReport report, MonthComparison comparison, IReadOnlyList<string> insights, string filePath)
    {
        // PDFsharp needs to be told once to use the fonts installed in Windows.
        if (!_fontsConfigured)
        {
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            _fontsConfigured = true;
        }

        List<string> lines = ReportFormatter.BuildLines(report, comparison, insights);

        using PdfDocument document = new PdfDocument();
        document.Info.Title = "ReceiptRen - " + ReportService.MonthName(report);

        XFont titleFont = new XFont("Courier New", 16, XFontStyleEx.Bold);
        XFont bodyFont = new XFont("Courier New", 10, XFontStyleEx.Regular);

        const double margin = 40;
        const double lineHeight = 15;

        PdfPage page = document.AddPage();
        XGraphics graphics = XGraphics.FromPdfPage(page);
        double y = margin;

        for (int i = 0; i < lines.Count; i++)
        {
            // Start a new page when the current one is full.
            if (y + lineHeight > page.Height.Point - margin)
            {
                graphics.Dispose();
                page = document.AddPage();
                graphics = XGraphics.FromPdfPage(page);
                y = margin;
            }

            XFont font = i == 0 ? titleFont : bodyFont;
            graphics.DrawString(lines[i], font, XBrushes.Black, new XPoint(margin, y + 10));
            y += i == 0 ? lineHeight + 8 : lineHeight;
        }

        graphics.Dispose();
        document.Save(filePath);
    }
}
