using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedButton : Button
{
    private Color normalColor = Color.FromArgb(52, 58, 64);
    private Color hoverColor = Color.FromArgb(73, 80, 87);
    private Color selectedColor = Color.DodgerBlue;
    private bool isSelected = false;
    private int borderRadius = 10;

    public int BorderRadius
    {
        get => borderRadius;
        set { borderRadius = value; Invalidate(); }
    }

    public Color NormalColor
    {
        get => normalColor;
        set { normalColor = value; if (!isSelected && !IsMouseOver) BackColor = value; }
    }

    public Color HoverColor
    {
        get => hoverColor;
        set { hoverColor = value; if (IsMouseOver && !isSelected) BackColor = value; }
    }

    public Color SelectedColor
    {
        get => selectedColor;
        set { selectedColor = value; if (isSelected) BackColor = value; }
    }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected != value)
            {
                isSelected = value;
                UpdateBackColor();
            }
        }
    }

    private bool IsMouseOver = false;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = normalColor;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        Cursor = Cursors.Hand;
        this.MouseEnter += (s, e) => { IsMouseOver = true; UpdateBackColor(); };
        this.MouseLeave += (s, e) => { IsMouseOver = false; UpdateBackColor(); };
    }

    private void UpdateBackColor()
    {
        if (IsSelected)
        {
            // 选中状态：鼠标悬浮时用提亮色，否则用选中色
            BackColor = IsMouseOver ? ControlPaint.Light(selectedColor, 0.3f) : selectedColor;
        }
        else
        {
            // 未选中状态：根据鼠标悬浮决定
            BackColor = IsMouseOver ? hoverColor : normalColor;
        }
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(0, 0, borderRadius, borderRadius, 180, 90);
        path.AddArc(Width - borderRadius, 0, borderRadius, borderRadius, 270, 90);
        path.AddArc(Width - borderRadius, Height - borderRadius, borderRadius, borderRadius, 0, 90);
        path.AddArc(0, Height - borderRadius, borderRadius, borderRadius, 90, 90);
        this.Region = new Region(path);
        base.OnPaint(pevent);
    }
}