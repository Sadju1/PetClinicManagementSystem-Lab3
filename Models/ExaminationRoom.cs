namespace PetClinicLab3.Models;

public class ExaminationRoom
{
    public int RoomID { get; set; }
    public string RoomName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public string? Description { get; set; }
    public int? AssignedVeterinarianID { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation
    public string AssignedVetName { get; set; } = "";
}
