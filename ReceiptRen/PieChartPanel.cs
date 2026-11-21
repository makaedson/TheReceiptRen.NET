namespace ReceiptRen;

/// <summary>
/// Small custom control that draws a pie chart of category spending with GDI+,
/// plus a colour legend. Call SetData to change what it shows.
/// </summary>
public class PieChartPanel : Panel
{
    private static readonly Color[] Palette =
    {
        Color.FromArgb(0x25, 0x63, 0xEB), Color.FromArgb(0x10, 0xB9, 0x81), Color.FromArgb(0xF5, 0x9E, 0x0B),
        Color.FromArgb(0xEF, 0x44, 0x44), Color.FromArgb(0x8B, 0x5C, 0xF6), Color.FromArgb(0x06, 0xB6, 0xD4),
        Color.FromArgb(0xEC, 0x48, 0x99), Color.FromArgb(0x84, 0xCC, 0x16), Color.FromArgb(0x64, 0x74, 0x8B)
    };

    private List<CategoryTotal> _data = new List<CategoryTotal>();

    /// <summary>Creates the panel with double buffering so the chart does not flicker.</summary>
    public PieChartPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    /// <summary>Replaces the chart data and redraws it.</summary>
    public void SetData(IEnumerable<CategoryTotal> data)
    {
        _data = data.ToList();
        Invalidate();
    }

    /// <summary>Draws the pie on the left and the legend on the right.</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        if (_data.Count == 0 || _data.Sum(d => d.Amount) <= 0)
        {
            using StringFormat center = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString("No spending to show", Font, Brushes.Gray, ClientRectangle, center);
            return;
        }

        int diameter = Math.Min(Height - 20, (int)(Width * 0.55)) - 10;
        if (diameter < 20) return;

        Rectangle pie = new Rectangle(10, (Height - diameter) / 2, diameter, diameter);
        float startAngle = -90f;

        for (int i = 0; i < _data.Count; i++)
        {
            float sweep = (float)(_data[i].Percentage * 3.6);
            using SolidBrush brush = new SolidBrush(Palette[i % Palette.Length]);
            g.FillPie(brush, pie, startAngle, sweep);
            using Pen separator = new Pen(Color.White, 2);
            g.DrawPie(separator, pie, startAngle, sweep);
            startAngle += sweep;
        }

        // Donut hole with the total in the middle.
        int hole = (int)(diameter * 0.55);
        Rectangle holeRect = new Rectangle(pie.X + (diameter - hole) / 2, pie.Y + (diameter - hole) / 2, hole, hole);
        g.FillEllipse(Brushes.White, holeRect);
        using (StringFormat centered = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        using (Font totalFont = new Font("Segoe UI", 10f, FontStyle.Bold))
        {
            g.DrawString("Total\n" + _data.Sum(d => d.Amount).ToString("N0"), totalFont, Brushes.Black, holeRect, centered);
        }

        // Legend: colour box, category name and percentage.
        int legendX = pie.Right + 20;
        int lineHeight = Font.Height + 8;
        int box = Font.Height - 2;
        int legendY = Math.Max(10, (Height - _data.Count * lineHeight) / 2);

        for (int i = 0; i < _data.Count; i++)
        {
            using SolidBrush brush = new SolidBrush(Palette[i % Palette.Length]);
            g.FillRectangle(brush, legendX, legendY + i * lineHeight + 2, box, box);
            g.DrawString(_data[i].CategoryName + " (" + _data[i].Percentage.ToString("0.#") + "%)",
                Font, Brushes.Black, legendX + box + 8, legendY + i * lineHeight);
        }
    }
}
