using PetClinicLab3.Controllers;
using PetClinicLab3.Models;
using PetClinicLab3.Services;

namespace PetClinicLab3.Views;

public class DashboardForm : Form
{
    private readonly DataGridView _scheduleGrid = new();
    private readonly Label        _lblNoAppts   = new();

    public DashboardForm()
    {
        BackColor = UITheme.LightGray;

        var header = UITheme.HeaderPanel("Dashboard",
            $"Welcome, {AuthService.CurrentUser?.FullName}  |  {DateTime.Now:dddd, MMMM d, yyyy}");

        // Stats strip (DockStyle.Top)
        var statsPanel = new FlowLayoutPanel
        {
            Dock         = DockStyle.Top,
            Height       = 130,
            Padding      = new Padding(16, 12, 16, 8),
            BackColor    = UITheme.LightGray,
            WrapContents = false,
        };

        // Schedule section header
        var scheduleHeader = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 40,
            BackColor = Color.FromArgb(235, 240, 248),
            Padding   = new Padding(16, 0, 0, 0),
        };
        scheduleHeader.Controls.Add(new Label
        {
            Text      = "Today's Appointment Schedule",
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = UITheme.Navy,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        });

        // Schedule grid
        _scheduleGrid.Dock                = DockStyle.Fill;
        _scheduleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _scheduleGrid.ReadOnly            = true;
        _scheduleGrid.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
        _scheduleGrid.AllowUserToAddRows  = false;
        _scheduleGrid.RowHeadersVisible   = false;
        _scheduleGrid.BackgroundColor     = UITheme.White;
        _scheduleGrid.Font                = UITheme.LabelFont;
        _scheduleGrid.BorderStyle         = BorderStyle.None;
        _scheduleGrid.GridColor           = Color.FromArgb(220, 220, 220);
        _scheduleGrid.ColumnHeadersDefaultCellStyle.BackColor = UITheme.Navy;
        _scheduleGrid.ColumnHeadersDefaultCellStyle.ForeColor = UITheme.White;
        _scheduleGrid.ColumnHeadersDefaultCellStyle.Font      = UITheme.ButtonFont;
        _scheduleGrid.EnableHeadersVisualStyles = false;

        // Empty state label (shown when no appointments today)
        _lblNoAppts.Text      = "No appointments scheduled for today.";
        _lblNoAppts.Font      = new Font("Segoe UI", 11f, FontStyle.Italic);
        _lblNoAppts.ForeColor = Color.Gray;
        _lblNoAppts.Dock      = DockStyle.Fill;
        _lblNoAppts.TextAlign = ContentAlignment.MiddleCenter;
        _lblNoAppts.Visible   = false;

        // Outer content panel holds schedule header + grid
        var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.White, Padding = new Padding(16, 0, 16, 16) };
        contentPanel.Controls.Add(_scheduleGrid);
        contentPanel.Controls.Add(_lblNoAppts);
        contentPanel.Controls.Add(scheduleHeader);

        // Form z-order: Fill first, then Top panels, then header last
        Controls.Add(contentPanel);
        Controls.Add(statsPanel);
        Controls.Add(header);

        Load += (_, _) => LoadDashboard(statsPanel);
    }

    private void LoadDashboard(FlowLayoutPanel statsPanel)
    {
        try
        {
            var conCtrl   = new ConsultationController();
            var ownerCtrl = new OwnerController();
            var petCtrl   = new PetController();

            int totalOwners    = ownerCtrl.GetCount();
            int totalPets      = petCtrl.GetActiveCount();
            int todayTotal     = conCtrl.GetTodayCount();
            int todayAwaiting  = conCtrl.GetTodayAwaitingCount();
            int todayCompleted = conCtrl.GetTodayCountByStatus("Treated");

            statsPanel.Controls.Add(MakeCard("Registered Owners",   totalOwners.ToString(),    UITheme.Teal));
            statsPanel.Controls.Add(MakeCard("Active Pets",         totalPets.ToString(),       UITheme.Navy));
            statsPanel.Controls.Add(MakeCard("Today's Appointments",todayTotal.ToString(),      Color.FromArgb(70, 130, 180)));
            statsPanel.Controls.Add(MakeCard("Awaiting / In-Room",  todayAwaiting.ToString(),   Color.FromArgb(205, 100, 20)));
            statsPanel.Controls.Add(MakeCard("Completed Today",     todayCompleted.ToString(),  Color.FromArgb(0, 140, 70)));

            LoadScheduleGrid(conCtrl);
        }
        catch (Exception ex)
        {
            statsPanel.Controls.Add(new Label
            {
                Text      = $"Database unavailable: {ex.Message}",
                ForeColor = UITheme.ErrorRed,
                AutoSize  = true,
                Font      = UITheme.LabelFont,
            });
            _lblNoAppts.Text    = "Could not load today's schedule — database unavailable.";
            _lblNoAppts.Visible = true;
            _scheduleGrid.Visible = false;
        }
    }

    private void LoadScheduleGrid(ConsultationController conCtrl)
    {
        var today = conCtrl.GetTodayConsultations();

        if (today.Count == 0)
        {
            _scheduleGrid.Visible = false;
            _lblNoAppts.Visible   = true;
            return;
        }

        _scheduleGrid.DataSource = today.Select(c => new
        {
            Time      = $"{c.ScheduledTime:hh\\:mm}",
            Pet       = c.PetName,
            Owner     = c.OwnerName,
            Vet       = c.VetName,
            Room      = c.RoomNumber ?? "—",
            c.Status,
        }).ToList();

        // Color rows by status
        foreach (DataGridViewRow row in _scheduleGrid.Rows)
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

    private static Panel MakeCard(string label, string value, Color accent)
    {
        var card = new Panel
        {
            Size      = new Size(185, 100),
            BackColor = UITheme.White,
            Margin    = new Padding(0, 0, 14, 0),
        };

        var accentBar = new Panel { Height = 5, Dock = DockStyle.Top, BackColor = accent };

        var lblValue = new Label
        {
            Text      = value,
            Font      = new Font("Segoe UI", 26f, FontStyle.Bold),
            ForeColor = accent,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
        };

        var lblTitle = new Label
        {
            Text      = label,
            Font      = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(90, 90, 90),
            Dock      = DockStyle.Bottom,
            Height    = 26,
            TextAlign = ContentAlignment.MiddleCenter,
        };

        card.Controls.Add(lblValue);
        card.Controls.Add(lblTitle);
        card.Controls.Add(accentBar);
        return card;
    }
}
