using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class ExaminationRoomsForm : Form
{
    private readonly RoomController  _ctrl = new();
    private readonly VeterinarianController _vetCtrl = new();
    private readonly DataGridView    _grid = new();
    private List<ExaminationRoom>    _rooms = new();

    public ExaminationRoomsForm()
    {
        BackColor = UITheme.LightGray;
        var _hdr = UITheme.HeaderPanel("Examination Rooms", "Manage Clinic Rooms");

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 10, 16, 10), BackColor = UITheme.White };
        var btnNew = UITheme.TealButton("+ New Room");
        btnNew.Width = 120; btnNew.Click += (_, _) => OpenDetail(null);
        toolbar.Controls.Add(btnNew);

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
        _rooms = _ctrl.GetAll();
        _grid.DataSource = _rooms.Select(r => new
        {
            r.RoomID, r.RoomName, r.RoomNumber,
            AssignedVet = r.AssignedVetName ?? "(unassigned)",
            Available   = r.IsAvailable ? "Yes" : "No",
        }).ToList();
        if (_grid.Columns.Contains("RoomID")) _grid.Columns["RoomID"]!.Visible = false;
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow == null) return;
        int id = (int)_grid.CurrentRow.Cells["RoomID"].Value;
        OpenDetail(_ctrl.GetById(id));
    }

    private void OpenDetail(ExaminationRoom? room)
    {
        var dlg = new ExaminationRoomDetailForm(room, _vetCtrl.GetAll());
        if (dlg.ShowDialog() == DialogResult.OK) LoadGrid();
    }
}

public class ExaminationRoomDetailForm : Form
{
    private readonly RoomController _ctrl = new();
    private ExaminationRoom _room;

    private readonly TextBox   _txtName  = UITheme.MakeTextBox(100);
    private readonly TextBox   _txtNum   = UITheme.MakeTextBox(20);
    private readonly TextBox   _txtDesc  = new() { Multiline = true, Height = 60, Font = UITheme.InputFont };
    private readonly ComboBox  _cboVet   = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont };
    private readonly CheckBox  _chkAvail = new() { Text = "Available", Font = UITheme.LabelFont, Checked = true };
    private readonly Label     _lblError = new() { ForeColor = UITheme.ErrorRed, AutoSize = true, Text = "" };

    public ExaminationRoomDetailForm(ExaminationRoom? room, List<Veterinarian> vets)
    {
        _room           = room ?? new ExaminationRoom { IsAvailable = true };
        bool isNew      = _room.RoomID == 0;
        Text            = isNew ? "New Room" : $"Edit — {_room.RoomName}";
        Size            = new Size(500, 640);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = UITheme.White;

        var _roomHeader = UITheme.HeaderPanel(isNew ? "New Room" : "Edit Room");

        var vetList = new List<object> { new { VeterinarianID = 0, Display = "(No assignment)" } };
        vetList.AddRange(vets.Select(v => new { v.VeterinarianID, Display = v.FullName } as object));
        _cboVet.DataSource    = vetList;
        _cboVet.DisplayMember = "Display";
        _cboVet.ValueMember   = "VeterinarianID";

        var pnl = new TableLayoutPanel { RowCount = 9, ColumnCount = 2, Dock = DockStyle.Fill, Padding = new Padding(24) };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 9; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 3 ? 70 : 46));

        void Add(int r, string lbl, Control ctrl) { pnl.Controls.Add(UITheme.MakeLabel(lbl), 0, r); ctrl.Dock = DockStyle.Fill; pnl.Controls.Add(ctrl, 1, r); }

        Add(0, "Room Name*:", _txtName);
        Add(1, "Room Number:", _txtNum);
        Add(2, "Assign Vet:", _cboVet);
        Add(3, "Description:", _txtDesc);
        pnl.Controls.Add(_chkAvail, 1, 4);
        pnl.Controls.Add(_lblError, 1, 5);

        var btnSave = UITheme.TealButton("Save");
        btnSave.Dock = DockStyle.Fill; btnSave.Click += OnSave;
        pnl.Controls.Add(btnSave, 1, 7);

        Controls.Add(pnl);
        Controls.Add(_roomHeader);

        _txtName.Text  = _room.RoomName;
        _txtNum.Text   = _room.RoomNumber ?? "";
        _txtDesc.Text  = _room.Description ?? "";
        _chkAvail.Checked = _room.IsAvailable;
        if (_room.AssignedVeterinarianID.HasValue) _cboVet.SelectedValue = _room.AssignedVeterinarianID.Value;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblError.Text              = "";
        _room.RoomName              = _txtName.Text;
        _room.RoomNumber            = _txtNum.Text;
        _room.Description           = _txtDesc.Text;
        _room.IsAvailable           = _chkAvail.Checked;
        int vid = _cboVet.SelectedValue is int v ? v : 0;
        _room.AssignedVeterinarianID = vid == 0 ? null : vid;
        var (ok, err) = _ctrl.Save(_room);
        if (!ok) { _lblError.Text = err; return; }
        DialogResult = DialogResult.OK;
        Close();
    }
}
