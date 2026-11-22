using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class OwnerDetailForm : Form
{
    private readonly OwnerController _ctrl  = new();
    private Owner                    _owner;
    private readonly TextBox _txtFirst   = UITheme.MakeTextBox(50);
    private readonly TextBox _txtLast    = UITheme.MakeTextBox(50);
    private readonly TextBox _txtPhone   = UITheme.MakeTextBox(20);
    private readonly TextBox _txtEmail   = UITheme.MakeTextBox(100);
    private readonly TextBox _txtAddress = UITheme.MakeTextBox(255);
    private readonly Label   _lblError   = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public OwnerDetailForm(Owner? owner)
    {
        _owner          = owner ?? new Owner { IsActive = true };
        bool isNew      = _owner.OwnerID == 0;
        Text            = isNew ? "New Owner" : $"Edit Owner — {_owner.FullName}";
        Size            = new Size(480, 540);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        var header = UITheme.HeaderPanel(isNew ? "New Owner" : "Edit Owner");

        var pnl = new TableLayoutPanel
        {
            RowCount    = 8,
            ColumnCount = 2,
            Dock        = DockStyle.Fill,
            Padding     = new Padding(24),
        };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 8; i++)
            pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        void Add(int r, string lbl, Control ctrl)
        {
            pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r);
            ctrl.Dock = DockStyle.Fill;
            pnl.Controls.Add(ctrl, 1, r);
        }

        Add(0, "First Name*:", _txtFirst);
        Add(1, "Last Name*:", _txtLast);
        Add(2, "Phone:", _txtPhone);
        Add(3, "Email:", _txtEmail);
        Add(4, "Address:", _txtAddress);
        pnl.Controls.Add(_lblError, 1, 5);

        var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var btnSave = UITheme.TealButton("Save");
        btnSave.Width  = 110;
        btnSave.Click += OnSave;
        btnFlow.Controls.Add(btnSave);

        if (!isNew)
        {
            var btnDeact = UITheme.MakeButton("Deactivate", UITheme.ErrorRed, UITheme.White);
            btnDeact.Width  = 120;
            btnDeact.Click += OnDeactivate;
            btnFlow.Controls.Add(btnDeact);
        }

        pnl.Controls.Add(btnFlow, 1, 6);

        Controls.Add(pnl);
        Controls.Add(header);

        Populate();
    }

    private void Populate()
    {
        _txtFirst.Text   = _owner.FirstName;
        _txtLast.Text    = _owner.LastName;
        _txtPhone.Text   = _owner.Phone   ?? "";
        _txtEmail.Text   = _owner.Email   ?? "";
        _txtAddress.Text = _owner.Address ?? "";
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text = "";
        _owner.FirstName = _txtFirst.Text;
        _owner.LastName  = _txtLast.Text;
        _owner.Phone     = _txtPhone.Text;
        _owner.Email     = _txtEmail.Text;
        _owner.Address   = _txtAddress.Text;
        var (ok, err) = _ctrl.Save(_owner);
        if (!ok) { _lblError.Text = err; return; }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnDeactivate(object? sender, EventArgs e)
    {
        if (MessageBox.Show($"Deactivate {_owner.FullName}?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _ctrl.Deactivate(_owner.OwnerID);
        DialogResult = DialogResult.OK;
        Close();
    }
}
