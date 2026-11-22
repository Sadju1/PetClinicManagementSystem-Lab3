using MySqlConnector;
using PetClinicLab3.Configuration;

namespace PetClinicLab3.Services;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        CreateTables();
        SeedSpecializations();
        SeedRooms();
    }

    public static bool TestConnection()
    {
        try
        {
            using var conn = new MySqlConnection(AppConfig.ConnectionString);
            conn.Open();
            return true;
        }
        catch { return false; }
    }

    public static bool AdminExists()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Role='Administrator'", conn);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static void CreateTables()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();

        var ddl = new[]
        {
            @"CREATE TABLE IF NOT EXISTS Specializations (
                SpecializationID   INT AUTO_INCREMENT PRIMARY KEY,
                SpecializationName VARCHAR(100) NOT NULL UNIQUE,
                Description        TEXT
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS Veterinarians (
                VeterinarianID INT AUTO_INCREMENT PRIMARY KEY,
                FirstName      VARCHAR(50)  NOT NULL,
                LastName       VARCHAR(50)  NOT NULL,
                Phone          VARCHAR(20),
                Email          VARCHAR(100),
                LicenseNumber  VARCHAR(50),
                IsActive       TINYINT(1)   NOT NULL DEFAULT 1,
                CreatedAt      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS VetSpecializations (
                VetSpecializationID INT AUTO_INCREMENT PRIMARY KEY,
                VeterinarianID      INT NOT NULL,
                SpecializationID    INT NOT NULL,
                UNIQUE KEY uq_vet_spec (VeterinarianID, SpecializationID),
                FOREIGN KEY (VeterinarianID)   REFERENCES Veterinarians(VeterinarianID),
                FOREIGN KEY (SpecializationID) REFERENCES Specializations(SpecializationID)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS Users (
                UserID         INT AUTO_INCREMENT PRIMARY KEY,
                Username       VARCHAR(50)  NOT NULL UNIQUE,
                Password       VARCHAR(255) NOT NULL,
                Role           VARCHAR(20)  NOT NULL,
                FullName       VARCHAR(100) NOT NULL,
                IsActive       TINYINT(1)   NOT NULL DEFAULT 1,
                VeterinarianID INT,
                CreatedAt      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (VeterinarianID) REFERENCES Veterinarians(VeterinarianID)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS Owners (
                OwnerID   INT AUTO_INCREMENT PRIMARY KEY,
                FirstName VARCHAR(50)  NOT NULL,
                LastName  VARCHAR(50)  NOT NULL,
                Phone     VARCHAR(20),
                Email     VARCHAR(100),
                Address   VARCHAR(255),
                IsActive  TINYINT(1)  NOT NULL DEFAULT 1,
                CreatedAt DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS Pets (
                PetID             INT AUTO_INCREMENT PRIMARY KEY,
                OwnerID           INT          NOT NULL,
                PetName           VARCHAR(100) NOT NULL,
                Species           VARCHAR(50)  NOT NULL,
                Breed             VARCHAR(100),
                DateOfBirth       DATE,
                IdentificationTag VARCHAR(50),
                Notes             TEXT,
                IsActive          TINYINT(1)  NOT NULL DEFAULT 1,
                CreatedAt         DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (OwnerID) REFERENCES Owners(OwnerID)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS ExaminationRooms (
                RoomID                 INT AUTO_INCREMENT PRIMARY KEY,
                RoomName               VARCHAR(100) NOT NULL,
                RoomNumber             VARCHAR(20),
                Description            TEXT,
                AssignedVeterinarianID INT,
                IsAvailable            TINYINT(1) NOT NULL DEFAULT 1,
                FOREIGN KEY (AssignedVeterinarianID) REFERENCES Veterinarians(VeterinarianID)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS Consultations (
                ConsultationID INT AUTO_INCREMENT PRIMARY KEY,
                PetID          INT          NOT NULL,
                VeterinarianID INT          NOT NULL,
                RoomID         INT,
                ScheduledDate  DATE         NOT NULL,
                ScheduledTime  TIME         NOT NULL,
                Status         VARCHAR(20)  NOT NULL DEFAULT 'Booked',
                Reason         VARCHAR(500),
                Notes          TEXT,
                CreatedAt      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (PetID)          REFERENCES Pets(PetID),
                FOREIGN KEY (VeterinarianID) REFERENCES Veterinarians(VeterinarianID),
                FOREIGN KEY (RoomID)         REFERENCES ExaminationRooms(RoomID)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

            @"CREATE TABLE IF NOT EXISTS MedicalRecords (
                RecordID        INT AUTO_INCREMENT PRIMARY KEY,
                ConsultationID  INT NOT NULL UNIQUE,
                Diagnosis       TEXT,
                Treatment       TEXT,
                Prescription    TEXT,
                FollowUpDate    DATE,
                Notes           TEXT,
                CreatedByUserID INT NOT NULL,
                CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (ConsultationID)  REFERENCES Consultations(ConsultationID),
                FOREIGN KEY (CreatedByUserID) REFERENCES Users(UserID)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",
        };

        foreach (var sql in ddl)
        {
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }

    private static void SeedSpecializations()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();

        var specs = new[]
        {
            ("General Practice",   "Routine care and preventive medicine"),
            ("Surgery",            "Surgical procedures and post-operative care"),
            ("Dermatology",        "Skin, coat, and ear conditions"),
            ("Cardiology",         "Heart and cardiovascular conditions"),
            ("Orthopedics",        "Bone, joint, and musculoskeletal conditions"),
            ("Ophthalmology",      "Eye conditions and vision care"),
            ("Dentistry",          "Dental care and oral health"),
            ("Emergency & Critical Care", "Emergency treatment and intensive care"),
        };

        const string sql = @"INSERT IGNORE INTO Specializations (SpecializationName, Description)
                             VALUES (@nm, @desc)";
        foreach (var (name, desc) in specs)
        {
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add("@nm",   MySqlDbType.VarChar, 100).Value = name;
            cmd.Parameters.Add("@desc", MySqlDbType.Text).Value         = desc;
            cmd.ExecuteNonQuery();
        }
    }

    private static void SeedRooms()
    {
        using var conn = new MySqlConnection(AppConfig.ConnectionString);
        conn.Open();

        // Only seed if no rooms exist
        using var check = new MySqlCommand("SELECT COUNT(*) FROM ExaminationRooms", conn);
        if (Convert.ToInt32(check.ExecuteScalar()) > 0) return;

        var rooms = new[]
        {
            ("Exam Room 1", "R-01", "General examination room"),
            ("Exam Room 2", "R-02", "General examination room"),
            ("Surgery Suite","S-01", "Surgical procedure room"),
        };

        const string sql = @"INSERT INTO ExaminationRooms (RoomName, RoomNumber, Description, IsAvailable)
                             VALUES (@nm, @num, @desc, 1)";
        foreach (var (name, num, desc) in rooms)
        {
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add("@nm",   MySqlDbType.VarChar, 100).Value = name;
            cmd.Parameters.Add("@num",  MySqlDbType.VarChar, 20).Value  = num;
            cmd.Parameters.Add("@desc", MySqlDbType.Text).Value         = desc;
            cmd.ExecuteNonQuery();
        }
    }
}
