namespace PetClinicLab3.Models;

public class Consultation
{
    public int ConsultationID { get; set; }
    public int PetID { get; set; }
    public int VeterinarianID { get; set; }
    public int? RoomID { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public string Status { get; set; } = "Booked";
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public int? CancelledByUserID { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation (from joins)
    public string PetName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string VetName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
}
