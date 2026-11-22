using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class PetsForm : Form
{
    private readonly PetController  _ctrl = new();
    private readonly DataGridView   _grid = new();
    private readonly TextBox        _search = UITheme.MakeTextBox();
    private List<Pet>               _pets = new();

    public PetsForm()
    {
        BackColor = UITheme.LightGray;
        var _hdr = UITheme.HeaderPanel("Pets", "Manage Patients");

        var toolbar = new FlowLayoutPanel
        {
            Dock      = DockStyle.Top,
            Height    = 56,
            Padding   = new Padding(16, 10, 16, 10),
            BackColor = UITheme.White,
        };
        _search.Width           = 220;
        _search.PlaceholderText = "Search name, species, owner...";
        _search.TextChanged    += (_, _) => LoadGrid();
        toolbar.Controls.Add(_search);

        var btnNew = UITheme.TealButton("+ New Pet");
        btnNew.Width  = 110;
        btnNew.Click += (_, _) => OpenDetail(null);
        toolbar.Controls.Add(btnNew);

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
        _pets = string.IsNullOrWhiteSpace(_search.Text)
            ? _ctrl.GetAll()
            : _ctrl.Search(_search.Text);

        _grid.DataSource = _pets.Select(p => new
        {
            p.PetID, p.PetName, p.Species, p.Breed,
            Owner     = p.OwnerName,
            DOB       = p.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
            Tag       = p.IdentificationTag ?? "",
            Status    = p.IsActive ? "Active" : "Inactive",
        }).ToList();

        if (_grid.Columns.Contains("PetID"))
            _grid.Columns["PetID"]!.Visible = false;
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow == null) return;
        int id = (int)_grid.CurrentRow.Cells["PetID"].Value;
        OpenDetail(_ctrl.GetById(id));
    }

    private void OpenDetail(Pet? pet)
    {
        var dlg = new PetDetailForm(pet);
        if (dlg.ShowDialog() == DialogResult.OK) LoadGrid();
    }
}
