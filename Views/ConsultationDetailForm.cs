using PetClinicLab3.Controllers;
using PetClinicLab3.Models;
using PetClinicLab3.Services;

namespace PetClinicLab3.Views;

public class ConsultationDetailForm : Form
{
    private readonly ConsultationController _ctrl    = new();
    private readonly PetController          _petCtrl = new();
    private readonly VeterinarianController _vetCtrl = new();
    private readonly RoomController         _roomCtrl= new();
    private Consultation _c;

    private readonly ComboBox       _cboPet   = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont };
    private readonly ComboBox       _cboVet   = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont };
    private readonly ComboBox       _cboRoom  = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont };
    private readonly DateTimePicker _dtDate   = new() { Format = DateTimePickerFormat.Short, Font = UITheme.InputFont };
    private readonly DateTimePicker _dtTime   = new() { Format = DateTimePickerFormat.Time,  Font = UITheme.InputFont, ShowUpDown = true };
    private readonly ComboBox       _cboStatus= new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont };
    private readonly TextBox        _txtReason= UITheme.MakeTextBox(500);
    private readonly TextBox        _txtNotes = new() { Multiline = true, Height = 60, Font = UITheme.InputFont };
    private readonly Label          _lblError = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public ConsultationDetailForm(Consultation? c)
    {
        _c              = c ?? new Consultation { Status = "Booked", ScheduledDate = DateOnly.FromDateTime(DateTime.Today) };
        bool isNew      = _c.ConsultationID == 0;
        Text            = isNew ? "New Appointment" : $"Appointment #{_c.ConsultationID}";
        Size            = new Size(560, 760);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        var header = UITheme.HeaderPanel(isNew ? "New Appointment" : "Edit Appointment");

        // Populate dropdowns
        var pets = _petCtrl.GetAll();
        _cboPet.DataSource    = pets;
        _cboPet.DisplayMember = "PetName";
        _cboPet.ValueMember   = "PetID";

        var vets = _vetCtrl.GetAll();
        _cboVet.DataSource    = vets;
        _cboVet.DisplayMember = "FullName";
        _cboVet.ValueMember   = "VeterinarianID";

        var rooms = new List<object> { new { RoomID = 0, Display = "(No Room)" } };
        rooms.AddRange(_roomCtrl.GetAll().Select(r => new { r.RoomID, Display = $"{r.RoomName} ({r.RoomNumber})" } as object));
        _cboRoom.DataSource    = rooms;
        _cboRoom.DisplayMember = "Display";
        _cboRoom.ValueMember   = "RoomID";

        // Show only the current status + valid transitions (controller remains authoritative)
        if (isNew)
        {
            _cboStatus.Items.Add("Booked");
        }
        else
        {
            _cboStatus.Items.Add(_c.Status);
            foreach (var next in _ctrl.GetValidNextStatuses(_c.Status))
                _cboStatus.Items.Add(next);
        }

        bool terminal = !isNew && (_c.Status is "Treated" or "Missed" or "Cancelled"
                                   || AuthService.HasRole("ClinicManager"));

        var pnl = new TableLayoutPanel { RowCount = 11, ColumnCount = 2, Dock = DockStyle.Fill, Padding = new Padding(24) };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 11; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        void Add(int r, string lbl, Control ctrl) { pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r); ctrl.Dock = DockStyle.Fill; pnl.Controls.Add(ctrl, 1, r); }

        Add(0, "Pet*:", _cboPet);
        Add(1, "Veterinarian*:", _cboVet);
        Add(2, "Room:", _cboRoom);
        Add(3, "Date*:", _dtDate);
        Add(4, "Time*:", _dtTime);
        Add(5, "Status:", _cboStatus);
        Add(6, "Reason:", _txtReason);
        Add(7, "Notes:", _txtNotes);
        pnl.RowStyles[7] = new RowStyle(SizeType.Absolute, 70);
        pnl.Controls.Add(_lblError, 1, 9);

        var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill };
        if (!terminal)
        {
            var btnSave = UITheme.TealButton(isNew ? "Book" : "Save");
            btnSave.Width = 110; btnSave.Click += OnSave;
            btnFlow.Controls.Add(btnSave);
        }

        if (!isNew && !terminal && AuthService.CanManageMedicalRecords)
        {
            var btnRecord = UITheme.NavyButton("Medical Record");
            btnRecord.Width = 150;
            btnRecord.Click += (_, _) =>
            {
                var dlg = new MedicalRecordForm(_c.ConsultationID);
                dlg.ShowDialog();
                DialogResult = DialogResult.OK;
                Close();
            };
            btnFlow.Controls.Add(btnRecord);
        }

        pnl.Controls.Add(btnFlow, 1, 10);

        Controls.Add(pnl);
        Controls.Add(header);

        Populate(terminal);
    }

    private void Populate(bool terminal)
    {
        if (_c.PetID > 0)          _cboPet.SelectedValue  = _c.PetID;
        if (_c.VeterinarianID > 0) _cboVet.SelectedValue  = _c.VeterinarianID;
        _cboRoom.SelectedValue = _c.RoomID ?? 0;
        _dtDate.Value          = _c.ScheduledDate == DateOnly.MinValue
                                 ? DateTime.Today
                                 : _c.ScheduledDate.ToDateTime(TimeOnly.MinValue);
        _dtTime.Value          = DateTime.Today.Add(_c.ScheduledTime == TimeSpan.Zero
                                 ? TimeSpan.FromHours(9)
                                 : _c.ScheduledTime);
        _cboStatus.SelectedItem = _c.Status;
        _txtReason.Text         = _c.Reason ?? "";
        _txtNotes.Text          = _c.Notes  ?? "";

        if (terminal)
        {
            foreach (Control ctrl in new Control[] { _cboPet, _cboVet, _cboRoom, _dtDate, _dtTime, _cboStatus, _txtReason, _txtNotes })
                ctrl.Enabled = false;
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text         = "";
        _c.PetID               = _cboPet.SelectedValue  is int pid ? pid : 0;
        _c.VeterinarianID      = _cboVet.SelectedValue  is int vid ? vid : 0;
        int roomId             = _cboRoom.SelectedValue is int rid ? rid : 0;
        _c.RoomID              = roomId == 0 ? null : roomId;
        _c.ScheduledDate       = DateOnly.FromDateTime(_dtDate.Value);
        _c.ScheduledTime       = _dtTime.Value.TimeOfDay;
        _c.Reason              = _txtReason.Text;
        _c.Notes               = _txtNotes.Text;

        (bool ok, string err) result;
        if (_c.ConsultationID == 0)
            result = _ctrl.Book(_c);
        else
        {
            var newStatus = _cboStatus.SelectedItem?.ToString() ?? _c.Status;
            if (newStatus != _c.Status)
            {
                var (sok, serr) = _ctrl.UpdateStatus(_c.ConsultationID, newStatus);
                if (!sok) { _lblError.Text = serr; return; }
            }
            result = _ctrl.Update(_c);
        }

        if (!result.ok) { _lblError.Text = result.err; return; }
        DialogResult = DialogResult.OK;
        Close();
    }
}
