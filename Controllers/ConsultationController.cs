using PetClinicLab3.Models;
using PetClinicLab3.Repositories;
using PetClinicLab3.Services;

namespace PetClinicLab3.Controllers;

public class ConsultationController
{
    private readonly ConsultationRepository _repo = new();

    public List<Consultation> GetAll()          => _repo.GetAll();
    public List<Consultation> GetByDate(DateOnly date)  => _repo.GetByDate(date);
    public List<Consultation> GetByPet(int petId)       => _repo.GetByPet(petId);
    public List<Consultation> GetByVet(int vetId)       => _repo.GetByVeterinarian(vetId);
    public Consultation? GetById(int id)                => _repo.GetById(id);
    public int GetTodayCount()                          => _repo.GetTodayCount();
    public int GetCountByStatus(string status)          => _repo.GetCountByStatus(status);
    public int GetTodayCountByStatus(string status)     => _repo.GetTodayCountByStatus(status);
    public int GetTodayAwaitingCount()                  => _repo.GetTodayAwaitingCount();
    public List<Consultation> GetTodayConsultations()   => _repo.GetTodayConsultations();

    public IEnumerable<string> GetValidNextStatuses(string currentStatus) =>
        AllowedTransitions.TryGetValue(currentStatus, out var allowed) ? allowed : Enumerable.Empty<string>();

    private static readonly HashSet<string> ValidStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        { "Booked", "Arrived", "Treated", "Missed", "Cancelled" };

    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        ["Booked"]    = new() { "Arrived", "Missed", "Cancelled" },
        ["Arrived"]   = new() { "Treated", "Missed", "Cancelled" },
        ["Treated"]   = new(),
        ["Missed"]    = new(),
        ["Cancelled"] = new(),
    };

    public (bool ok, string error) Book(Consultation c)
    {
        if (c.PetID <= 0)          return (false, "Pet is required.");
        if (c.VeterinarianID <= 0) return (false, "Veterinarian is required.");

        var now = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute);
        if (c.ScheduledTime < TimeSpan.FromHours(8))
            return (false, "Appointment time must be 08:00 or later.");
        if (c.ScheduledTime > TimeSpan.FromHours(18))
            return (false, "Appointment time must be 18:00 or earlier.");

        if (c.ScheduledDate < DateOnly.FromDateTime(DateTime.Today))
            return (false, "Cannot book appointments in the past.");

        // TOCTOU gap acknowledged: acceptable for single-user desktop app.
        if (_repo.HasOverlap(c.VeterinarianID, c.ScheduledDate, c.ScheduledTime))
            return (false, "This veterinarian already has a booking at that date and time.");

        c.Status = "Booked";
        c.ConsultationID = _repo.Insert(c);
        return (true, "");
    }

    public (bool ok, string error) UpdateStatus(int id, string newStatus)
    {
        if (!ValidStatuses.Contains(newStatus))
            return (false, $"'{newStatus}' is not a valid status.");

        var c = _repo.GetById(id);
        if (c == null) return (false, "Consultation not found.");

        if (!AllowedTransitions.TryGetValue(c.Status, out var allowed) || !allowed.Contains(newStatus))
            return (false, $"Cannot transition from '{c.Status}' to '{newStatus}'.");

        _repo.UpdateStatus(id, newStatus);
        return (true, "");
    }

    public (bool ok, string error) Update(Consultation c)
    {
        var existing = _repo.GetById(c.ConsultationID);
        if (existing == null) return (false, "Consultation not found.");
        if (existing.Status is "Treated" or "Missed" or "Cancelled")
            return (false, $"Cannot edit a consultation with status '{existing.Status}'.");

        if (c.PetID <= 0)          return (false, "Pet is required.");
        if (c.VeterinarianID <= 0) return (false, "Veterinarian is required.");

        if (c.ScheduledTime < TimeSpan.FromHours(8))
            return (false, "Appointment time must be 08:00 or later.");
        if (c.ScheduledTime > TimeSpan.FromHours(18))
            return (false, "Appointment time must be 18:00 or earlier.");

        if (_repo.HasOverlap(c.VeterinarianID, c.ScheduledDate, c.ScheduledTime, c.ConsultationID))
            return (false, "This veterinarian already has a booking at that date and time.");

        c.Status = existing.Status;
        _repo.Update(c);
        return (true, "");
    }
}
