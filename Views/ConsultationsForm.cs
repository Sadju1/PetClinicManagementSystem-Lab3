using PetClinicLab3.Controllers;
using PetClinicLab3.Models;
using PetClinicLab3.Services;

namespace PetClinicLab3.Views;

public class ConsultationsForm : Form
{
    private readonly ConsultationController _ctrl = new();
    private readonly DataGridView           _grid = new();
    private readonly DateTimePicker         _dtFilter = new() { Format = DateTimePickerFormat.Short, Font = UITheme.InputFont, Width = 140 };
    private readonly CheckBox               _chkAll   = new() { Text = "Show All", Font = UITheme.LabelFont };
    private List<Consultation>              _items = new();

    public ConsultationsForm()
    {
        BackColor = UITheme.LightGray;
        var _hdr = UITheme.HeaderPanel("Consultations", "Manage Appointments");

        var toolbar = new FlowLayoutPanel
        {
            Dock      = DockStyle.Top,
            Height    = 56,
            Padding   = new Padding(16, 10, 16, 10),
            BackColor = UITheme.White,
        };
        toolbar.Controls.Add(UITheme.MakeLabel("Date:"));
        _dtFilter.Value       = DateTime.Today;
        _dtFilter.ValueChanged += (_, _) => { if (!_chkAll.Checked) LoadGrid(); };
        toolbar.Controls.Add(_dtFilter);
        _chkAll.CheckedChanged += (_, _) =>
        {
            _dtFilter.Enabled = !_chkAll.Checked;
            LoadGrid();
        };
        toolbar.Controls.Add(_chkAll);

        if (!AuthService.HasRole("ClinicManager"))
        {
            var btnNew = UITheme.TealButton("+ New Appointment");
            btnNew.Width  = 170;
            btnNew.Click += (_, _) => OpenDetail(null);
            toolbar.Controls.Add(btnNew);
        }

        _grid.Dock                = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ReadOnly            = true;
        _grid.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
        _grid.AllowUserToAddRows  = false;
        _grid.RowHeadersVisible   = false;
        _grid.BackgroundColor     = UITheme.White;
        _grid.Font                = UITheme.LabelFont;
        _grid.DoubleClick        += (_, _) => OpenSelected();

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(_hdr);

        Load += (_, _) => LoadGrid();
    }

    private void LoadGrid()
    {
        _items = _chkAll.Checked
            ? _ctrl.GetAll()
            : _ctrl.GetByDate(DateOnly.FromDateTime(_dtFilter.Value));

        _grid.DataSource = _items.Select(c => new
        {
            c.ConsultationID,
            Date    = c.ScheduledDate.ToString("yyyy-MM-dd"),
            Time    = $"{c.ScheduledTime:hh\\:mm}",
            c.PetName,
            Owner   = c.OwnerName,
            Vet     = c.VetName,
            Room    = c.RoomNumber ?? "-",
            c.Status,
        }).ToList();

        if (_grid.Columns.Contains("ConsultationID"))
            _grid.Columns["ConsultationID"]!.Visible = false;

        // Color rows by status
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var status = row.Cells["Status"].Value?.ToString() ?? "";
            row.DefaultCellStyle.BackColor = status switch
            {
                "Treated"   => Color.FromArgb(230, 255, 230),
                "Missed"    => Color.FromArgb(255, 230, 230),
                "Cancelled" => Color.FromArgb(240, 240, 240),
                "Arrived"   => Color.FromArgb(255, 255, 210),
                _           => UITheme.White,
            };
        }
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow == null) return;
        int id = (int)_grid.CurrentRow.Cells["ConsultationID"].Value;
        OpenDetail(_ctrl.GetById(id));
    }

    private void OpenDetail(Consultation? c)
    {
        var dlg = new ConsultationDetailForm(c);
        if (dlg.ShowDialog() == DialogResult.OK) LoadGrid();
    }
}
