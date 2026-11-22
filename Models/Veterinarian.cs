namespace PetClinicLab3.Models;

public class Veterinarian
{
    public int VeterinarianID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public string FullName => $"Dr. {FirstName} {LastName}";

    // Navigation
    public List<Specialization> Specializations { get; set; } = new();
}
