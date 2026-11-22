using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class ExaminationRoomRepository
{
    private static ExaminationRoom Map(MySqlDataReader r) => new()
    {
        RoomID                  = r.GetInt32("RoomID"),
        RoomName                = r.GetString("RoomName"),
        RoomNumber              = r.IsDBNull(r.GetOrdinal("RoomNumber"))    ? null : r.GetString("RoomNumber"),
        Description             = r.IsDBNull(r.GetOrdinal("Description"))   ? null : r.GetString("Description"),
        AssignedVeterinarianID  = r.IsDBNull(r.GetOrdinal("AssignedVeterinarianID")) ? null : r.GetInt32("AssignedVeterinarianID"),
        IsAvailable             = r.GetBoolean("IsAvailable"),
        AssignedVetName         = r.HasColumn("AssignedVetName") && !r.IsDBNull(r.GetOrdinal("AssignedVetName"))
                                  ? r.GetString("AssignedVetName") : null,
    };

    public List<ExaminationRoom> GetAll()
    {
        var list = new List<ExaminationRoom>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT r.*, CONCAT('Dr. ', v.FirstName,' ',v.LastName) AS AssignedVetName
                              FROM ExaminationRooms r
                              LEFT JOIN Veterinarians v ON r.AssignedVeterinarianID=v.VeterinarianID
                              ORDER BY r.RoomName";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public ExaminationRoom? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"SELECT r.*, CONCAT('Dr. ', v.FirstName,' ',v.LastName) AS AssignedVetName
                              FROM ExaminationRooms r
                              LEFT JOIN Veterinarians v ON r.AssignedVeterinarianID=v.VeterinarianID
                              WHERE r.RoomID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(ExaminationRoom room)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO ExaminationRooms (RoomName,RoomNumber,Description,AssignedVeterinarianID,IsAvailable)
                             VALUES (@nm,@num,@desc,@vid,@avail); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@nm",    MySqlDbType.VarChar, 100).Value = room.RoomName;
        cmd.Parameters.Add("@num",   MySqlDbType.VarChar, 20).Value  = (object?)room.RoomNumber ?? DBNull.Value;
        cmd.Parameters.Add("@desc",  MySqlDbType.Text).Value         = (object?)room.Description ?? DBNull.Value;
        cmd.Parameters.Add("@vid",   MySqlDbType.Int32).Value        = (object?)room.AssignedVeterinarianID ?? DBNull.Value;
        cmd.Parameters.Add("@avail", MySqlDbType.Bool).Value         = room.IsAvailable;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(ExaminationRoom room)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"UPDATE ExaminationRooms SET RoomName=@nm,RoomNumber=@num,Description=@desc,
                             AssignedVeterinarianID=@vid,IsAvailable=@avail WHERE RoomID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@nm",    MySqlDbType.VarChar, 100).Value = room.RoomName;
        cmd.Parameters.Add("@num",   MySqlDbType.VarChar, 20).Value  = (object?)room.RoomNumber ?? DBNull.Value;
        cmd.Parameters.Add("@desc",  MySqlDbType.Text).Value         = (object?)room.Description ?? DBNull.Value;
        cmd.Parameters.Add("@vid",   MySqlDbType.Int32).Value        = (object?)room.AssignedVeterinarianID ?? DBNull.Value;
        cmd.Parameters.Add("@avail", MySqlDbType.Bool).Value         = room.IsAvailable;
        cmd.Parameters.Add("@id",    MySqlDbType.Int32).Value        = room.RoomID;
        cmd.ExecuteNonQuery();
    }

    public int GetCount()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM ExaminationRooms", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
