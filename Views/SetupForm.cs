using PetClinicLab3.Controllers;

namespace PetClinicLab3.Views;

public class SetupForm : Form
{
    private readonly TextBox _txtUsername = UITheme.MakeTextBox(50);
    private readonly TextBox _txtFullName = UITheme.MakeTextBox(100);
    private readonly TextBox _txtPassword = UITheme.MakeTextBox(100);
    private readonly TextBox _txtConfirm  = UITheme.MakeTextBox(100);
    private readonly Label   _lblError    = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public SetupForm()
    {
        Text            = "Pet Clinic — Initial Setup";
        Size            = new Size(440, 420);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        _txtPassword.PasswordChar = '●';
        _txtConfirm.PasswordChar  = '●';

        var header = UITheme.HeaderPanel("Initial Setup", "Create Administrator Account");

        var pnl = new TableLayoutPanel
        {
            RowCount = 6, ColumnCount = 2,
            Dock     = DockStyle.Fill,
            Padding  = new Padding(24),
        };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 6; i++)
            pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        void AddRow(int row, string label, Control ctrl)
        {
            pnl.Controls.Add(UITheme.MakeLabel(label), 0, row);
            ctrl.Dock = DockStyle.Fill;
            pnl.Controls.Add(ctrl, 1, row);
        }

        AddRow(0, "Username:", _txtUsername);
        AddRow(1, "Full Name:", _txtFullName);
        AddRow(2, "Password:", _txtPassword);
        AddRow(3, "Confirm:", _txtConfirm);
        pnl.Controls.Add(_lblError, 1, 4);

        var btn = UITheme.TealButton("Create Admin Account");
        btn.Dock   = DockStyle.Fill;
        btn.Click += OnCreate;
        pnl.Controls.Add(btn, 1, 5);

        Controls.Add(pnl);
        Controls.Add(header);
    }

    private void OnCreate(object? sender, EventArgs e)
    {
        _lblError.Text = "";
        if (_txtPassword.Text != _txtConfirm.Text)
        {
            _lblError.Text = "Passwords do not match.";
            return;
        }
        var ctrl = new UserController();
        var (ok, err) = ctrl.CreateAdmin(_txtUsername.Text, _txtFullName.Text, _txtPassword.Text);
        if (!ok) { _lblError.Text = err; return; }
        MessageBox.Show("Administrator account created. You can now log in.", "Success",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}
