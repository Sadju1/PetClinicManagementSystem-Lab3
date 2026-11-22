using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class PetDetailForm : Form
{
    private readonly PetController   _ctrl      = new();
    private readonly OwnerController _ownerCtrl = new();
    private Pet _pet;

    private readonly ComboBox _cboOwner  = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont };
    private readonly TextBox  _txtName   = UITheme.MakeTextBox(100);
    private readonly TextBox  _txtSpecies= UITheme.MakeTextBox(50);
    private readonly TextBox  _txtBreed  = UITheme.MakeTextBox(100);
    private readonly DateTimePicker _dtDOB = new() { Format = DateTimePickerFormat.Short, Font = UITheme.InputFont };
    private readonly CheckBox _chkDOB    = new() { Text = "Known", Font = UITheme.LabelFont };
    private readonly TextBox  _txtTag    = UITheme.MakeTextBox(50);
    private readonly TextBox  _txtNotes  = new() { Multiline = true, Height = 60, Font = UITheme.InputFont, ScrollBars = ScrollBars.Vertical };
    private readonly Label    _lblError  = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public PetDetailForm(Pet? pet)
    {
        _pet            = pet ?? new Pet { IsActive = true };
        bool isNew      = _pet.PetID == 0;
        Text            = isNew ? "New Pet" : $"Edit Pet — {_pet.PetName}";
        Size            = new Size(540, 710);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        var header = UITheme.HeaderPanel(isNew ? "New Pet" : "Edit Pet");

        var pnl = new TableLayoutPanel
        {
            RowCount = 11, ColumnCount = 2,
            Dock     = DockStyle.Fill,
            Padding  = new Padding(24),
        };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 11; i++)
            pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 7 ? 70 : 44));

        void Add(int r, string lbl, Control ctrl)
        {
            pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r);
            ctrl.Dock = DockStyle.Fill;
            pnl.Controls.Add(ctrl, 1, r);
        }

        // Owner dropdown
        var owners = _ownerCtrl.GetAll();
        _cboOwner.DataSource    = owners;
        _cboOwner.DisplayMember = "FullName";
        _cboOwner.ValueMember   = "OwnerID";

        Add(0, "Owner*:", _cboOwner);
        Add(1, "Pet Name*:", _txtName);
        Add(2, "Species*:", _txtSpecies);
        Add(3, "Breed:", _txtBreed);

        // DOB row
        var dobPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        _dtDOB.Width = 140;
        dobPanel.Controls.Add(_dtDOB);
        dobPanel.Controls.Add(_chkDOB);
        pnl.Controls.Add(UITheme.MakeLabel("Date of Birth:"), 0, 4);
        pnl.Controls.Add(dobPanel, 1, 4);

        Add(5, "ID Tag:", _txtTag);
        Add(6, "Notes:", _txtNotes);
        pnl.RowStyles[6] = new RowStyle(SizeType.Absolute, 80);
        pnl.Controls.Add(_lblError, 1, 8);

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
        pnl.Controls.Add(btnFlow, 1, 9);

        Controls.Add(pnl);
        Controls.Add(header);

        _chkDOB.CheckedChanged += (_, _) => _dtDOB.Enabled = _chkDOB.Checked;
        Populate();
    }

    private void Populate()
    {
        if (_pet.OwnerID > 0) _cboOwner.SelectedValue = _pet.OwnerID;
        _txtName.Text    = _pet.PetName;
        _txtSpecies.Text = _pet.Species;
        _txtBreed.Text   = _pet.Breed   ?? "";
        _txtTag.Text     = _pet.IdentificationTag ?? "";
        _txtNotes.Text   = _pet.Notes   ?? "";
        bool hasDOB      = _pet.DateOfBirth.HasValue;
        _chkDOB.Checked  = hasDOB;
        _dtDOB.Enabled   = hasDOB;
        if (hasDOB) _dtDOB.Value = _pet.DateOfBirth!.Value;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text      = "";
        _pet.OwnerID        = _cboOwner.SelectedValue is int id ? id : 0;
        _pet.PetName        = _txtName.Text;
        _pet.Species        = _txtSpecies.Text;
        _pet.Breed          = _txtBreed.Text;
        _pet.DateOfBirth    = _chkDOB.Checked ? _dtDOB.Value : null;
        _pet.IdentificationTag = _txtTag.Text;
        _pet.Notes          = _txtNotes.Text;
        var (ok, err) = _ctrl.Save(_pet);
        if (!ok) { _lblError.Text = err; return; }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnDeactivate(object? sender, EventArgs e)
    {
        if (MessageBox.Show($"Deactivate {_pet.PetName}?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _ctrl.Deactivate(_pet.PetID);
        DialogResult = DialogResult.OK;
        Close();
    }
}
