using System.Drawing.Drawing2D;

namespace ReceiptRen;

/// <summary>
/// Central place for the colours, fonts and styling helpers used by every window,
/// so the whole application has one consistent look.
/// </summary>
public static class Theme
{
    /// <summary>Main brand colour (buttons, banner start).</summary>
    public static readonly Color Blue = Color.FromArgb(0x25, 0x63, 0xEB);

    /// <summary>Positive colour (banner end, save buttons, decreases in spending).</summary>
    public static readonly Color Green = Color.FromArgb(0x10, 0xB9, 0x81);

    /// <summary>Warning / highlight colour.</summary>
    public static readonly Color Orange = Color.FromArgb(0xF5, 0x9E, 0x0B);

    /// <summary>Accent colour for category-related items.</summary>
    public static readonly Color Purple = Color.FromArgb(0x8B, 0x5C, 0xF6);

    /// <summary>Danger colour (delete buttons, increases in spending).</summary>
    public static readonly Color Red = Color.FromArgb(0xEF, 0x44, 0x44);

    /// <summary>Dark colour used for table headers.</summary>
    public static readonly Color Navy = Color.FromArgb(0x1E, 0x29, 0x3B);

    /// <summary>Neutral grey used for secondary buttons.</summary>
    public static readonly Color Grey = Color.FromArgb(0x64, 0x74, 0x8B);

    /// <summary>Window background.</summary>
    public static readonly Color Background = Color.FromArgb(0xF1, 0xF5, 0xF9);

    /// <summary>Background of cards and tables.</summary>
    public static readonly Color Surface = Color.White;

    /// <summary>Thin border and grid line colour.</summary>
    public static readonly Color Border = Color.FromArgb(0xE2, 0xE8, 0xF0);

    /// <summary>Main text colour.</summary>
    public static readonly Color TextDark = Color.FromArgb(0x0F, 0x17, 0x2A);

    /// <summary>Secondary text colour.</summary>
    public static readonly Color Muted = Color.FromArgb(0x64, 0x74, 0x8B);

    /// <summary>Regular font used by all windows.</summary>
    public static readonly Font BodyFont = new Font("Segoe UI", 10f);

    /// <summary>Bold version of the regular font.</summary>
    public static readonly Font BoldFont = new Font("Segoe UI", 10f, FontStyle.Bold);

    /// <summary>Applies the shared window look: background colour and font.</summary>
    public static void StyleForm(Form form)
    {
        form.BackColor = Background;
        form.Font = BodyFont;
    }

    /// <summary>Turns a button into a flat, coloured button with a hover effect.</summary>
    public static void StyleButton(Button button, Color back)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(back, 0.1f);
        button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(back, 0.2f);
        button.BackColor = back;
        button.ForeColor = Color.White;
        button.Font = BoldFont;
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    /// <summary>Gives a DataGridView a modern flat look: dark header, striped rows, light selection.</summary>
    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Border;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 40;

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Navy,
            ForeColor = Color.White,
            Font = BoldFont,
            SelectionBackColor = Navy,
            SelectionForeColor = Color.White,
            Padding = new Padding(6, 0, 0, 0),
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = TextDark,
            Font = BodyFont,
            SelectionBackColor = Color.FromArgb(0xDB, 0xEA, 0xFE),
            SelectionForeColor = TextDark,
            Padding = new Padding(6, 0, 6, 0)
        };

        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(0xF8, 0xFA, 0xFC)
        };

        grid.RowTemplate.Height = 34;
    }

    /// <summary>Gives a details-view ListView a dark header that matches the grids.</summary>
    public static void StyleList(ListView list)
    {
        list.BorderStyle = BorderStyle.None;
        list.BackColor = Surface;
        list.OwnerDraw = true;

        list.DrawColumnHeader += (s, e) =>
        {
            using SolidBrush brush = new SolidBrush(Navy);
            e.Graphics.FillRectangle(brush, e.Bounds);

            Rectangle textBounds = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? "", BoldFont, textBounds, Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        };
        list.DrawItem += (s, e) => e.DrawDefault = true;
        list.DrawSubItem += (s, e) => e.DrawDefault = true;
    }
}

/// <summary>
/// Banner with a blue-to-green gradient, a bold title and an optional subtitle.
/// Used at the top of every window.
/// </summary>
public class GradientPanel : Panel
{
    private string _title = "";
    private string _subtitle = "";

    /// <summary>Large white text of the banner.</summary>
    public string Title
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    /// <summary>Smaller text under the title (may be empty).</summary>
    public string Subtitle
    {
        get => _subtitle;
        set { _subtitle = value; Invalidate(); }
    }

    /// <summary>Size in points of the title text.</summary>
    public float TitleSize { get; set; } = 20f;

