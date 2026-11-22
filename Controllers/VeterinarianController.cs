using PetClinicLab3.Models;
using PetClinicLab3.Repositories;

namespace PetClinicLab3.Controllers;

public class VeterinarianController
{
    private readonly VeterinarianRepository    _vetRepo  = new();
    private readonly SpecializationRepository  _specRepo = new();
    private readonly VetSpecializationRepository _vsRepo = new();

    public List<Veterinarian> GetAll(bool includeInactive = false)
    {
        var vets = _vetRepo.GetAll(includeInactive);
        foreach (var v in vets)
            v.Specializations = _vsRepo.GetByVeterinarian(v.VeterinarianID)
                                       .Select(vs => new Specialization
                                       {
                                           SpecializationID   = vs.SpecializationID,
                                           SpecializationName = vs.SpecializationName
                                       }).ToList();
        return vets;
    }

    public List<Veterinarian> Search(string term)
    {
        var vets = string.IsNullOrWhiteSpace(term) ? _vetRepo.GetAll() : _vetRepo.Search(term.Trim());
        foreach (var v in vets)
            v.Specializations = _vsRepo.GetByVeterinarian(v.VeterinarianID)
                                       .Select(vs => new Specialization
                                       {
                                           SpecializationID   = vs.SpecializationID,
                                           SpecializationName = vs.SpecializationName
                                       }).ToList();
        return vets;
    }

    public Veterinarian? GetById(int id)
    {
        var v = _vetRepo.GetById(id);
        if (v == null) return null;
        v.Specializations = _vsRepo.GetByVeterinarian(id)
                                   .Select(vs => new Specialization
                                   {
                                       SpecializationID   = vs.SpecializationID,
                                       SpecializationName = vs.SpecializationName
                                   }).ToList();
        return v;
    }

    public List<Specialization> GetAllSpecializations() => _specRepo.GetAll();
    public int GetActiveCount() => _vetRepo.GetActiveCount();

    public (bool ok, string error) Save(Veterinarian v, List<int> specializationIds)
    {
        if (string.IsNullOrWhiteSpace(v.FirstName)) return (false, "First name is required.");
        if (string.IsNullOrWhiteSpace(v.LastName))  return (false, "Last name is required.");
        v.FirstName     = v.FirstName.Trim();
        v.LastName      = v.LastName.Trim();
        v.Phone         = string.IsNullOrWhiteSpace(v.Phone)         ? null : v.Phone.Trim();
        v.Email         = string.IsNullOrWhiteSpace(v.Email)         ? null : v.Email.Trim();
        v.LicenseNumber = string.IsNullOrWhiteSpace(v.LicenseNumber) ? null : v.LicenseNumber.Trim();
        v.IsActive      = true;

        if (v.VeterinarianID == 0)
            v.VeterinarianID = _vetRepo.Insert(v);
        else
            _vetRepo.Update(v);

        // Sync specializations: remove all then re-add
        var existing = _vsRepo.GetByVeterinarian(v.VeterinarianID);
        foreach (var ex in existing)
            _vsRepo.Delete(ex.VetSpecializationID);
        foreach (var sid in specializationIds)
            _vsRepo.Insert(v.VeterinarianID, sid);

        return (true, "");
    }

    public (bool ok, string error) Deactivate(int id)
    {
        var v = _vetRepo.GetById(id);
        if (v == null) return (false, "Veterinarian not found.");
        v.IsActive = false;
        _vetRepo.Update(v);
        return (true, "");
    }
}
