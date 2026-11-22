using PetClinicLab3.Controllers;
using PetClinicLab3.Models;

namespace PetClinicLab3.Views;

public class OwnersForm : Form
{
    private readonly OwnerController _ctrl = new();
    private readonly DataGridView    _grid = new();
    private readonly TextBox         _search = UITheme.MakeTextBox();
    private List<Owner>              _owners = new();

    public OwnersForm()
    {
        BackColor = UITheme.LightGray;
        var _hdr = UITheme.HeaderPanel("Owners", "Manage Pet Owners");

        // Toolbar
        var toolbar = new FlowLayoutPanel
        {
            Dock      = DockStyle.Top,
            Height    = 56,
            Padding   = new Padding(16, 10, 16, 10),
            BackColor = UITheme.White,
        };
        _search.Width       = 220;
        _search.PlaceholderText = "Search name, phone, email...";
        _search.TextChanged += (_, _) => LoadGrid();
        toolbar.Controls.Add(_search);

        var btnNew = UITheme.TealButton("+ New Owner");
        btnNew.Width  = 130;
        btnNew.Click += (_, _) => OpenDetail(null);
        toolbar.Controls.Add(btnNew);

        // Grid
        _grid.Dock               = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ReadOnly           = true;
        _grid.SelectionMode      = DataGridViewSelectionMode.FullRowSelect;
        _grid.AllowUserToAddRows = false;
        _grid.RowHeadersVisible  = false;
        _grid.BackgroundColor    = UITheme.White;
        _grid.Font               = UITheme.LabelFont;
        _grid.DoubleClick        += (_, _) => OpenSelected();

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(_hdr);

        Load += (_, _) => LoadGrid();
    }

    private void LoadGrid()
    {
        _owners = string.IsNullOrWhiteSpace(_search.Text)
            ? _ctrl.GetAll()
            : _ctrl.Search(_search.Text);

        _grid.DataSource = _owners.Select(o => new
        {
            o.OwnerID, Name = o.FullName, o.Phone, o.Email, o.Address,
            Status = o.IsActive ? "Active" : "Inactive"
        }).ToList();

        if (_grid.Columns.Contains("OwnerID"))
            _grid.Columns["OwnerID"]!.Visible = false;
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow == null) return;
        int id = (int)_grid.CurrentRow.Cells["OwnerID"].Value;
        OpenDetail(_ctrl.GetById(id));
    }

    private void OpenDetail(Owner? owner)
    {
        var dlg = new OwnerDetailForm(owner);
        if (dlg.ShowDialog() == DialogResult.OK)
            LoadGrid();
    }
}
