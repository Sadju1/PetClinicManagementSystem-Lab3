using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class MedicalRecordRepository
{
    private static MedicalRecord Map(MySqlDataReader r) => new()
    {
        RecordID        = r.GetInt32("RecordID"),
        ConsultationID  = r.GetInt32("ConsultationID"),
        Diagnosis       = r.IsDBNull(r.GetOrdinal("Diagnosis"))    ? null : r.GetString("Diagnosis"),
        Treatment       = r.IsDBNull(r.GetOrdinal("Treatment"))    ? null : r.GetString("Treatment"),
        Prescription    = r.IsDBNull(r.GetOrdinal("Prescription")) ? null : r.GetString("Prescription"),
        FollowUpDate    = r.IsDBNull(r.GetOrdinal("FollowUpDate")) ? null
                          : DateOnly.FromDateTime(r.GetDateTime("FollowUpDate")),
        Notes           = r.IsDBNull(r.GetOrdinal("Notes"))        ? null : r.GetString("Notes"),
        CreatedByUserID = r.GetInt32("CreatedByUserID"),
        CreatedAt       = r.GetDateTime("CreatedAt"),
        PetName         = r.HasColumn("PetName")    && !r.IsDBNull(r.GetOrdinal("PetName"))    ? r.GetString("PetName")    : "",
        OwnerName       = r.HasColumn("OwnerName")  && !r.IsDBNull(r.GetOrdinal("OwnerName"))  ? r.GetString("OwnerName")  : "",
        VetName         = r.HasColumn("VetName")    && !r.IsDBNull(r.GetOrdinal("VetName"))    ? r.GetString("VetName")    : "",
        CreatedByName   = r.HasColumn("CreatedByName") && !r.IsDBNull(r.GetOrdinal("CreatedByName")) ? r.GetString("CreatedByName") : "",
        ScheduledDate   = r.HasColumn("ScheduledDate") && !r.IsDBNull(r.GetOrdinal("ScheduledDate"))
                          ? DateOnly.FromDateTime(r.GetDateTime("ScheduledDate")) : DateOnly.MinValue,
    };

    private const string BaseSelect = @"
        SELECT mr.*,
               p.PetName,
               CONCAT(o.FirstName,' ',o.LastName) AS OwnerName,
               CONCAT('Dr. ',v.FirstName,' ',v.LastName) AS VetName,
               u.FullName AS CreatedByName,
               c.ScheduledDate
        FROM MedicalRecords mr
        JOIN Consultations c  ON mr.ConsultationID=c.ConsultationID
        JOIN Pets p           ON c.PetID=p.PetID
        JOIN Owners o         ON p.OwnerID=o.OwnerID
        JOIN Veterinarians v  ON c.VeterinarianID=v.VeterinarianID
        JOIN Users u          ON mr.CreatedByUserID=u.UserID";

    public List<MedicalRecord> GetAll()
    {
        var list = new List<MedicalRecord>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(BaseSelect + " ORDER BY mr.CreatedAt DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<MedicalRecord> GetByPet(int petId)
    {
        var list = new List<MedicalRecord>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            BaseSelect + " WHERE p.PetID=@pid ORDER BY mr.CreatedAt DESC", conn);
        cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value = petId;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public MedicalRecord? GetByConsultation(int consultationId)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            BaseSelect + " WHERE mr.ConsultationID=@cid", conn);
        cmd.Parameters.Add("@cid", MySqlDbType.Int32).Value = consultationId;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public MedicalRecord? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(BaseSelect + " WHERE mr.RecordID=@id", conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(MedicalRecord mr)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO MedicalRecords
                             (ConsultationID,Diagnosis,Treatment,Prescription,FollowUpDate,Notes,CreatedByUserID)
                             VALUES (@cid,@diag,@treat,@rx,@fu,@nt,@uid); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@cid",   MySqlDbType.Int32).Value        = mr.ConsultationID;
        cmd.Parameters.Add("@diag",  MySqlDbType.Text).Value         = (object?)mr.Diagnosis    ?? DBNull.Value;
        cmd.Parameters.Add("@treat", MySqlDbType.Text).Value         = (object?)mr.Treatment    ?? DBNull.Value;
        cmd.Parameters.Add("@rx",    MySqlDbType.Text).Value         = (object?)mr.Prescription ?? DBNull.Value;
        cmd.Parameters.Add("@fu",    MySqlDbType.Date).Value         = mr.FollowUpDate.HasValue
                                                                       ? (object)mr.FollowUpDate.Value.ToDateTime(TimeOnly.MinValue)
                                                                       : DBNull.Value;
        cmd.Parameters.Add("@nt",    MySqlDbType.Text).Value         = (object?)mr.Notes        ?? DBNull.Value;
        cmd.Parameters.Add("@uid",   MySqlDbType.Int32).Value        = mr.CreatedByUserID;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(MedicalRecord mr)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"UPDATE MedicalRecords SET Diagnosis=@diag,Treatment=@treat,
                             Prescription=@rx,FollowUpDate=@fu,Notes=@nt WHERE RecordID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@diag",  MySqlDbType.Text).Value = (object?)mr.Diagnosis    ?? DBNull.Value;
        cmd.Parameters.Add("@treat", MySqlDbType.Text).Value = (object?)mr.Treatment    ?? DBNull.Value;
        cmd.Parameters.Add("@rx",    MySqlDbType.Text).Value = (object?)mr.Prescription ?? DBNull.Value;
        cmd.Parameters.Add("@fu",    MySqlDbType.Date).Value = mr.FollowUpDate.HasValue
                                                              ? (object)mr.FollowUpDate.Value.ToDateTime(TimeOnly.MinValue)
                                                              : DBNull.Value;
        cmd.Parameters.Add("@nt",    MySqlDbType.Text).Value = (object?)mr.Notes ?? DBNull.Value;
        cmd.Parameters.Add("@id",    MySqlDbType.Int32).Value = mr.RecordID;
        cmd.ExecuteNonQuery();
    }

    public List<MedicalRecord> Search(string term)
    {
        var list = new List<MedicalRecord>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"
            SELECT mr.*,
                   p.PetName,
                   CONCAT(o.FirstName,' ',o.LastName) AS OwnerName,
                   CONCAT('Dr. ',v.FirstName,' ',v.LastName) AS VetName,
                   u.FullName AS CreatedByName,
                   c.ScheduledDate
            FROM MedicalRecords mr
            JOIN Consultations c  ON mr.ConsultationID=c.ConsultationID
            JOIN Pets p           ON c.PetID=p.PetID
            JOIN Owners o         ON p.OwnerID=o.OwnerID
            JOIN Veterinarians v  ON c.VeterinarianID=v.VeterinarianID
            JOIN Users u          ON mr.CreatedByUserID=u.UserID
            WHERE p.PetName LIKE @term
               OR CONCAT(o.FirstName,' ',o.LastName) LIKE @term
               OR CONCAT('Dr. ',v.FirstName,' ',v.LastName) LIKE @term
            ORDER BY mr.CreatedAt DESC";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@term", MySqlDbType.VarChar).Value = $"%{term}%";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public int GetCount()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM MedicalRecords", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
