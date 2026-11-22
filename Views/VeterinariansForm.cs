using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class VeterinariansForm : Form
{
    private readonly VeterinarianController _ctrl = new();
    private readonly DataGridView           _grid = new();
    private readonly TextBox                _search = UITheme.MakeTextBox();
    private List<Veterinarian>              _vets = new();

    public VeterinariansForm()
    {
        BackColor = UITheme.LightGray;
        var _hdr = UITheme.HeaderPanel("Veterinarians", "Manage Clinic Staff");

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 10, 16, 10), BackColor = UITheme.White };
        _search.Width           = 220;
        _search.PlaceholderText = "Search name, license...";
        _search.TextChanged    += (_, _) => LoadGrid();
        toolbar.Controls.Add(_search);

        var btnNew = UITheme.TealButton("+ New Veterinarian");
        btnNew.Width  = 160;
        btnNew.Click += (_, _) => OpenDetail(null);
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
        _vets = string.IsNullOrWhiteSpace(_search.Text)
            ? _ctrl.GetAll()
            : _ctrl.Search(_search.Text);

        _grid.DataSource = _vets.Select(v => new
        {
            v.VeterinarianID,
            Name          = v.FullName,
            v.Phone,
            v.Email,
            License       = v.LicenseNumber ?? "",
            Specializations = string.Join(", ", v.Specializations.Select(s => s.SpecializationName)),
            Status        = v.IsActive ? "Active" : "Inactive",
        }).ToList();

        if (_grid.Columns.Contains("VeterinarianID"))
            _grid.Columns["VeterinarianID"]!.Visible = false;
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow == null) return;
        int id = (int)_grid.CurrentRow.Cells["VeterinarianID"].Value;
        OpenDetail(_ctrl.GetById(id));
    }

    private void OpenDetail(Veterinarian? v)
    {
        var dlg = new VeterinarianDetailForm(v);
        if (dlg.ShowDialog() == DialogResult.OK) LoadGrid();
    }
}
