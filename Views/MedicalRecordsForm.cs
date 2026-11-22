using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class MedicalRecordsForm : Form
{
    private readonly MedicalRecordController _ctrl = new();
    private readonly DataGridView            _grid = new();
    private readonly TextBox                 _txtSearch = UITheme.MakeTextBox(100);
    private readonly Label                   _lblEmpty  = new();
    private List<MedicalRecord>              _items = new();

    public MedicalRecordsForm()
    {
        BackColor = UITheme.LightGray;
        var hdr = UITheme.HeaderPanel("Medical Records", "Patient History");

        // Toolbar with search
        var toolbar = new FlowLayoutPanel
        {
            Dock      = DockStyle.Top,
            Height    = 56,
            Padding   = new Padding(16, 10, 16, 10),
            BackColor = UITheme.White,
        };

        toolbar.Controls.Add(UITheme.MakeLabel("Search:"));
        _txtSearch.Width       = 220;
        _txtSearch.PlaceholderText = "Pet name, owner, or vet…";
        _txtSearch.KeyDown    += (_, e) => { if (e.KeyCode == Keys.Enter) DoSearch(); };
        toolbar.Controls.Add(_txtSearch);

        var btnSearch = UITheme.TealButton("Search");
        btnSearch.Width  = 90;
        btnSearch.Click += (_, _) => DoSearch();
        toolbar.Controls.Add(btnSearch);

        var btnClear = UITheme.NavyButton("Clear");
        btnClear.Width  = 70;
        btnClear.Click += (_, _) => { _txtSearch.Clear(); LoadGrid(); };
        toolbar.Controls.Add(btnClear);

        var btnRefresh = UITheme.MakeButton("Refresh", UITheme.LightGray, UITheme.DarkText);
        btnRefresh.Width  = 90;
        btnRefresh.Click += (_, _) => LoadGrid();
        toolbar.Controls.Add(btnRefresh);

        // Grid
        _grid.Dock                = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ReadOnly            = true;
        _grid.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
        _grid.AllowUserToAddRows  = false;
        _grid.RowHeadersVisible   = false;
        _grid.BackgroundColor     = UITheme.White;
        _grid.Font                = UITheme.LabelFont;
        _grid.DoubleClick        += (_, _) => OpenSelected();

        // Empty state
        _lblEmpty.Text      = "No records found. Try a different search term.";
        _lblEmpty.Font      = new Font("Segoe UI", 11f, FontStyle.Italic);
        _lblEmpty.ForeColor = Color.Gray;
        _lblEmpty.Dock      = DockStyle.Fill;
        _lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
        _lblEmpty.Visible   = false;

        Controls.Add(_grid);
        Controls.Add(_lblEmpty);
        Controls.Add(toolbar);
        Controls.Add(hdr);

        Load += (_, _) => LoadGrid();
    }

    private void DoSearch()
    {
        var term = _txtSearch.Text.Trim();
        try
        {
            _items = _ctrl.Search(term);
            ShowItems();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Search failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadGrid()
    {
        try
        {
            _items = _ctrl.GetAll();
            ShowItems();
        }
        catch (Exception ex)
        {
            _lblEmpty.Text    = $"Could not load records: {ex.Message}";
            _lblEmpty.Visible = true;
            _grid.Visible     = false;
        }
    }

    private void ShowItems()
    {
        if (_items.Count == 0)
        {
            _grid.Visible     = false;
            _lblEmpty.Text    = _txtSearch.Text.Length > 0
                ? $"No records found matching \"{_txtSearch.Text.Trim()}\"."
                : "No medical records yet.";
            _lblEmpty.Visible = true;
            return;
        }

        _lblEmpty.Visible = false;
        _grid.Visible     = true;

        _grid.DataSource = _items.Select(r => new
        {
            r.RecordID,
            r.ConsultationID,
            Date      = r.ScheduledDate == DateOnly.MinValue ? "" : r.ScheduledDate.ToString("yyyy-MM-dd"),
            r.PetName,
            Owner     = r.OwnerName,
            Vet       = r.VetName,
            Diagnosis = (r.Diagnosis?.Length > 60 ? r.Diagnosis[..60] + "…" : r.Diagnosis) ?? "",
            Created   = r.CreatedAt.ToString("yyyy-MM-dd"),
        }).ToList();

        if (_grid.Columns.Contains("RecordID"))       _grid.Columns["RecordID"]!.Visible       = false;
        if (_grid.Columns.Contains("ConsultationID")) _grid.Columns["ConsultationID"]!.Visible = false;
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow == null) return;
        int cid = (int)_grid.CurrentRow.Cells["ConsultationID"].Value;
        var dlg = new MedicalRecordForm(cid);
        if (dlg.ShowDialog() == DialogResult.OK) LoadGrid();
    }
}
