using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class ConsultationRepository
{
    private static Consultation Map(MySqlDataReader r) => new()
    {
        ConsultationID  = r.GetInt32("ConsultationID"),
        PetID           = r.GetInt32("PetID"),
        VeterinarianID  = r.GetInt32("VeterinarianID"),
        RoomID          = r.IsDBNull(r.GetOrdinal("RoomID")) ? null : r.GetInt32("RoomID"),
        ScheduledDate   = DateOnly.FromDateTime(r.GetDateTime("ScheduledDate")),
        ScheduledTime   = r.GetTimeSpan("ScheduledTime"),
        Status          = r.GetString("Status"),
        Reason          = r.IsDBNull(r.GetOrdinal("Reason")) ? null : r.GetString("Reason"),
        Notes           = r.IsDBNull(r.GetOrdinal("Notes"))  ? null : r.GetString("Notes"),
        CreatedAt       = r.GetDateTime("CreatedAt"),
        PetName         = r.HasColumn("PetName")    && !r.IsDBNull(r.GetOrdinal("PetName"))    ? r.GetString("PetName")    : "",
        OwnerName       = r.HasColumn("OwnerName")  && !r.IsDBNull(r.GetOrdinal("OwnerName"))  ? r.GetString("OwnerName")  : "",
        VetName         = r.HasColumn("VetName")    && !r.IsDBNull(r.GetOrdinal("VetName"))    ? r.GetString("VetName")    : "",
        RoomNumber      = r.HasColumn("RoomNumber") && !r.IsDBNull(r.GetOrdinal("RoomNumber")) ? r.GetString("RoomNumber") : null,
    };

    private const string BaseSelect = @"
        SELECT c.*,
               p.PetName,
               CONCAT(o.FirstName,' ',o.LastName) AS OwnerName,
               CONCAT('Dr. ',v.FirstName,' ',v.LastName) AS VetName,
               r.RoomNumber
        FROM Consultations c
        JOIN Pets p         ON c.PetID=p.PetID
        JOIN Owners o       ON p.OwnerID=o.OwnerID
        JOIN Veterinarians v ON c.VeterinarianID=v.VeterinarianID
        LEFT JOIN ExaminationRooms r ON c.RoomID=r.RoomID";

    public List<Consultation> GetAll()
    {
        var list = new List<Consultation>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(BaseSelect + " ORDER BY c.ScheduledDate DESC, c.ScheduledTime DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<Consultation> GetByDate(DateOnly date)
    {
        var list = new List<Consultation>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(BaseSelect + " WHERE c.ScheduledDate=@d ORDER BY c.ScheduledTime", conn);
        cmd.Parameters.Add("@d", MySqlDbType.Date).Value = date.ToDateTime(TimeOnly.MinValue);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<Consultation> GetByVeterinarian(int vetId)
    {
        var list = new List<Consultation>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            BaseSelect + " WHERE c.VeterinarianID=@vid ORDER BY c.ScheduledDate DESC, c.ScheduledTime", conn);
        cmd.Parameters.Add("@vid", MySqlDbType.Int32).Value = vetId;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<Consultation> GetByPet(int petId)
    {
        var list = new List<Consultation>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            BaseSelect + " WHERE c.PetID=@pid ORDER BY c.ScheduledDate DESC, c.ScheduledTime", conn);
        cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value = petId;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Consultation? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(BaseSelect + " WHERE c.ConsultationID=@id", conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public bool HasOverlap(int vetId, DateOnly date, TimeSpan time, int? excludeId = null)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        string sql = @"SELECT COUNT(*) FROM Consultations
                       WHERE VeterinarianID=@vid AND ScheduledDate=@d AND ScheduledTime=@t
                       AND Status NOT IN ('Cancelled','Missed')"
                     + (excludeId.HasValue ? " AND ConsultationID<>@excl" : "");
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@vid", MySqlDbType.Int32).Value = vetId;
        cmd.Parameters.Add("@d",   MySqlDbType.Date).Value  = date.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@t",   MySqlDbType.Time).Value  = time;
        if (excludeId.HasValue)
            cmd.Parameters.Add("@excl", MySqlDbType.Int32).Value = excludeId.Value;
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int Insert(Consultation c)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO Consultations (PetID,VeterinarianID,RoomID,ScheduledDate,ScheduledTime,Status,Reason,Notes)
                             VALUES (@pid,@vid,@rid,@d,@t,@st,@rsn,@nt); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value        = c.PetID;
        cmd.Parameters.Add("@vid", MySqlDbType.Int32).Value        = c.VeterinarianID;
        cmd.Parameters.Add("@rid", MySqlDbType.Int32).Value        = (object?)c.RoomID ?? DBNull.Value;
        cmd.Parameters.Add("@d",   MySqlDbType.Date).Value         = c.ScheduledDate.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@t",   MySqlDbType.Time).Value         = c.ScheduledTime;
        cmd.Parameters.Add("@st",  MySqlDbType.VarChar, 20).Value  = c.Status;
        cmd.Parameters.Add("@rsn", MySqlDbType.VarChar, 500).Value = (object?)c.Reason ?? DBNull.Value;
        cmd.Parameters.Add("@nt",  MySqlDbType.Text).Value         = (object?)c.Notes ?? DBNull.Value;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void UpdateStatus(int id, string status)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            "UPDATE Consultations SET Status=@st WHERE ConsultationID=@id", conn);
        cmd.Parameters.Add("@st", MySqlDbType.VarChar, 20).Value = status;
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value       = id;
        cmd.ExecuteNonQuery();
    }

    public void Update(Consultation c)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"UPDATE Consultations SET PetID=@pid,VeterinarianID=@vid,RoomID=@rid,
                             ScheduledDate=@d,ScheduledTime=@t,Status=@st,Reason=@rsn,Notes=@nt
                             WHERE ConsultationID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value        = c.PetID;
        cmd.Parameters.Add("@vid", MySqlDbType.Int32).Value        = c.VeterinarianID;
        cmd.Parameters.Add("@rid", MySqlDbType.Int32).Value        = (object?)c.RoomID ?? DBNull.Value;
        cmd.Parameters.Add("@d",   MySqlDbType.Date).Value         = c.ScheduledDate.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@t",   MySqlDbType.Time).Value         = c.ScheduledTime;
        cmd.Parameters.Add("@st",  MySqlDbType.VarChar, 20).Value  = c.Status;
        cmd.Parameters.Add("@rsn", MySqlDbType.VarChar, 500).Value = (object?)c.Reason ?? DBNull.Value;
        cmd.Parameters.Add("@nt",  MySqlDbType.Text).Value         = (object?)c.Notes ?? DBNull.Value;
        cmd.Parameters.Add("@id",  MySqlDbType.Int32).Value        = c.ConsultationID;
        cmd.ExecuteNonQuery();
    }

    public int GetTodayCount()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM Consultations WHERE ScheduledDate=CURDATE()", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetCountByStatus(string status)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM Consultations WHERE Status=@st", conn);
        cmd.Parameters.Add("@st", MySqlDbType.VarChar, 20).Value = status;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetTodayCountByStatus(string status)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM Consultations WHERE ScheduledDate=CURDATE() AND Status=@st", conn);
        cmd.Parameters.Add("@st", MySqlDbType.VarChar, 20).Value = status;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetTodayAwaitingCount()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM Consultations WHERE ScheduledDate=CURDATE() AND Status IN ('Booked','Arrived')", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<Consultation> GetTodayConsultations()
    {
        var list = new List<Consultation>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(BaseSelect + " WHERE c.ScheduledDate=CURDATE() ORDER BY c.ScheduledTime", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }
}
