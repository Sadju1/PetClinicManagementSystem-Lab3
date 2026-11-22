using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class PetRepository
{
    private static Pet Map(MySqlDataReader r) => new()
    {
        PetID             = r.GetInt32("PetID"),
        OwnerID           = r.GetInt32("OwnerID"),
        PetName           = r.GetString("PetName"),
        Species           = r.GetString("Species"),
        Breed             = r.IsDBNull(r.GetOrdinal("Breed"))             ? null : r.GetString("Breed"),
        DateOfBirth       = r.IsDBNull(r.GetOrdinal("DateOfBirth"))       ? null : r.GetDateTime("DateOfBirth"),
        IdentificationTag = r.IsDBNull(r.GetOrdinal("IdentificationTag")) ? null : r.GetString("IdentificationTag"),
        Notes             = r.IsDBNull(r.GetOrdinal("Notes"))             ? null : r.GetString("Notes"),
        IsActive          = r.GetBoolean("IsActive"),
        CreatedAt         = r.GetDateTime("CreatedAt"),
        OwnerName         = r.HasColumn("OwnerName") && !r.IsDBNull(r.GetOrdinal("OwnerName"))
                            ? r.GetString("OwnerName") : "",
    };

    public List<Pet> GetAll(bool includeInactive = false)
    {
        var list = new List<Pet>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        string sql = @"SELECT p.*, CONCAT(o.FirstName,' ',o.LastName) AS OwnerName
                       FROM Pets p JOIN Owners o ON p.OwnerID=o.OwnerID"
                     + (includeInactive ? "" : " WHERE p.IsActive=1")
                     + " ORDER BY p.PetName";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<Pet> GetByOwner(int ownerId)
    {
        var list = new List<Pet>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT p.*, CONCAT(o.FirstName,' ',o.LastName) AS OwnerName
                              FROM Pets p JOIN Owners o ON p.OwnerID=o.OwnerID
                              WHERE p.OwnerID=@oid ORDER BY p.PetName";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@oid", MySqlDbType.Int32).Value = ownerId;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<Pet> Search(string term)
    {
        var list = new List<Pet>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT p.*, CONCAT(o.FirstName,' ',o.LastName) AS OwnerName
                              FROM Pets p JOIN Owners o ON p.OwnerID=o.OwnerID
                              WHERE p.PetName LIKE @t OR p.Species LIKE @t OR p.IdentificationTag LIKE @t
                              OR o.FirstName LIKE @t OR o.LastName LIKE @t
                              ORDER BY p.PetName";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@t", MySqlDbType.VarChar).Value = $"%{term}%";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Pet? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT p.*, CONCAT(o.FirstName,' ',o.LastName) AS OwnerName
                              FROM Pets p JOIN Owners o ON p.OwnerID=o.OwnerID WHERE p.PetID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(Pet p)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO Pets (OwnerID,PetName,Species,Breed,DateOfBirth,IdentificationTag,Notes,IsActive)
                             VALUES (@oid,@nm,@sp,@br,@dob,@tag,@nt,@ac); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@oid", MySqlDbType.Int32).Value        = p.OwnerID;
        cmd.Parameters.Add("@nm",  MySqlDbType.VarChar, 100).Value = p.PetName;
        cmd.Parameters.Add("@sp",  MySqlDbType.VarChar, 50).Value  = p.Species;
        cmd.Parameters.Add("@br",  MySqlDbType.VarChar, 100).Value = (object?)p.Breed ?? DBNull.Value;
        cmd.Parameters.Add("@dob", MySqlDbType.Date).Value         = (object?)p.DateOfBirth ?? DBNull.Value;
        cmd.Parameters.Add("@tag", MySqlDbType.VarChar, 50).Value  = (object?)p.IdentificationTag ?? DBNull.Value;
        cmd.Parameters.Add("@nt",  MySqlDbType.Text).Value         = (object?)p.Notes ?? DBNull.Value;
        cmd.Parameters.Add("@ac",  MySqlDbType.Bool).Value         = p.IsActive;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Pet p)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"UPDATE Pets SET OwnerID=@oid,PetName=@nm,Species=@sp,Breed=@br,
                             DateOfBirth=@dob,IdentificationTag=@tag,Notes=@nt,IsActive=@ac WHERE PetID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@oid", MySqlDbType.Int32).Value        = p.OwnerID;
        cmd.Parameters.Add("@nm",  MySqlDbType.VarChar, 100).Value = p.PetName;
        cmd.Parameters.Add("@sp",  MySqlDbType.VarChar, 50).Value  = p.Species;
        cmd.Parameters.Add("@br",  MySqlDbType.VarChar, 100).Value = (object?)p.Breed ?? DBNull.Value;
        cmd.Parameters.Add("@dob", MySqlDbType.Date).Value         = (object?)p.DateOfBirth ?? DBNull.Value;
        cmd.Parameters.Add("@tag", MySqlDbType.VarChar, 50).Value  = (object?)p.IdentificationTag ?? DBNull.Value;
        cmd.Parameters.Add("@nt",  MySqlDbType.Text).Value         = (object?)p.Notes ?? DBNull.Value;
        cmd.Parameters.Add("@ac",  MySqlDbType.Bool).Value         = p.IsActive;
        cmd.Parameters.Add("@id",  MySqlDbType.Int32).Value        = p.PetID;
        cmd.ExecuteNonQuery();
    }

    public int GetActiveCount()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Pets WHERE IsActive=1", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}

// Extension helper
public static class DataReaderExtensions
{
    public static bool HasColumn(this MySqlDataReader reader, string name)
    {
        for (int i = 0; i < reader.FieldCount; i++)
            if (reader.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
