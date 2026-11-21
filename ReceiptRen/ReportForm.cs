using System.Globalization;
using MySqlConnector;

namespace ReceiptRen;

/// <summary>
/// Shows the monthly report: summary cards, donut chart, category table, month comparison,
/// insights, and lets the user export the report as text or PDF.
/// </summary>
public class ReportForm : Form
{
    private readonly ReportService _service = new ReportService(new ReceiptController());
    private readonly ComboBox cboMonth = new ComboBox();
    private readonly NumericUpDown numYear = new NumericUpDown();
    private readonly GradientPanel banner = new GradientPanel();

    private readonly SummaryCard cardTotal = new SummaryCard { Title = "Total spent", Accent = Theme.Blue };
    private readonly SummaryCard cardDaily = new SummaryCard { Title = "Daily average", Accent = Theme.Green };
    private readonly SummaryCard cardBusiest = new SummaryCard { Title = "Busiest day", Accent = Theme.Orange };
    private readonly SummaryCard cardBiggest = new SummaryCard { Title = "Biggest expense", Accent = Theme.Red };
    private readonly SummaryCard cardTop = new SummaryCard { Title = "Top categories", Accent = Theme.Purple };

    private readonly PieChartPanel pie = new PieChartPanel();
    private readonly ListView lvBreakdown = new ListView();
    private readonly ListBox lstInsights = new ListBox();
    private readonly ListView lvComparison = new ListView();

    private MonthlyReport? _report;
    private MonthComparison? _comparison;
    private List<string> _insights = new List<string>();

    /// <summary>Builds the report window, starting on the given month.</summary>
    public ReportForm(int month, int year)
    {
        Text = "Monthly Report";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1100, 760);
        MinimumSize = new Size(960, 680);
        Theme.StyleForm(this);

        banner.Dock = DockStyle.Fill;
        banner.Title = "Monthly Report";
        banner.Subtitle = "Where did your money go?";

