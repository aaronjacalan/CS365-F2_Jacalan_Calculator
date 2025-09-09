using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class GradientButton : Button
{
    public Color ColorTop { get; set; } = Color.FromArgb(152, 43, 237);
    public Color ColorBottom { get; set; } = Color.FromArgb(89, 0, 179);

    protected override void OnPaint(PaintEventArgs e)
    {
        using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, ColorTop, ColorBottom, 90F))
        {
            e.Graphics.FillRectangle(brush, this.ClientRectangle);
        }

        // Draw text
        TextRenderer.DrawText(e.Graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor, 
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
