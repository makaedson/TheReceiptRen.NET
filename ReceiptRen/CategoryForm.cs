using MySqlConnector;

namespace ReceiptRen;

/// <summary>
/// Dialog for managing categories: add, rename and remove.
/// A category that is used by receipts cannot be removed.
/// </summary>
public class CategoryForm : Form
{
    private readonly CategoryController _controller = new CategoryController();
    private readonly ListBox lstCategories = new ListBox();
    private readonly TextBox txtName = new TextBox();
    private readonly Button btnAdd = new Button();
    private readonly Button btnRename = new Button();
    private readonly Button btnDelete = new Button();
    private readonly Button btnClose = new Button();

    /// <summary>Builds the dialog and loads the categories.</summary>
    public CategoryForm()
    {
        Text = "Manage Categories";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(500, 380);
        Theme.StyleForm(this);

        GradientPanel banner = new GradientPanel { Dock = DockStyle.Fill, Title = "Categories", TitleSize = 15f };
        Panel body = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };

        TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(banner, 0, 0);
        layout.Controls.Add(body, 0, 1);
        Controls.Add(layout);

        lstCategories.SetBounds(18, 18, 220, 250);
        lstCategories.BorderStyle = BorderStyle.FixedSingle;
        lstCategories.ItemHeight = 24;
        lstCategories.DisplayMember = nameof(Category.CategoryName);
        lstCategories.SelectedIndexChanged += (s, e) =>
        {
            if (lstCategories.SelectedItem is Category c) txtName.Text = c.CategoryName;
        };

        body.Controls.Add(new Label { Text = "Category name", Left = 258, Top = 18, Width = 220, ForeColor = Theme.Muted });
        txtName.SetBounds(258, 42, 220, 28);
        txtName.MaxLength = 80;

        btnAdd.Text = "Add";
        btnAdd.SetBounds(258, 86, 220, 38);
        Theme.StyleButton(btnAdd, Theme.Green);
        btnAdd.Click += BtnAdd_Click;

        btnRename.Text = "Rename selected";
        btnRename.SetBounds(258, 132, 220, 38);
        Theme.StyleButton(btnRename, Theme.Blue);
        btnRename.Click += BtnRename_Click;

        btnDelete.Text = "Delete selected";
        btnDelete.SetBounds(258, 178, 220, 38);
        Theme.StyleButton(btnDelete, Theme.Red);
        btnDelete.Click += BtnDelete_Click;

        btnClose.Text = "Close";
        btnClose.SetBounds(378, 230, 100, 38);
        Theme.StyleButton(btnClose, Theme.Grey);
        btnClose.DialogResult = DialogResult.OK;

        body.Controls.AddRange(new Control[] { lstCategories, txtName, btnAdd, btnRename, btnDelete, btnClose });
        CancelButton = btnClose;

        Load += (s, e) => Run(() => LoadCategories());
    }

    /// <summary>Reloads the list from the database.</summary>
    private void LoadCategories()
    {
        lstCategories.DataSource = _controller.GetAll();
        lstCategories.DisplayMember = nameof(Category.CategoryName);
    }

    /// <summary>Adds the category typed in the text box.</summary>
    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        Run(() =>
        {
            _controller.Add(txtName.Text);
            LoadCategories();
            txtName.Clear();
        });
    }

    /// <summary>Renames the selected category to the text in the text box.</summary>
    private void BtnRename_Click(object? sender, EventArgs e)
    {
        if (lstCategories.SelectedItem is not Category selected)
        {
            MessageBox.Show("Select a category first.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Run(() =>
        {
            _controller.Rename(selected.CategoryId, txtName.Text);
            LoadCategories();
        });
    }

    /// <summary>Deletes the selected category after confirmation (refused if it is in use).</summary>
    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (lstCategories.SelectedItem is not Category selected)
        {
            MessageBox.Show("Select a category first.", "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult answer = MessageBox.Show(
            "Delete the category \"" + selected.CategoryName + "\"?", "ReceiptRen",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        Run(() =>
        {
            _controller.Delete(selected.CategoryId);
            LoadCategories();
            txtName.Clear();
        });
    }

    /// <summary>Runs an action and shows a clear message if validation or the database fails.</summary>
    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (MySqlException ex)
        {
            MessageBox.Show("Database error: " + ex.Message + "\nIs MySQL running in XAMPP?",
                "ReceiptRen", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
