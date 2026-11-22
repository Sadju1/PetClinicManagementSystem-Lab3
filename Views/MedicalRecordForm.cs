using PetClinicLab3.Controllers;
using PetClinicLab3.Models;
using PetClinicLab3.Services;

namespace PetClinicLab3.Views;

public class MedicalRecordForm : Form
{
    private readonly MedicalRecordController  _ctrl   = new();
    private readonly ConsultationController   _conCtrl= new();
    private MedicalRecord _record;
    private readonly int _consultationId;

    private readonly TextBox       _txtDiag    = new() { Multiline = true, Height = 70, Font = UITheme.InputFont, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox       _txtTreat   = new() { Multiline = true, Height = 70, Font = UITheme.InputFont, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox       _txtRx      = new() { Multiline = true, Height = 70, Font = UITheme.InputFont, ScrollBars = ScrollBars.Vertical };
    private readonly DateTimePicker _dtFollowUp = new() { Format = DateTimePickerFormat.Short, Font = UITheme.InputFont };
    private readonly CheckBox      _chkFollowUp= new() { Text = "Follow-up needed", Font = UITheme.LabelFont };
    private readonly TextBox       _txtNotes   = new() { Multiline = true, Height = 60, Font = UITheme.InputFont, ScrollBars = ScrollBars.Vertical };
    private readonly Label         _lblInfo    = new() { AutoSize = true, Font = UITheme.LabelFont };
    private readonly Label         _lblError   = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public MedicalRecordForm(int consultationId)
    {
        _consultationId = consultationId;
        _record         = new MedicalRecord { ConsultationID = consultationId };

        Text            = "Medical Record";
        Size            = new Size(600, 700);
        MinimumSize     = new Size(500, 500);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        var header = UITheme.HeaderPanel("Medical Record");

        var pnl = new TableLayoutPanel { RowCount = 11, ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(20) };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 11; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, i < 4 ? 82 : 46));

        void Add(int r, string lbl, Control ctrl) { pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r); ctrl.Dock = DockStyle.Fill; pnl.Controls.Add(ctrl, 1, r); }

        pnl.Controls.Add(_lblInfo, 0, 0);
        pnl.SetColumnSpan(_lblInfo, 2);
        Add(1, "Diagnosis:", _txtDiag);
        Add(2, "Treatment:", _txtTreat);
        Add(3, "Prescription:", _txtRx);

        var fuPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        _chkFollowUp.CheckedChanged += (_, _) => _dtFollowUp.Enabled = _chkFollowUp.Checked;
        _dtFollowUp.Enabled = false;
        fuPanel.Controls.Add(_chkFollowUp);
        fuPanel.Controls.Add(_dtFollowUp);
        Add(4, "Follow-up:", fuPanel);
        Add(5, "Notes:", _txtNotes);
        pnl.RowStyles[5] = new RowStyle(SizeType.Absolute, 70);
        pnl.Controls.Add(_lblError, 1, 7);

        var btnSave = UITheme.TealButton("Save Record");
        btnSave.Dock   = DockStyle.Fill;
        btnSave.Click += OnSave;
        pnl.Controls.Add(btnSave, 1, 9);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(pnl);

        Controls.Add(scroll);
        Controls.Add(header);

        Load += (_, _) => LoadExisting();
    }

    private void LoadExisting()
    {
        var consultation = _conCtrl.GetById(_consultationId);
        if (consultation != null)
            _lblInfo.Text = $"Patient: {consultation.PetName}  |  Owner: {consultation.OwnerName}  |  Vet: {consultation.VetName}  |  Date: {consultation.ScheduledDate}";

        var existing = _ctrl.GetByConsultation(_consultationId);
        if (existing != null)
        {
            _record          = existing;
            _txtDiag.Text    = existing.Diagnosis    ?? "";
            _txtTreat.Text   = existing.Treatment    ?? "";
            _txtRx.Text      = existing.Prescription ?? "";
            _txtNotes.Text   = existing.Notes        ?? "";
            if (existing.FollowUpDate.HasValue)
            {
                _chkFollowUp.Checked = true;
                _dtFollowUp.Value    = existing.FollowUpDate.Value.ToDateTime(TimeOnly.MinValue);
                _dtFollowUp.Enabled  = true;
            }
        }

        bool readOnly = !AuthService.CanManageMedicalRecords;
        foreach (Control ctrl in new Control[] { _txtDiag, _txtTreat, _txtRx, _txtNotes, _chkFollowUp, _dtFollowUp })
            ctrl.Enabled = !readOnly;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text       = "";
        _record.ConsultationID= _consultationId;
        _record.Diagnosis    = _txtDiag.Text;
        _record.Treatment    = _txtTreat.Text;
        _record.Prescription = _txtRx.Text;
        _record.Notes        = _txtNotes.Text;
        _record.FollowUpDate = _chkFollowUp.Checked
                               ? DateOnly.FromDateTime(_dtFollowUp.Value)
                               : null;

        var (ok, err) = _ctrl.Save(_record);
        if (!ok) { _lblError.Text = err; return; }
        MessageBox.Show("Medical record saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }
}
