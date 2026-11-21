using System.Globalization;
using MySqlConnector;

namespace ReceiptRen;

/// <summary>
/// Main window: banner, summary cards for the current month, receipt search filters,
/// the receipts grid, and buttons to add, edit, delete, manage categories and open the monthly report.
/// </summary>
public class MainForm : Form
{
    private readonly ReceiptController _receipts = new ReceiptController();
    private readonly CategoryController _categories = new CategoryController();
    private readonly ReportService _reportService;

    private readonly GradientPanel banner = new GradientPanel();
    private readonly SummaryCard cardTotal = new SummaryCard { Title = "Spent this month", Accent = Theme.Blue };
    private readonly SummaryCard cardDaily = new SummaryCard { Title = "Daily average", Accent = Theme.Green };
    private readonly SummaryCard cardTop = new SummaryCard { Title = "Top category", Accent = Theme.Purple };
    private readonly SummaryCard cardBiggest = new SummaryCard { Title = "Biggest expense", Accent = Theme.Orange };

    private readonly DateTimePicker dtpFrom = new DateTimePicker();
    private readonly DateTimePicker dtpTo = new DateTimePicker();
    private readonly ComboBox cboCategory = new ComboBox();
    private readonly TextBox txtMin = new TextBox();
    private readonly TextBox txtMax = new TextBox();
    private readonly TextBox txtKeyword = new TextBox();
    private readonly DataGridView grid = new DataGridView();

    /// <summary>Builds the main window.</summary>
    public MainForm()
    {
        _reportService = new ReportService(_receipts);

        Text = "ReceiptRen - Personal Spending Tracker";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 720);
        MinimumSize = new Size(900, 600);
        Theme.StyleForm(this);

