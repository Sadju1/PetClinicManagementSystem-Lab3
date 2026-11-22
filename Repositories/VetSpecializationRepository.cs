using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class VetSpecializationRepository
{
    private static VetSpecialization Map(MySqlDataReader r) => new()
    {
        VetSpecializationID = r.GetInt32("VetSpecializationID"),
        VeterinarianID      = r.GetInt32("VeterinarianID"),
        SpecializationID    = r.GetInt32("SpecializationID"),
        SpecializationName  = r.HasColumn("SpecializationName") && !r.IsDBNull(r.GetOrdinal("SpecializationName"))
                              ? r.GetString("SpecializationName") : "",
    };

    public List<VetSpecialization> GetByVeterinarian(int vetId)
    {
        var list = new List<VetSpecialization>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT vs.*, s.SpecializationName FROM VetSpecializations vs
                              JOIN Specializations s ON vs.SpecializationID=s.SpecializationID
                              WHERE vs.VeterinarianID=@vid ORDER BY s.SpecializationName";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@vid", MySqlDbType.Int32).Value = vetId;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public void Insert(int vetId, int specializationId)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT IGNORE INTO VetSpecializations (VeterinarianID, SpecializationID)
                             VALUES (@vid, @sid)";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@vid", MySqlDbType.Int32).Value = vetId;
        cmd.Parameters.Add("@sid", MySqlDbType.Int32).Value = specializationId;
        cmd.ExecuteNonQuery();
    }

    public void Delete(int vetSpecializationId)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            "DELETE FROM VetSpecializations WHERE VetSpecializationID=@id", conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = vetSpecializationId;
        cmd.ExecuteNonQuery();
    }

    public void DeleteByVetAndSpec(int vetId, int specializationId)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            "DELETE FROM VetSpecializations WHERE VeterinarianID=@vid AND SpecializationID=@sid", conn);
        cmd.Parameters.Add("@vid", MySqlDbType.Int32).Value = vetId;
        cmd.Parameters.Add("@sid", MySqlDbType.Int32).Value = specializationId;
        cmd.ExecuteNonQuery();
    }
}