        // Rows: banner, controls bar, summary cards, content.
        TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, GradientPanel.PreferredHeight(20f, true)));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryCard.PreferredHeight + 14));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(banner, 0, 0);
        layout.Controls.Add(BuildControlsBar(month, year), 0, 1);
        layout.Controls.Add(BuildCardsRow(), 0, 2);
        layout.Controls.Add(BuildContent(), 0, 3);

        Controls.Add(layout);
        Load += (s, e) => LoadReport();
    }

    /// <summary>Creates the bar with the month and year selectors and the buttons.</summary>
    private Control BuildControlsBar(int month, int year)
    {
        FlowLayoutPanel bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 8, 8, 0),
            Margin = new Padding(12, 6, 12, 0),
            BackColor = Theme.Surface
        };

        cboMonth.DropDownStyle = ComboBoxStyle.DropDownList;
        cboMonth.Width = 130;
        for (int m = 1; m <= 12; m++)
            cboMonth.Items.Add(CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m));
        cboMonth.SelectedIndex = month - 1;

        numYear.Minimum = 2000;
        numYear.Maximum = 2100;
        numYear.Value = year;
        numYear.Width = 80;

        Button btnShow = MakeButton("Show report", Theme.Blue, (s, e) => LoadReport());
        Button btnText = MakeButton("Export as text", Theme.Green, (s, e) => Export(new TextReportGenerator()));
        Button btnPdf = MakeButton("Export as PDF", Theme.Orange, (s, e) => Export(new PdfReportGenerator()));

        bar.Controls.Add(new Label { Text = "Month", AutoSize = true, ForeColor = Theme.Muted, Margin = new Padding(6, 8, 3, 0) });
        bar.Controls.Add(cboMonth);
        bar.Controls.Add(new Label { Text = "Year", AutoSize = true, ForeColor = Theme.Muted, Margin = new Padding(14, 8, 3, 0) });
        bar.Controls.Add(numYear);
        bar.Controls.Add(btnShow);
        bar.Controls.Add(btnText);
        bar.Controls.Add(btnPdf);
        return bar;
    }

    /// <summary>Creates a styled button with a click handler.</summary>
    private static Button MakeButton(string text, Color color, EventHandler onClick)
    {
        Button button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(10, 0, 10, 0),
            Margin = new Padding(10, 0, 0, 0)
        };
        Theme.StyleButton(button, color);
        button.Click += onClick;
        return button;
    }

    /// <summary>Creates the row of five summary cards.</summary>
    private Control BuildCardsRow()
    {
        TableLayoutPanel row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, Padding = new Padding(6, 6, 6, 0) };
        for (int i = 0; i < 5; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

        SummaryCard[] cards = { cardTotal, cardDaily, cardBusiest, cardBiggest, cardTop };
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Dock = DockStyle.Fill;
            cards[i].Margin = new Padding(6, 0, 6, 0);
            row.Controls.Add(cards[i], i, 0);
        }

        return row;
    }

    /// <summary>Creates the 2 x 2 area with the chart, breakdown, insights and comparison.</summary>
    private Control BuildContent()
    {
        TableLayoutPanel content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(6) };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        SetupList(lvBreakdown, ("Category", 170), ("Amount", 110), ("% of total", 90));
        SetupList(lvComparison, ("Category", 140), ("Last month", 100), ("This month", 100), ("Change", 90));

        lstInsights.BorderStyle = BorderStyle.None;
        lstInsights.HorizontalScrollbar = true;
        lstInsights.IntegralHeight = false;
        lstInsights.ItemHeight = 26;
        lstInsights.DrawMode = DrawMode.OwnerDrawFixed;
        lstInsights.DrawItem += DrawInsight;

        content.Controls.Add(new TitledCard("Spending share", pie), 0, 0);
        content.Controls.Add(new TitledCard("Category breakdown", lvBreakdown), 1, 0);
        content.Controls.Add(new TitledCard("Insights", lstInsights), 0, 1);
        content.Controls.Add(new TitledCard("Comparison with previous month", lvComparison), 1, 1);
        return content;
    }

    /// <summary>Configures a ListView as a read-only details table with the given columns.</summary>
    private static void SetupList(ListView list, params (string Name, int Width)[] columns)
    {
        list.View = View.Details;
        list.FullRowSelect = true;
        list.GridLines = false;
        list.Dock = DockStyle.Fill;
        foreach ((string name, int width) in columns)
            list.Columns.Add(name, width);
        Theme.StyleList(list);
    }

    /// <summary>Draws one insight line with a coloured bullet in front of the text.</summary>
    private void DrawInsight(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();
        using (SolidBrush bullet = new SolidBrush(Theme.Blue))
            e.Graphics.FillEllipse(bullet, e.Bounds.X + 4, e.Bounds.Y + 9, 8, 8);

        TextRenderer.DrawText(e.Graphics, lstInsights.Items[e.Index]?.ToString() ?? "", Theme.BodyFont,
            new Rectangle(e.Bounds.X + 20, e.Bounds.Y, e.Bounds.Width - 20, e.Bounds.Height),
            Theme.TextDark, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
    }

    /// <summary>Calculates the report for the selected month and fills every part of the window.</summary>
    private void LoadReport()
    {
        try
        {
            int month = cboMonth.SelectedIndex + 1;
            int year = (int)numYear.Value;

            _report = _service.GenerateReport(month, year);
            _comparison = _service.CompareMonths(_report);
            _insights = _service.GetInsights(_report, _comparison);
        }
        catch (MySqlException ex)
        {
            MessageBox.Show("Could not load the report: " + ex.Message + "\nIs MySQL running in XAMPP?",
                "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CultureInfo ci = CultureInfo.InvariantCulture;
        MonthlyReport r = _report;

        banner.Subtitle = ReportService.MonthName(r);

        cardTotal.Value = r.TotalSpent.ToString("N2", ci);
        cardTotal.Subtitle = r.ReceiptCount + " receipt(s)";

        cardDaily.Value = r.DailyAverage.ToString("N2", ci);
        cardDaily.Subtitle = "Average receipt " + r.AverageReceipt.ToString("N2", ci);

        cardBusiest.Value = r.BusiestDay.HasValue ? r.BusiestDay.Value.ToString() : "-";
        cardBusiest.Subtitle = r.BusiestDay.HasValue ? r.BusiestDayTotal.ToString("N2", ci) + " spent" : "No receipts yet";

        Receipt? big = r.BiggestExpense;
        cardBiggest.Value = big != null ? big.Amount.ToString("N2", ci) : "-";
        cardBiggest.Subtitle = big != null ? big.StoreName + " [" + big.CategoryName + "]" : "No receipts yet";

        List<CategoryTotal> top = r.TopCategories;
        cardTop.Value = top.Count > 0 ? top[0].CategoryName : "-";
        cardTop.Subtitle = top.Count > 1 ? "then " + string.Join(", ", top.Skip(1).Select(c => c.CategoryName)) : "";

        pie.SetData(r.Breakdown);

        lvBreakdown.Items.Clear();
        foreach (CategoryTotal c in r.Breakdown)
        {
            lvBreakdown.Items.Add(new ListViewItem(new[]
            {
                c.CategoryName, c.Amount.ToString("N2", ci), c.Percentage.ToString("0.0", ci) + "%"
            }));
        }

        lstInsights.Items.Clear();
        foreach (string insight in _insights)
            lstInsights.Items.Add(insight);

        lvComparison.Items.Clear();
        foreach (CategoryChange c in _comparison!.CategoryChanges)
            AddComparisonRow(c.CategoryName, c.PreviousAmount, c.CurrentAmount, c.ChangePercent, false);
        AddComparisonRow("TOTAL", _comparison.Previous.TotalSpent, r.TotalSpent, _comparison.TotalChangePercent, true);
    }

    /// <summary>
    /// Adds one row to the comparison table. The change is red when spending went up
    /// and green when it went down.
    /// </summary>
    private void AddComparisonRow(string name, decimal previous, decimal current, double? change, bool bold)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        ListViewItem item = new ListViewItem(new[]
        {
            name, previous.ToString("N2", ci), current.ToString("N2", ci), ReportFormatter.FormatChange(change)
        });

        item.UseItemStyleForSubItems = false;
        Font font = bold ? Theme.BoldFont : Theme.BodyFont;
        foreach (ListViewItem.ListViewSubItem sub in item.SubItems)
        {
            sub.Font = font;
            sub.ForeColor = Theme.TextDark;
        }

        if (change.HasValue && Math.Round(change.Value) != 0)
            item.SubItems[3].ForeColor = change.Value > 0 ? Theme.Red : Theme.Green;

        lvComparison.Items.Add(item);
    }

    /// <summary>Asks for a file name and writes the current report with the given generator.</summary>
    private void Export(IReportGenerator generator)
    {
        if (_report == null || _comparison == null)
        {
            MessageBox.Show("Show a report first.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using SaveFileDialog dialog = new SaveFileDialog
        {
            Filter = generator.FileFilter,
            FileName = "ReceiptRen-" + _report.Year + "-" + _report.Month.ToString("00") + generator.FileExtension
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            generator.Generate(_report, _comparison, _insights, dialog.FileName);
            MessageBox.Show("Report saved to:\n" + dialog.FileName, "ReceiptRen",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show("Could not save the file: " + ex.Message, "ReceiptRen",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
