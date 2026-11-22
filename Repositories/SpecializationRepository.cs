using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class SpecializationRepository
{
    private static Specialization Map(MySqlDataReader r) => new()
    {
        SpecializationID   = r.GetInt32("SpecializationID"),
        SpecializationName = r.GetString("SpecializationName"),
        Description        = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString("Description"),
    };

    public List<Specialization> GetAll()
    {
        var list = new List<Specialization>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Specializations ORDER BY SpecializationName", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Specialization? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Specializations WHERE SpecializationID=@id", conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(Specialization s)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO Specializations (SpecializationName, Description)
                             VALUES (@nm,@desc); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@nm",   MySqlDbType.VarChar, 100).Value = s.SpecializationName;
        cmd.Parameters.Add("@desc", MySqlDbType.Text).Value         = (object?)s.Description ?? DBNull.Value;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Specialization s)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = "UPDATE Specializations SET SpecializationName=@nm, Description=@desc WHERE SpecializationID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@nm",   MySqlDbType.VarChar, 100).Value = s.SpecializationName;
        cmd.Parameters.Add("@desc", MySqlDbType.Text).Value         = (object?)s.Description ?? DBNull.Value;
        cmd.Parameters.Add("@id",   MySqlDbType.Int32).Value        = s.SpecializationID;
        cmd.ExecuteNonQuery();
    }
}
