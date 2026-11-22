using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class OwnerRepository
{
    private static Owner Map(MySqlDataReader r) => new()
    {
        OwnerID   = r.GetInt32("OwnerID"),
        FirstName = r.GetString("FirstName"),
        LastName  = r.GetString("LastName"),
        Phone     = r.IsDBNull(r.GetOrdinal("Phone"))   ? null : r.GetString("Phone"),
        Email     = r.IsDBNull(r.GetOrdinal("Email"))   ? null : r.GetString("Email"),
        Address   = r.IsDBNull(r.GetOrdinal("Address")) ? null : r.GetString("Address"),
        IsActive  = r.GetBoolean("IsActive"),
        CreatedAt = r.GetDateTime("CreatedAt"),
    };

    public List<Owner> GetAll(bool includeInactive = false)
    {
        var list = new List<Owner>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        string sql = includeInactive
            ? "SELECT * FROM Owners ORDER BY LastName, FirstName"
            : "SELECT * FROM Owners WHERE IsActive=1 ORDER BY LastName, FirstName";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<Owner> Search(string term)
    {
        var list = new List<Owner>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT * FROM Owners
            WHERE (FirstName LIKE @t OR LastName LIKE @t OR Phone LIKE @t OR Email LIKE @t)
            ORDER BY LastName, FirstName";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@t", MySqlDbType.VarChar).Value = $"%{term}%";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Owner? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Owners WHERE OwnerID=@id", conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(Owner o)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO Owners (FirstName,LastName,Phone,Email,Address,IsActive)
                             VALUES (@fn,@ln,@ph,@em,@ad,@ac); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@fn", MySqlDbType.VarChar, 50).Value  = o.FirstName;
        cmd.Parameters.Add("@ln", MySqlDbType.VarChar, 50).Value  = o.LastName;
        cmd.Parameters.Add("@ph", MySqlDbType.VarChar, 20).Value  = (object?)o.Phone ?? DBNull.Value;
        cmd.Parameters.Add("@em", MySqlDbType.VarChar, 100).Value = (object?)o.Email ?? DBNull.Value;
        cmd.Parameters.Add("@ad", MySqlDbType.VarChar, 255).Value = (object?)o.Address ?? DBNull.Value;
        cmd.Parameters.Add("@ac", MySqlDbType.Bool).Value         = o.IsActive;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Owner o)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"UPDATE Owners SET FirstName=@fn,LastName=@ln,Phone=@ph,
                             Email=@em,Address=@ad,IsActive=@ac WHERE OwnerID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@fn", MySqlDbType.VarChar, 50).Value  = o.FirstName;
        cmd.Parameters.Add("@ln", MySqlDbType.VarChar, 50).Value  = o.LastName;
        cmd.Parameters.Add("@ph", MySqlDbType.VarChar, 20).Value  = (object?)o.Phone ?? DBNull.Value;
        cmd.Parameters.Add("@em", MySqlDbType.VarChar, 100).Value = (object?)o.Email ?? DBNull.Value;
        cmd.Parameters.Add("@ad", MySqlDbType.VarChar, 255).Value = (object?)o.Address ?? DBNull.Value;
        cmd.Parameters.Add("@ac", MySqlDbType.Bool).Value         = o.IsActive;
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value        = o.OwnerID;
        cmd.ExecuteNonQuery();
    }

    public int GetCount()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Owners WHERE IsActive=1", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
