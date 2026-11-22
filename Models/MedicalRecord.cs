namespace PetClinicLab3.Models;

public class MedicalRecord
{
    public int RecordID { get; set; }
    public int ConsultationID { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public string? Notes { get; set; }
    public int CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public string PetName       { get; set; } = "";
    public string OwnerName     { get; set; } = "";
    public string VetName       { get; set; } = "";
    public string CreatedByName { get; set; } = "";
    public DateOnly ScheduledDate { get; set; }
}
