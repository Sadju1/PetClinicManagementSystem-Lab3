namespace PetClinicLab3.Configuration;

public static class AppConfig
{
    public static string ConnectionString { get; private set; } =
        "Server=127.0.0.1;Port=3306;Database=PetClinicDB;User ID=root;Password=;AllowZeroDateTime=true;ConvertZeroDateTime=true;";

    public static void SetConnectionString(string cs) => ConnectionString = cs;
}
