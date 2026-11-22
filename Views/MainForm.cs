using PetClinicLab3.Services;

namespace PetClinicLab3.Views;

public class MainForm : Form
{
    private readonly Panel       _sidePanel;
    private readonly Panel       _contentPanel;
    private Form?                _currentChild;

    public MainForm()
    {
        Text            = "Pet Clinic Management System";
        Size            = new Size(1200, 760);
        MinimumSize     = new Size(900, 600);
        WindowState     = FormWindowState.Maximized;
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = UITheme.LightGray;

        // Sidebar
        _sidePanel = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 210,
            BackColor = UITheme.Navy,
        };

        var appTitle = new Label
        {
            Text      = "🐾 Pet Clinic",
            Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = UITheme.White,
            Dock      = DockStyle.Top,
            Height    = 70,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        _sidePanel.Controls.Add(appTitle);

        bool isClinicManager = AuthService.HasRole("ClinicManager");

        AddNavButton("Dashboard",     () => ShowForm(new DashboardForm()));

        if (!isClinicManager)
        {
            AddNavButton("Owners",          () => ShowForm(new OwnersForm()));
            AddNavButton("Pets",            () => ShowForm(new PetsForm()));
        }

        AddNavButton("Consultations", () => ShowForm(new ConsultationsForm()));

        if (!isClinicManager)
        {
            AddNavButton("Medical Records", () => ShowForm(new MedicalRecordsForm()));
            AddNavButton("Veterinarians",   () => ShowForm(new VeterinariansForm()));
            AddNavButton("Exam Rooms",      () => ShowForm(new ExaminationRoomsForm()));
        }

        AddNavButton("Reports",       () => ShowForm(new ReportsForm()));

        if (AuthService.IsAdmin)
            AddNavButton("Users", () => ShowForm(new UsersForm()));

        // Logout at bottom
        var btnLogout = UITheme.MakeButton("Logout", Color.FromArgb(80, 20, 20), UITheme.White);
        btnLogout.Dock   = DockStyle.Bottom;
        btnLogout.Height = 42;
        btnLogout.Click += (_, _) => { AuthService.Logout(); Close(); };
        _sidePanel.Controls.Add(btnLogout);

        _contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };

        Controls.Add(_contentPanel);
        Controls.Add(_sidePanel);

        Load += (_, _) => ShowForm(new DashboardForm());
    }

    private void AddNavButton(string label, Action action)
    {
        var btn = new Button
        {
            Text      = label,
            Dock      = DockStyle.Top,
            Height    = 46,
            FlatStyle = FlatStyle.Flat,
            BackColor = UITheme.Navy,
            ForeColor = UITheme.White,
            Font      = UITheme.ButtonFont,
            Cursor    = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(16, 0, 0, 0),
        };
        btn.FlatAppearance.BorderSize        = 0;
        btn.FlatAppearance.MouseOverBackColor = UITheme.Teal;
        btn.Click += (_, _) => action();
        _sidePanel.Controls.Add(btn);
        // Insert above logout; keep order by using BringToFront on appTitle
        btn.BringToFront();
    }

    private void ShowForm(Form child)
    {
        _currentChild?.Close();
        _currentChild      = child;
        child.TopLevel     = false;
        child.FormBorderStyle = FormBorderStyle.None;
        child.Dock         = DockStyle.Fill;
        _contentPanel.Controls.Clear();
        _contentPanel.Controls.Add(child);
        child.Show();
    }
}