        // Rows: banner, summary cards, filters, receipts grid, buttons.
        TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, GradientPanel.PreferredHeight(20f, true)));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryCard.PreferredHeight + 14));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));

        banner.Dock = DockStyle.Fill;
        banner.Title = "ReceiptRen";
        banner.Subtitle = "Personal spending tracker";

        layout.Controls.Add(banner, 0, 0);
        layout.Controls.Add(BuildCardsRow(), 0, 1);
        layout.Controls.Add(BuildFilterPanel(), 0, 2);

        SetupGrid();
        Panel gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 4, 12, 4) };
        gridHost.Controls.Add(grid);
        layout.Controls.Add(gridHost, 0, 3);

        layout.Controls.Add(BuildButtonPanel(), 0, 4);

        Controls.Add(layout);
        Load += (s, e) => Run(() => { LoadCategoryFilter(); LoadReceipts(); UpdateSummary(); });
    }

    /// <summary>Creates the row of four summary cards.</summary>
    private Control BuildCardsRow()
    {
        TableLayoutPanel row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(6, 6, 6, 0) };
        for (int i = 0; i < 4; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        foreach (SummaryCard card in new[] { cardTotal, cardDaily, cardTop, cardBiggest })
        {
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(6, 0, 6, 0);
        }

        row.Controls.Add(cardTotal, 0, 0);
        row.Controls.Add(cardDaily, 1, 0);
        row.Controls.Add(cardTop, 2, 0);
        row.Controls.Add(cardBiggest, 3, 0);
        return row;
    }

    /// <summary>Creates the row of search filters (date range, category, amount range, store keyword).</summary>
    private Control BuildFilterPanel()
    {
        FlowLayoutPanel panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 6, 8, 0),
            Margin = new Padding(12, 6, 12, 0),
            BackColor = Theme.Surface
        };

        dtpFrom.Format = DateTimePickerFormat.Short;
        dtpFrom.ShowCheckBox = true;
        dtpFrom.Checked = false;
        dtpFrom.Width = 140;

        dtpTo.Format = DateTimePickerFormat.Short;
        dtpTo.ShowCheckBox = true;
        dtpTo.Checked = false;
        dtpTo.Width = 140;

        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCategory.Width = 150;

        txtMin.Width = 70;
        txtMax.Width = 70;
        txtKeyword.Width = 130;

        Button btnSearch = new Button { Text = "Search", AutoSize = true, Padding = new Padding(10, 0, 10, 0) };
        Theme.StyleButton(btnSearch, Theme.Blue);
        btnSearch.Click += (s, e) => Run(() => LoadReceipts());

        Button btnClear = new Button { Text = "Clear", AutoSize = true, Padding = new Padding(10, 0, 10, 0) };
        Theme.StyleButton(btnClear, Theme.Grey);
        btnClear.Click += (s, e) =>
        {
            dtpFrom.Checked = false;
            dtpTo.Checked = false;
            cboCategory.SelectedIndex = 0;
            txtMin.Clear();
            txtMax.Clear();
            txtKeyword.Clear();
            Run(() => LoadReceipts());
        };

        AddLabeled(panel, "From", dtpFrom);
        AddLabeled(panel, "To", dtpTo);
        AddLabeled(panel, "Category", cboCategory);
        AddLabeled(panel, "Min", txtMin);
        AddLabeled(panel, "Max", txtMax);
        AddLabeled(panel, "Store", txtKeyword);
        panel.Controls.Add(btnSearch);
        panel.Controls.Add(btnClear);

        return panel;
    }

    /// <summary>Adds a small label followed by its control to a flow panel.</summary>
    private static void AddLabeled(FlowLayoutPanel panel, string label, Control control)
    {
        panel.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            ForeColor = Theme.Muted,
            Margin = new Padding(10, 6, 3, 0)
        });
        control.Margin = new Padding(3, 3, 3, 3);
        panel.Controls.Add(control);
    }

    /// <summary>Creates the bottom row of action buttons.</summary>
    private Control BuildButtonPanel()
    {
        FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 0) };

        panel.Controls.Add(MakeButton("Add receipt", Theme.Blue, (s, e) => AddReceipt()));
        panel.Controls.Add(MakeButton("Edit selected", Theme.Green, (s, e) => EditSelected()));
        panel.Controls.Add(MakeButton("Delete selected", Theme.Red, (s, e) => DeleteSelected()));
        panel.Controls.Add(MakeButton("Categories", Theme.Purple, (s, e) => OpenCategories()));
        panel.Controls.Add(MakeButton("Monthly report", Theme.Orange, (s, e) => OpenReport()));

        return panel;
    }

    /// <summary>Creates a styled action button with a click handler.</summary>
    private static Button MakeButton(string text, Color color, EventHandler onClick)
    {
        Button button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(14, 4, 14, 4),
            Margin = new Padding(6, 0, 6, 0)
        };
        Theme.StyleButton(button, color);
        button.Click += onClick;
        return button;
    }

    /// <summary>Configures the receipts grid: read-only, full-row selection, fixed columns.</summary>
    private void SetupGrid()
    {
        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Theme.StyleGrid(grid);

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Date", DataPropertyName = nameof(Receipt.ReceiptDate), FillWeight = 14,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Store / description", DataPropertyName = nameof(Receipt.StoreName), FillWeight = 30
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Category", DataPropertyName = nameof(Receipt.CategoryName), FillWeight = 18
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Amount", DataPropertyName = nameof(Receipt.Amount), FillWeight = 14,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Font = Theme.BoldFont
            }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Note", DataPropertyName = nameof(Receipt.Note), FillWeight = 24
        });

        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditSelected(); };
    }

    /// <summary>Fills the category filter, with "All categories" as the first entry.</summary>
    private void LoadCategoryFilter()
    {
        List<Category> list = _categories.GetAll();
        list.Insert(0, new Category { CategoryId = 0, CategoryName = "All categories" });

        cboCategory.DataSource = list;
        cboCategory.DisplayMember = nameof(Category.CategoryName);
        cboCategory.ValueMember = nameof(Category.CategoryId);
    }

    /// <summary>Reads the filter controls, runs the search and shows the matching receipts.</summary>
    private void LoadReceipts()
    {
        ReceiptFilter filter = new ReceiptFilter { Keyword = txtKeyword.Text };

        if (dtpFrom.Checked) filter.From = dtpFrom.Value.Date;
        if (dtpTo.Checked) filter.To = dtpTo.Value.Date;
        if (cboCategory.SelectedValue is int id && id > 0) filter.CategoryId = id;

        if (!string.IsNullOrWhiteSpace(txtMin.Text))
        {
            if (!decimal.TryParse(txtMin.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal min))
            {
                MessageBox.Show("Minimum amount must be a number.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            filter.MinAmount = min;
        }

        if (!string.IsNullOrWhiteSpace(txtMax.Text))
        {
            if (!decimal.TryParse(txtMax.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal max))
            {
                MessageBox.Show("Maximum amount must be a number.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            filter.MaxAmount = max;
        }

        grid.DataSource = _receipts.Search(filter);
    }

    /// <summary>Updates the banner and the four summary cards with the current month's numbers.</summary>
    private void UpdateSummary()
    {
        DateTime today = DateTime.Today;
        MonthlyReport report = _reportService.GenerateReport(today.Month, today.Year);
        CultureInfo ci = CultureInfo.InvariantCulture;

        banner.Subtitle = "Personal spending tracker  -  " + ReportService.MonthName(report);

        cardTotal.Value = report.TotalSpent.ToString("N2", ci);
        cardTotal.Subtitle = report.ReceiptCount + " receipt(s) this month";

        cardDaily.Value = report.DailyAverage.ToString("N2", ci);
        cardDaily.Subtitle = "Average receipt " + report.AverageReceipt.ToString("N2", ci);

        CategoryTotal? top = report.Breakdown.FirstOrDefault();
        cardTop.Value = top?.CategoryName ?? "-";
        cardTop.Subtitle = top != null ? top.Percentage.ToString("0.#", ci) + "% of your spending" : "No receipts yet";

        Receipt? big = report.BiggestExpense;
        cardBiggest.Value = big != null ? big.Amount.ToString("N2", ci) : "-";
        cardBiggest.Subtitle = big != null
            ? big.StoreName + " on the " + ReportService.Ordinal(big.ReceiptDate.Day)
            : "No receipts yet";
    }

    /// <summary>Returns the receipt of the selected grid row, or null when nothing is selected.</summary>
    private Receipt? GetSelectedReceipt()
    {
        return grid.CurrentRow?.DataBoundItem as Receipt;
    }

    /// <summary>Opens the dialog to add a new receipt and saves it.</summary>
    private void AddReceipt()
    {
        Run(() =>
        {
            using ReceiptForm form = new ReceiptForm(_categories.GetAll());
            if (form.ShowDialog(this) != DialogResult.OK) return;

            _receipts.Add(form.Result);
            RefreshData();
        });
    }

    /// <summary>Opens the dialog to edit the selected receipt and saves the changes.</summary>
    private void EditSelected()
    {
        Receipt? selected = GetSelectedReceipt();
        if (selected == null)
        {
            MessageBox.Show("Select a receipt first.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Run(() =>
        {
            using ReceiptForm form = new ReceiptForm(_categories.GetAll(), selected);
            if (form.ShowDialog(this) != DialogResult.OK) return;

            _receipts.Update(form.Result);
            RefreshData();
        });
    }

    /// <summary>Deletes the selected receipt after confirmation.</summary>
    private void DeleteSelected()
    {
        Receipt? selected = GetSelectedReceipt();
        if (selected == null)
        {
            MessageBox.Show("Select a receipt first.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult answer = MessageBox.Show(
            "Delete the receipt from \"" + selected.StoreName + "\" (" + selected.Amount.ToString("N2") + ")?",
            "ReceiptRen", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        Run(() =>
        {
            _receipts.Delete(selected.ReceiptId);
            RefreshData();
        });
    }

    /// <summary>Opens the category manager, then refreshes the filter list and receipts.</summary>
    private void OpenCategories()
    {
        using CategoryForm form = new CategoryForm();
        form.ShowDialog(this);

        Run(() => { LoadCategoryFilter(); RefreshData(); });
    }

    /// <summary>Opens the monthly report window on the current month.</summary>
    private void OpenReport()
    {
        using ReportForm form = new ReportForm(DateTime.Today.Month, DateTime.Today.Year);
        form.ShowDialog(this);
    }

    /// <summary>Reloads the grid and the monthly summary after data changed.</summary>
    private void RefreshData()
    {
        LoadReceipts();
        UpdateSummary();
    }

    /// <summary>Runs an action and shows a clear message if the database cannot be reached.</summary>
    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (MySqlException ex)
        {
            MessageBox.Show("Database error: " + ex.Message + "\n\nMake sure MySQL is running in XAMPP and that " +
                            "database.sql has been imported.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