    /// <summary>Creates the banner with double buffering to avoid flicker.</summary>
    public GradientPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    /// <summary>
    /// Height the banner needs so that the title (and subtitle) are never cut off,
    /// whatever the screen scaling is.
    /// </summary>
    public static int PreferredHeight(float titleSize, bool hasSubtitle)
    {
        using Font titleFont = new Font("Segoe UI", titleSize, FontStyle.Bold);
        using Font subFont = new Font("Segoe UI", 10f);
        return titleFont.Height + (hasSubtitle ? subFont.Height : 0) + 24;
    }

    /// <summary>Paints the gradient and the texts.</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width <= 0 || Height <= 0) return;

        using (LinearGradientBrush brush = new LinearGradientBrush(
                   ClientRectangle, Theme.Blue, Theme.Green, LinearGradientMode.Horizontal))
        {
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        using Font titleFont = new Font("Segoe UI", TitleSize, FontStyle.Bold);
        using Font subFont = new Font("Segoe UI", 10f);

        const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
        int titleHeight = titleFont.Height;
        int subHeight = string.IsNullOrEmpty(_subtitle) ? 0 : subFont.Height;
        int y = Math.Max(2, (Height - titleHeight - subHeight) / 2);

        TextRenderer.DrawText(e.Graphics, _title, titleFont,
            new Rectangle(18, y, Width - 36, titleHeight + 2), Color.White, flags);
        if (subHeight > 0)
            TextRenderer.DrawText(e.Graphics, _subtitle, subFont,
                new Rectangle(20, y + titleHeight, Width - 40, subHeight + 2), Color.FromArgb(0xE0, 0xF2, 0xFE), flags);
    }
}

/// <summary>
/// Small dashboard card: coloured accent bar, a title, a big value and a small subtitle.
/// </summary>
public class SummaryCard : Panel
{
    private string _title = "";
    private string _value = "-";
    private string _subtitle = "";
    private Color _accent = Theme.Blue;

    /// <summary>Small grey label at the top of the card.</summary>
    public string Title { get => _title; set { _title = value; Invalidate(); } }

    /// <summary>Big number or text in the accent colour.</summary>
    public string Value { get => _value; set { _value = value; Invalidate(); } }

    /// <summary>Small grey line under the value.</summary>
    public string Subtitle { get => _subtitle; set { _subtitle = value; Invalidate(); } }

    /// <summary>Colour of the left bar and of the value.</summary>
    public Color Accent { get => _accent; set { _accent = value; Invalidate(); } }

    /// <summary>Creates the card with a white background and double buffering.</summary>
    public SummaryCard()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Theme.Surface;
    }

    /// <summary>
    /// Height the card needs so that its three lines of text are never cut off,
    /// whatever the screen scaling is.
    /// </summary>
    public static int PreferredHeight
    {
        get
        {
            using Font titleFont = new Font("Segoe UI", 9.5f);
            using Font valueFont = new Font("Segoe UI", 19f, FontStyle.Bold);
            using Font subFont = new Font("Segoe UI", 9f);
            return 10 + titleFont.Height + valueFont.Height + subFont.Height + 10;
        }
    }

    /// <summary>Paints the border, accent bar and the three texts.</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        using (Pen border = new Pen(Theme.Border))
            g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        using (SolidBrush bar = new SolidBrush(_accent))
            g.FillRectangle(bar, 0, 0, 6, Height);

        const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
                                      TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
        int textWidth = Math.Max(10, Width - 28);

        using Font titleFont = new Font("Segoe UI", 9.5f);
        using Font valueFont = new Font("Segoe UI", 19f, FontStyle.Bold);
        using Font subFont = new Font("Segoe UI", 9f);

        // Each line sits directly under the previous one, using the real font heights.
        int y = 10;
        TextRenderer.DrawText(g, _title, titleFont, new Rectangle(18, y, textWidth, titleFont.Height + 2), Theme.Muted, flags);
        y += titleFont.Height;
        TextRenderer.DrawText(g, _value, valueFont, new Rectangle(18, y, textWidth, valueFont.Height + 2), _accent, flags);
        y += valueFont.Height;
        TextRenderer.DrawText(g, _subtitle, subFont, new Rectangle(18, y, textWidth, subFont.Height + 2), Theme.Muted, flags);
    }
}

/// <summary>
/// White panel with a bold title on top and any control (table, chart, list) filling the rest.
/// </summary>
public class TitledCard : Panel
{
    /// <summary>Creates the card around the given content control.</summary>
    public TitledCard(string title, Control content)
    {
        BackColor = Theme.Surface;
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        Padding = new Padding(12, 8, 12, 12);

        TableLayoutPanel table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.Surface
        };
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Label label = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Font = Theme.BoldFont,
            ForeColor = Theme.TextDark,
            TextAlign = ContentAlignment.MiddleLeft
        };

        content.Dock = DockStyle.Fill;
        table.Controls.Add(label, 0, 0);
        table.Controls.Add(content, 0, 1);
        Controls.Add(table);
    }
}
