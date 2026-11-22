using PetClinicLab3.Controllers;

namespace PetClinicLab3.Views;

public class LoginForm : Form
{
    private readonly TextBox _txtUser = UITheme.MakeTextBox(50);
    private readonly TextBox _txtPass = UITheme.MakeTextBox(100);
    private readonly Label   _lblErr  = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public LoginForm()
    {
        Text            = "Pet Clinic — Login";
        Size            = new Size(380, 340);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        _txtPass.PasswordChar = '●';

        var header = UITheme.HeaderPanel("Pet Clinic", "Management System");

        var pnl = new TableLayoutPanel
        {
            RowCount    = 5,
            ColumnCount = 2,
            Dock        = DockStyle.Fill,
            Padding     = new Padding(32, 20, 32, 20),
        };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 5; i++)
            pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        void AddRow(int row, string label, Control ctrl)
        {
            pnl.Controls.Add(UITheme.MakeLabel(label), 0, row);
            ctrl.Dock = DockStyle.Fill;
            pnl.Controls.Add(ctrl, 1, row);
        }

        AddRow(0, "Username:", _txtUser);
        AddRow(1, "Password:", _txtPass);
        pnl.Controls.Add(_lblErr, 1, 2);

        var btnLogin = UITheme.TealButton("Login");
        btnLogin.Dock   = DockStyle.Fill;
        btnLogin.Click += OnLogin;
        pnl.Controls.Add(btnLogin, 1, 3);

        Controls.Add(pnl);
        Controls.Add(header);

        AcceptButton = btnLogin;
    }

    private void OnLogin(object? sender, EventArgs e)
    {
        _lblErr.Text = "";
        var ctrl = new UserController();
        var (ok, _) = ctrl.Login(_txtUser.Text, _txtPass.Text);
        if (!ok) { _lblErr.Text = "Invalid username or password."; return; }
        var main = new MainForm();
        main.FormClosed += (_, _) => Close();
        Hide();
        main.Show();
    }
}
