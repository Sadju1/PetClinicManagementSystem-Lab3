namespace PetClinicLab3.Models;

public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public string FullName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public int? VeterinarianID { get; set; }
    public DateTime CreatedAt { get; set; }
}
