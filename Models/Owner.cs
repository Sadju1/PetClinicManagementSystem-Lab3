namespace PetClinicLab3.Models;

public class Owner
{
    public int OwnerID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
