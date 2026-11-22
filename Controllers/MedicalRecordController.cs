using PetClinicLab3.Models;
using PetClinicLab3.Repositories;
using PetClinicLab3.Services;

namespace PetClinicLab3.Controllers;

public class MedicalRecordController
{
    private readonly MedicalRecordRepository  _repo    = new();
    private readonly ConsultationRepository   _conRepo = new();

    public List<MedicalRecord> GetAll()           => _repo.GetAll();
    public List<MedicalRecord> GetByPet(int petId) => _repo.GetByPet(petId);
    public MedicalRecord? GetById(int id)          => _repo.GetById(id);
    public MedicalRecord? GetByConsultation(int cid) => _repo.GetByConsultation(cid);
    public int GetCount()                          => _repo.GetCount();
    public List<MedicalRecord> Search(string term) =>
        string.IsNullOrWhiteSpace(term) ? _repo.GetAll() : _repo.Search(term.Trim());

    public (bool ok, string error) Save(MedicalRecord mr)
    {
        if (!AuthService.CanManageMedicalRecords)
            return (false, "Only veterinarians and administrators can manage medical records.");

        var consultation = _conRepo.GetById(mr.ConsultationID);
        if (consultation == null)
            return (false, "Consultation not found.");

        if (consultation.Status != "Arrived" && consultation.Status != "Treated")
            return (false, "Medical records can only be created for 'Arrived' or 'Treated' consultations.");

        // Enforce at-most-one medical record per consultation
        if (mr.RecordID == 0)
        {
            var existing = _repo.GetByConsultation(mr.ConsultationID);
            if (existing != null)
                return (false, "A medical record already exists for this consultation.");
        }

        mr.CreatedByUserID = AuthService.CurrentUser!.UserID;

        if (mr.RecordID == 0)
            mr.RecordID = _repo.Insert(mr);
        else
        {
            var current = _repo.GetById(mr.RecordID);
            if (current == null) return (false, "Medical record not found.");
            mr.CreatedByUserID = current.CreatedByUserID;
            _repo.Update(mr);
        }

        // Auto-transition consultation to Treated if still Arrived
        if (consultation.Status == "Arrived")
            _conRepo.UpdateStatus(consultation.ConsultationID, "Treated");

        return (true, "");
    }
}
