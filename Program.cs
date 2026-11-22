using PetClinicLab3.Services;
using PetClinicLab3.Views;

namespace PetClinicLab3;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        if (!DatabaseInitializer.TestConnection())
        {
            MessageBox.Show(
                "Cannot connect to the MariaDB database.\n\n" +
                "Please ensure XAMPP is running with MySQL/MariaDB on port 3306, " +
                "then restart the application.",
                "Database Connection Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        DatabaseInitializer.Initialize();

        if (!DatabaseInitializer.AdminExists())
        {
            Application.Run(new SetupForm());
            if (!DatabaseInitializer.AdminExists()) return;
        }

        Application.Run(new LoginForm());
    }
}
