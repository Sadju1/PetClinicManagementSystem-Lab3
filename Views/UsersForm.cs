using PetClinicLab3.Controllers;
using PetClinicLab3.Models;
using PetClinicLab3.Services;

namespace PetClinicLab3.Views;

public class UsersForm : Form
{
    private readonly UserController _ctrl = new();
    private readonly DataGridView   _grid = new();
    private List<User>              _users = new();

    public UsersForm()
    {
        BackColor = UITheme.LightGray;
        var _hdr = UITheme.HeaderPanel("Users", "Manage System Accounts");

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 10, 16, 10), BackColor = UITheme.White };
        var btnNew = UITheme.TealButton("+ New User");
        btnNew.Width = 120; btnNew.Click += (_, _) => OpenDetail(null);
        toolbar.Controls.Add(btnNew);

        var btnPwd = UITheme.NavyButton("Change My Password");
        btnPwd.Width = 170; btnPwd.Click += (_, _) =>
        {
            new ChangePasswordForm().ShowDialog();
        };
        toolbar.Controls.Add(btnPwd);

        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AllowUserToAddRows = false;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = UITheme.White;
        _grid.Font = UITheme.LabelFont;
        _grid.DoubleClick += (_, _) => OpenSelected();

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(_hdr);

        Load += (_, _) => LoadGrid();
    }

    private void LoadGrid()
    {
        _users = _ctrl.GetAll();
        _grid.DataSource = _users.Select(u => new
        {
            u.UserID, u.Username, u.FullName, u.Role,
            Status = u.IsActive ? "Active" : "Inactive",
        }).ToList();
        if (_grid.Columns.Contains("UserID")) _grid.Columns["UserID"]!.Visible = false;
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow == null) return;
        int id = (int)_grid.CurrentRow.Cells["UserID"].Value;
        OpenDetail(_ctrl.GetById(id));
    }

    private void OpenDetail(User? u)
    {
        var dlg = new UserDetailForm(u);
        if (dlg.ShowDialog() == DialogResult.OK) LoadGrid();
    }
}

public class UserDetailForm : Form
{
    private readonly UserController _ctrl = new();
    private User _user;

    private readonly TextBox  _txtUsername = UITheme.MakeTextBox(50);
    private readonly TextBox  _txtFullName = UITheme.MakeTextBox(100);
    private readonly ComboBox _cboRole     = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont };
    private readonly TextBox  _txtPassword = UITheme.MakeTextBox(100);
    private readonly CheckBox _chkActive   = new() { Text = "Active", Checked = true, Font = UITheme.LabelFont };
    private readonly Label    _lblError    = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public UserDetailForm(User? user)
    {
        _user           = user ?? new User { IsActive = true, Role = "Receptionist" };
        bool isNew      = _user.UserID == 0;
        Text            = isNew ? "New User" : $"Edit User — {_user.Username}";
        Size            = new Size(480, 580);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        var _userHeader = UITheme.HeaderPanel(isNew ? "New User" : "Edit User");
        _txtPassword.PasswordChar = '●';
        _cboRole.Items.AddRange(new[] { "Administrator", "Veterinarian", "Receptionist" });

        var pnl = new TableLayoutPanel { RowCount = 9, ColumnCount = 2, Dock = DockStyle.Fill, Padding = new Padding(24) };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 9; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        void Add(int r, string lbl, Control ctrl) { pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r); ctrl.Dock = DockStyle.Fill; pnl.Controls.Add(ctrl, 1, r); }

        Add(0, "Username*:", _txtUsername);
        Add(1, "Full Name*:", _txtFullName);
        Add(2, "Role*:", _cboRole);
        Add(3, isNew ? "Password*:" : "New Password:", _txtPassword);
        pnl.Controls.Add(_chkActive, 1, 4);
        pnl.Controls.Add(_lblError, 1, 5);

        var btnSave = UITheme.TealButton("Save");
        btnSave.Dock = DockStyle.Fill; btnSave.Click += OnSave;
        pnl.Controls.Add(btnSave, 1, 7);

        Controls.Add(pnl);
        Controls.Add(_userHeader);

        _txtUsername.Text  = _user.Username;
        _txtFullName.Text  = _user.FullName;
        _cboRole.SelectedItem = _user.Role;
        _chkActive.Checked = _user.IsActive;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text    = "";
        _user.Username    = _txtUsername.Text;
        _user.FullName    = _txtFullName.Text;
        _user.Role        = _cboRole.SelectedItem?.ToString() ?? "";
        _user.IsActive    = _chkActive.Checked;
        string? newPwd    = string.IsNullOrWhiteSpace(_txtPassword.Text) ? null : _txtPassword.Text;
        var (ok, err)     = _ctrl.Save(_user, newPwd);
        if (!ok) { _lblError.Text = err; return; }
        DialogResult = DialogResult.OK;
        Close();
    }
}

public class ChangePasswordForm : Form
{
    private readonly UserController _ctrl = new();
    private readonly TextBox _txtCurrent = UITheme.MakeTextBox(100);
    private readonly TextBox _txtNew     = UITheme.MakeTextBox(100);
    private readonly TextBox _txtConfirm = UITheme.MakeTextBox(100);
    private readonly Label   _lblError   = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public ChangePasswordForm()
    {
        Text            = "Change Password";
        Size            = new Size(440, 480);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        _txtCurrent.PasswordChar = '●';
        _txtNew.PasswordChar     = '●';
        _txtConfirm.PasswordChar = '●';

        var _pwdHeader = UITheme.HeaderPanel("Change Password");

        var pnl = new TableLayoutPanel { RowCount = 7, ColumnCount = 2, Dock = DockStyle.Fill, Padding = new Padding(24) };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 7; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        void Add(int r, string lbl, Control ctrl) { pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r); ctrl.Dock = DockStyle.Fill; pnl.Controls.Add(ctrl, 1, r); }

        Add(0, "Current:", _txtCurrent);
        Add(1, "New Password:", _txtNew);
        Add(2, "Confirm:", _txtConfirm);
        pnl.Controls.Add(_lblError, 1, 3);

        var btnSave = UITheme.TealButton("Save");
        btnSave.Dock = DockStyle.Fill; btnSave.Click += OnSave;
        pnl.Controls.Add(btnSave, 1, 5);

        Controls.Add(pnl);
        Controls.Add(_pwdHeader);
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text = "";
        if (_txtNew.Text != _txtConfirm.Text) { _lblError.Text = "Passwords do not match."; return; }
        var (ok, err) = _ctrl.ChangeOwnPassword(_txtCurrent.Text, _txtNew.Text);
        if (!ok) { _lblError.Text = err; return; }
        MessageBox.Show("Password changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}
