using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class VeterinarianRepository
{
    private static Veterinarian Map(MySqlDataReader r) => new()
    {
        VeterinarianID = r.GetInt32("VeterinarianID"),
        FirstName      = r.GetString("FirstName"),
        LastName       = r.GetString("LastName"),
        Phone          = r.IsDBNull(r.GetOrdinal("Phone"))         ? null : r.GetString("Phone"),
        Email          = r.IsDBNull(r.GetOrdinal("Email"))         ? null : r.GetString("Email"),
        LicenseNumber  = r.IsDBNull(r.GetOrdinal("LicenseNumber")) ? null : r.GetString("LicenseNumber"),
        IsActive       = r.GetBoolean("IsActive"),
        CreatedAt      = r.GetDateTime("CreatedAt"),
    };

    public List<Veterinarian> GetAll(bool includeInactive = false)
    {
        var list = new List<Veterinarian>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        string sql = "SELECT * FROM Veterinarians"
                     + (includeInactive ? "" : " WHERE IsActive=1")
                     + " ORDER BY LastName, FirstName";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Veterinarian? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Veterinarians WHERE VeterinarianID=@id", conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public List<Veterinarian> Search(string term)
    {
        var list = new List<Veterinarian>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT * FROM Veterinarians
                              WHERE FirstName LIKE @t OR LastName LIKE @t OR LicenseNumber LIKE @t OR Email LIKE @t
                              ORDER BY LastName, FirstName";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@t", MySqlDbType.VarChar).Value = $"%{term}%";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public int Insert(Veterinarian v)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO Veterinarians (FirstName,LastName,Phone,Email,LicenseNumber,IsActive)
                             VALUES (@fn,@ln,@ph,@em,@lic,@ac); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@fn",  MySqlDbType.VarChar, 50).Value  = v.FirstName;
        cmd.Parameters.Add("@ln",  MySqlDbType.VarChar, 50).Value  = v.LastName;
        cmd.Parameters.Add("@ph",  MySqlDbType.VarChar, 20).Value  = (object?)v.Phone ?? DBNull.Value;
        cmd.Parameters.Add("@em",  MySqlDbType.VarChar, 100).Value = (object?)v.Email ?? DBNull.Value;
        cmd.Parameters.Add("@lic", MySqlDbType.VarChar, 50).Value  = (object?)v.LicenseNumber ?? DBNull.Value;
        cmd.Parameters.Add("@ac",  MySqlDbType.Bool).Value         = v.IsActive;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Veterinarian v)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"UPDATE Veterinarians SET FirstName=@fn,LastName=@ln,Phone=@ph,
                             Email=@em,LicenseNumber=@lic,IsActive=@ac WHERE VeterinarianID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@fn",  MySqlDbType.VarChar, 50).Value  = v.FirstName;
        cmd.Parameters.Add("@ln",  MySqlDbType.VarChar, 50).Value  = v.LastName;
        cmd.Parameters.Add("@ph",  MySqlDbType.VarChar, 20).Value  = (object?)v.Phone ?? DBNull.Value;
        cmd.Parameters.Add("@em",  MySqlDbType.VarChar, 100).Value = (object?)v.Email ?? DBNull.Value;
        cmd.Parameters.Add("@lic", MySqlDbType.VarChar, 50).Value  = (object?)v.LicenseNumber ?? DBNull.Value;
        cmd.Parameters.Add("@ac",  MySqlDbType.Bool).Value         = v.IsActive;
        cmd.Parameters.Add("@id",  MySqlDbType.Int32).Value        = v.VeterinarianID;
        cmd.ExecuteNonQuery();
    }

    public int GetActiveCount()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Veterinarians WHERE IsActive=1", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
