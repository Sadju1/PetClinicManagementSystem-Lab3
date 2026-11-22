namespace PetClinicLab3.Views;

internal static class UITheme
{
    public static readonly Color Navy      = Color.FromArgb(26, 54, 93);
    public static readonly Color Teal      = Color.FromArgb(32, 178, 170);
    public static readonly Color LightGray = Color.FromArgb(245, 245, 245);
    public static readonly Color White     = Color.White;
    public static readonly Color DarkText  = Color.FromArgb(30, 30, 30);
    public static readonly Color ErrorRed  = Color.FromArgb(192, 0, 0);
    public static readonly Color SuccessGreen = Color.FromArgb(0, 128, 64);
    public static readonly Font  TitleFont  = new("Segoe UI", 16f, FontStyle.Bold);
    public static readonly Font  LabelFont  = new("Segoe UI", 10f);
    public static readonly Font  InputFont  = new("Segoe UI", 10f);
    public static readonly Font  ButtonFont = new("Segoe UI", 10f, FontStyle.Bold);

    public static Button MakeButton(string text, Color back, Color fore)
    {
        return new Button
        {
            Text      = text,
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            Font      = ButtonFont,
            Cursor    = Cursors.Hand,
            Height    = 36,
        };
    }

    public static Button NavyButton(string text) => MakeButton(text, Navy, White);
    public static Button TealButton(string text)  => MakeButton(text, Teal, White);

    public static Label MakeLabel(string text, Font? font = null) => new()
    {
        Text      = text,
        Font      = font ?? LabelFont,
        ForeColor = DarkText,
        AutoSize  = true,
    };

    public static TextBox MakeTextBox(int maxLen = 200) => new()
    {
        Font      = InputFont,
        MaxLength = maxLen,
        Height    = 28,
        ImeMode   = ImeMode.Disable,
    };

    public static Panel HeaderPanel(string title, string subtitle = "")
    {
        var panel = new Panel
        {
            BackColor = Navy,
            Dock      = DockStyle.Top,
            Height    = 70,
            Padding   = new Padding(16, 10, 16, 10),
        };
        var lbl = new Label
        {
            Text      = title,
            Font      = TitleFont,
            ForeColor = White,
            AutoSize  = false,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if (!string.IsNullOrEmpty(subtitle))
            lbl.Text = $"{title}  —  {subtitle}";
        panel.Controls.Add(lbl);
        return panel;
    }
}
