using MySqlConnector;
using PetClinicLab3.Configuration;
using PetClinicLab3.Models;

namespace PetClinicLab3.Repositories;

public class UserRepository
{
    private static User MapUser(MySqlDataReader r) => new()
    {
        UserID         = r.GetInt32("UserID"),
        Username       = r.GetString("Username"),
        Password       = r.GetString("Password"),
        Role           = r.GetString("Role"),
        FullName       = r.GetString("FullName"),
        IsActive       = r.GetBoolean("IsActive"),
        VeterinarianID = r.IsDBNull(r.GetOrdinal("VeterinarianID")) ? null : r.GetInt32("VeterinarianID"),
        CreatedAt      = r.GetDateTime("CreatedAt"),
    };

    public List<User> GetAll()
    {
        var list = new List<User>();
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Users ORDER BY FullName", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(MapUser(reader));
        return list;
    }

    public User? GetById(int id)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Users WHERE UserID = @id", conn);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public User? GetByUsername(string username)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Users WHERE Username = @u AND IsActive = 1", conn);
        cmd.Parameters.Add("@u", MySqlDbType.VarChar, 50).Value = username;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public bool AdminExists()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Administrator'", conn);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int Insert(User u)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"INSERT INTO Users (Username,Password,Role,FullName,IsActive,VeterinarianID)
                             VALUES (@un,@pw,@role,@fn,@active,@vid);
                             SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@un",     MySqlDbType.VarChar, 50).Value  = u.Username;
        cmd.Parameters.Add("@pw",     MySqlDbType.VarChar, 255).Value = u.Password;
        cmd.Parameters.Add("@role",   MySqlDbType.VarChar, 20).Value  = u.Role;
        cmd.Parameters.Add("@fn",     MySqlDbType.VarChar, 100).Value = u.FullName;
        cmd.Parameters.Add("@active", MySqlDbType.Bool).Value         = u.IsActive;
        cmd.Parameters.Add("@vid",    MySqlDbType.Int32).Value        = (object?)u.VeterinarianID ?? DBNull.Value;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(User u)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        const string sql = @"UPDATE Users SET Username=@un, Role=@role, FullName=@fn,
                             IsActive=@active, VeterinarianID=@vid WHERE UserID=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@un",     MySqlDbType.VarChar, 50).Value  = u.Username;
        cmd.Parameters.Add("@role",   MySqlDbType.VarChar, 20).Value  = u.Role;
        cmd.Parameters.Add("@fn",     MySqlDbType.VarChar, 100).Value = u.FullName;
        cmd.Parameters.Add("@active", MySqlDbType.Bool).Value         = u.IsActive;
        cmd.Parameters.Add("@vid",    MySqlDbType.Int32).Value        = (object?)u.VeterinarianID ?? DBNull.Value;
        cmd.Parameters.Add("@id",     MySqlDbType.Int32).Value        = u.UserID;
        cmd.ExecuteNonQuery();
    }

    public void UpdatePassword(int userId, string hashedPassword)
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("UPDATE Users SET Password=@pw WHERE UserID=@id", conn);
        cmd.Parameters.Add("@pw", MySqlDbType.VarChar, 255).Value = hashedPassword;
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value        = userId;
        cmd.ExecuteNonQuery();
    }
}
