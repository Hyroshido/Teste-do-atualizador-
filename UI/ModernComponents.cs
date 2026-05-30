namespace DataSmartUpdater.UI;

public class ModernButton : Button
{
    private bool _isHovering;

    public ModernButton()
    {
        FlatStyle = FlatStyle.Flat;
        Font = new Font("Segoe UI", 9, FontStyle.Bold);
        Cursor = Cursors.Hand;
        Height = 36;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = ModernColors.BackgroundHover;
        FlatAppearance.MouseDownBackColor = ModernColors.PrimaryDark;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovering = true;
        BackColor = ModernColors.BackgroundHover;
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovering = false;
        BackColor = ModernColors.BackgroundCard;
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_isHovering)
        {
            var rect = ClientRectangle;
            rect.Inflate(-1, -1);
            using var pen = new Pen(ModernColors.Primary);
            e.Graphics.DrawRectangle(pen, rect);
        }
        
        if (Focused)
        {
            var rect = ClientRectangle;
            rect.Inflate(-2, -2);
            ControlPaint.DrawFocusRectangle(e.Graphics, rect, ModernColors.Primary, ModernColors.BackgroundCard);
        }
    }
}

public class ModernPrimaryButton : ModernButton
{
    public ModernPrimaryButton()
    {
        BackColor = ModernColors.Primary;
        ForeColor = ModernColors.TextPrimary;
    }
}

public class ModernSecondaryButton : ModernButton
{
    public ModernSecondaryButton()
    {
        BackColor = ModernColors.BackgroundCard;
        ForeColor = ModernColors.TextPrimary;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = ModernColors.BorderLight;
    }
}

public class ModernCard : Panel
{
    public ModernCard()
    {
        BackColor = ModernColors.BackgroundCard;
        Padding = new Padding(16);
        Margin = new Padding(8);
    }
}

public class ModernLabel : Label
{
    public ModernLabel()
    {
        Font = new Font("Segoe UI", 9);
        ForeColor = ModernColors.TextSecondary;
        AutoSize = true;
    }
}

public class ModernTitleLabel : Label
{
    public ModernTitleLabel()
    {
        Font = new Font("Segoe UI", 12, FontStyle.Bold);
        ForeColor = ModernColors.Primary;
        AutoSize = true;
    }
}
