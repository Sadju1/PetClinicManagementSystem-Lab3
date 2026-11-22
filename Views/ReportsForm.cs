using PetClinicLab3.Controllers;

namespace PetClinicLab3.Views;

public class ReportsForm : Form
{
    private readonly ConsultationController   _conCtrl  = new();
    private readonly MedicalRecordController  _recCtrl  = new();
    private readonly OwnerController          _ownerCtrl= new();
    private readonly PetController            _petCtrl  = new();
    private readonly VeterinarianController   _vetCtrl  = new();
    private readonly DataGridView             _grid     = new();
    private readonly ComboBox                 _cboReport= new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.InputFont, Width = 220 };
    private readonly DateTimePicker           _dtFrom   = new() { Format = DateTimePickerFormat.Short, Font = UITheme.InputFont, Width = 130 };
    private readonly DateTimePicker           _dtTo     = new() { Format = DateTimePickerFormat.Short, Font = UITheme.InputFont, Width = 130 };

    public ReportsForm()
    {
        BackColor = UITheme.LightGray;
        var _hdr = UITheme.HeaderPanel("Reports", "Clinic Analytics");

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 10, 16, 10), BackColor = UITheme.White };
        _cboReport.Items.AddRange(new[]
        {
            "Consultations by Status",
            "Appointments by Veterinarian",
            "Medical Records Summary",
            "Active Pets & Owners",
        });
        _cboReport.SelectedIndex = 0;
        toolbar.Controls.Add(UITheme.MakeLabel("Report:"));
        toolbar.Controls.Add(_cboReport);
        toolbar.Controls.Add(UITheme.MakeLabel("  From:"));
        _dtFrom.Value = DateTime.Today.AddMonths(-1);
        toolbar.Controls.Add(_dtFrom);
        toolbar.Controls.Add(UITheme.MakeLabel("  To:"));
        _dtTo.Value   = DateTime.Today;
        toolbar.Controls.Add(_dtTo);
        var btnRun = UITheme.TealButton("Run");
        btnRun.Width = 80; btnRun.Click += (_, _) => RunReport();
        toolbar.Controls.Add(btnRun);

        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = UITheme.White;
        _grid.Font = UITheme.LabelFont;

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(_hdr);

        Load += (_, _) => RunReport();
    }

    private void RunReport()
    {
        var from = DateOnly.FromDateTime(_dtFrom.Value);
        var to   = DateOnly.FromDateTime(_dtTo.Value);

        try
        {
            switch (_cboReport.SelectedIndex)
            {
                case 0: // By status
                    var allCons = _conCtrl.GetAll()
                        .Where(c => c.ScheduledDate >= from && c.ScheduledDate <= to);
                    _grid.DataSource = allCons.GroupBy(c => c.Status)
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count).ToList();
                    break;

                case 1: // By vet
                    var consByVet = _conCtrl.GetAll()
                        .Where(c => c.ScheduledDate >= from && c.ScheduledDate <= to);
                    _grid.DataSource = consByVet.GroupBy(c => c.VetName)
                        .Select(g => new { Veterinarian = g.Key, Total = g.Count(),
                            Treated = g.Count(x => x.Status == "Treated") })
                        .OrderByDescending(x => x.Total).ToList();
                    break;

                case 2: // Medical records
                    var recs = _recCtrl.GetAll();
                    _grid.DataSource = recs.Select(r => new
                    {
                        Date      = r.ScheduledDate == DateOnly.MinValue ? "" : r.ScheduledDate.ToString("yyyy-MM-dd"),
                        r.PetName,
                        Owner     = r.OwnerName,
                        Vet       = r.VetName,
                        Diagnosis = r.Diagnosis?.Length > 80 ? r.Diagnosis[..80] + "…" : r.Diagnosis ?? "",
                    }).ToList();
                    break;

                case 3: // Active pets & owners
                    var pets = _petCtrl.GetAll();
                    _grid.DataSource = pets.Select(p => new
                    {
                        p.PetName, p.Species, p.Breed,
                        Owner = p.OwnerName,
                    }).ToList();
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Report error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
