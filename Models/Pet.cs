namespace PetClinicLab3.Models;

public class Pet
{
    public int PetID { get; set; }
    public int OwnerID { get; set; }
    public string PetName { get; set; } = "";
    public string Species { get; set; } = "";
    public string? Breed { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? IdentificationTag { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation (populated by joins)
    public string OwnerName { get; set; } = "";
}
