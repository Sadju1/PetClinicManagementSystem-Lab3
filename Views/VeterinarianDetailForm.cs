using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class VeterinarianDetailForm : Form
{
    private readonly VeterinarianController _ctrl = new();
    private Veterinarian _vet;

    private readonly TextBox   _txtFirst   = UITheme.MakeTextBox(50);
    private readonly TextBox   _txtLast    = UITheme.MakeTextBox(50);
    private readonly TextBox   _txtPhone   = UITheme.MakeTextBox(20);
    private readonly TextBox   _txtEmail   = UITheme.MakeTextBox(100);
    private readonly TextBox   _txtLicense = UITheme.MakeTextBox(50);
    private readonly CheckedListBox _specList = new() { Font = UITheme.InputFont };
    private readonly Label     _lblError   = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };
    private List<Specialization> _allSpecs = new();

    public VeterinarianDetailForm(Veterinarian? vet)
    {
        _vet            = vet ?? new Veterinarian { IsActive = true };
        bool isNew      = _vet.VeterinarianID == 0;
        Text            = isNew ? "New Veterinarian" : $"Edit — {_vet.FullName}";
        Size            = new Size(540, 760);
        MinimumSize     = new Size(480, 600);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        var header = UITheme.HeaderPanel(isNew ? "New Veterinarian" : "Edit Veterinarian");

        var pnl = new TableLayoutPanel { RowCount = 10, ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(24) };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 10; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 6 ? 130 : 46));

        void Add(int r, string lbl, Control ctrl) { pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r); ctrl.Dock = DockStyle.Fill; pnl.Controls.Add(ctrl, 1, r); }

        Add(0, "First Name*:", _txtFirst);
        Add(1, "Last Name*:",  _txtLast);
        Add(2, "Phone:",       _txtPhone);
        Add(3, "Email:",       _txtEmail);
        Add(4, "License No.:", _txtLicense);
        pnl.Controls.Add(UITheme.MakeLabel("Specializations:"), 0, 5);
        _specList.Dock = DockStyle.Fill;
        pnl.Controls.Add(_specList, 1, 5);
        pnl.RowStyles[5] = new RowStyle(SizeType.Absolute, 130);
        pnl.Controls.Add(_lblError, 1, 7);

        var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var btnSave = UITheme.TealButton("Save");
        btnSave.Width = 110; btnSave.Click += OnSave;
        btnFlow.Controls.Add(btnSave);
        if (!isNew)
        {
            var btnDeact = UITheme.MakeButton("Deactivate", UITheme.ErrorRed, UITheme.White);
            btnDeact.Width = 120; btnDeact.Click += OnDeactivate;
            btnFlow.Controls.Add(btnDeact);
        }
        pnl.Controls.Add(btnFlow, 1, 8);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(pnl);

        Controls.Add(scroll);
        Controls.Add(header);

        Populate();
    }

    private void Populate()
    {
        _txtFirst.Text   = _vet.FirstName;
        _txtLast.Text    = _vet.LastName;
        _txtPhone.Text   = _vet.Phone         ?? "";
        _txtEmail.Text   = _vet.Email         ?? "";
        _txtLicense.Text = _vet.LicenseNumber ?? "";

        _allSpecs = _ctrl.GetAllSpecializations();
        _specList.Items.Clear();
        var vetSpecIds = new HashSet<int>(_vet.Specializations.Select(s => s.SpecializationID));
        foreach (var s in _allSpecs)
            _specList.Items.Add(s.SpecializationName, vetSpecIds.Contains(s.SpecializationID));
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text      = "";
        _vet.FirstName      = _txtFirst.Text;
        _vet.LastName       = _txtLast.Text;
        _vet.Phone          = _txtPhone.Text;
        _vet.Email          = _txtEmail.Text;
        _vet.LicenseNumber  = _txtLicense.Text;

        var selectedIds = new List<int>();
        for (int i = 0; i < _specList.Items.Count; i++)
            if (_specList.GetItemChecked(i))
                selectedIds.Add(_allSpecs[i].SpecializationID);

        var (ok, err) = _ctrl.Save(_vet, selectedIds);
        if (!ok) { _lblError.Text = err; return; }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnDeactivate(object? sender, EventArgs e)
    {
        if (MessageBox.Show($"Deactivate {_vet.FullName}?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _ctrl.Deactivate(_vet.VeterinarianID);
        DialogResult = DialogResult.OK;
        Close();
    }
}
