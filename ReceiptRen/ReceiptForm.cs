using System.Globalization;

namespace ReceiptRen;

/// <summary>
/// Dialog used to add a new receipt or edit an existing one.
/// After the dialog returns DialogResult.OK, read the finished receipt from the Result property.
/// </summary>
public class ReceiptForm : Form
{
    private readonly DateTimePicker dtpDate = new DateTimePicker();
    private readonly TextBox txtStore = new TextBox();
    private readonly ComboBox cboCategory = new ComboBox();
    private readonly TextBox txtAmount = new TextBox();
    private readonly TextBox txtNote = new TextBox();
    private readonly Button btnSave = new Button();
    private readonly Button btnCancel = new Button();
    private readonly int _receiptId;

    /// <summary>The receipt filled in by the user (valid after the dialog closes with OK).</summary>
    public Receipt Result { get; private set; } = new Receipt();

    /// <summary>Opens the dialog. Pass an existing receipt to edit it, or null to add a new one.</summary>
    public ReceiptForm(List<Category> categories, Receipt? existing = null)
    {
        Text = existing == null ? "Add Receipt" : "Edit Receipt";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(440, 340);
        Theme.StyleForm(this);

        GradientPanel banner = new GradientPanel
        {
            Dock = DockStyle.Fill,
            Title = existing == null ? "Add receipt" : "Edit receipt",
            TitleSize = 15f
        };

        Panel body = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };

        TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(banner, 0, 0);
        layout.Controls.Add(body, 0, 1);
        Controls.Add(layout);

        AddRow(body, "Date", dtpDate, 20);
        AddRow(body, "Store / description", txtStore, 62);
        AddRow(body, "Category", cboCategory, 104);
        AddRow(body, "Amount", txtAmount, 146);
        AddRow(body, "Note (optional)", txtNote, 188);

        dtpDate.Format = DateTimePickerFormat.Short;
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCategory.DataSource = categories;
        cboCategory.DisplayMember = nameof(Category.CategoryName);
        cboCategory.ValueMember = nameof(Category.CategoryId);

        btnSave.Text = "Save";
        btnSave.SetBounds(215, 232, 100, 36);
        Theme.StyleButton(btnSave, Theme.Green);
        btnSave.Click += BtnSave_Click;

        btnCancel.Text = "Cancel";
        btnCancel.SetBounds(325, 232, 100, 36);
        Theme.StyleButton(btnCancel, Theme.Grey);
        btnCancel.DialogResult = DialogResult.Cancel;

        body.Controls.Add(btnSave);
        body.Controls.Add(btnCancel);
        AcceptButton = btnSave;
        CancelButton = btnCancel;

        if (existing != null)
        {
            _receiptId = existing.ReceiptId;
            dtpDate.Value = existing.ReceiptDate;
            txtStore.Text = existing.StoreName;
            cboCategory.SelectedValue = existing.CategoryId;
            txtAmount.Text = existing.Amount.ToString("0.00", CultureInfo.CurrentCulture);
            txtNote.Text = existing.Note;
        }
    }

    /// <summary>Places a label and its input control on one row of the dialog body.</summary>
    private static void AddRow(Panel body, string label, Control input, int top)
    {
        body.Controls.Add(new Label { Text = label, Left = 18, Top = top + 4, Width = 130, ForeColor = Theme.Muted });
        input.SetBounds(155, top, 265, 28);
        body.Controls.Add(input);
    }

    /// <summary>Validates the input and closes the dialog with OK when everything is correct.</summary>
    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!decimal.TryParse(txtAmount.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount))
        {
            MessageBox.Show("Amount must be a number.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtAmount.Focus();
            return;
        }

        Receipt receipt = new Receipt
        {
            ReceiptId = _receiptId,
            ReceiptDate = dtpDate.Value.Date,
            StoreName = txtStore.Text.Trim(),
            CategoryId = cboCategory.SelectedValue is int id ? id : 0,
            Amount = amount,
            Note = txtNote.Text.Trim()
        };

        string? error = ReceiptController.Validate(receipt);
        if (error != null)
        {
            MessageBox.Show(error, "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // A future date is allowed, but the user is warned and can go back and fix it.
        if (receipt.ReceiptDate > DateTime.Today)
        {
            DialogResult answer = MessageBox.Show(
                "This date is in the future. Save the receipt anyway?", "ReceiptRen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
        }

        Result = receipt;
        DialogResult = DialogResult.OK;
    }
}
