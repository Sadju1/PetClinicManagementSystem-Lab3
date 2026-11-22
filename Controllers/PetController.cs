using PetClinicLab3.Models;
using PetClinicLab3.Repositories;

namespace PetClinicLab3.Controllers;

public class PetController
{
    private readonly PetRepository _repo = new();

    public List<Pet> GetAll(bool includeInactive = false) => _repo.GetAll(includeInactive);
    public List<Pet> GetByOwner(int ownerId) => _repo.GetByOwner(ownerId);
    public List<Pet> Search(string term) => string.IsNullOrWhiteSpace(term)
        ? _repo.GetAll()
        : _repo.Search(term.Trim());
    public Pet? GetById(int id) => _repo.GetById(id);
    public int GetActiveCount() => _repo.GetActiveCount();

    public (bool ok, string error) Save(Pet p)
    {
        if (p.OwnerID <= 0)                         return (false, "Owner is required.");
        if (string.IsNullOrWhiteSpace(p.PetName))   return (false, "Pet name is required.");
        if (string.IsNullOrWhiteSpace(p.Species))   return (false, "Species is required.");
        p.PetName           = p.PetName.Trim();
        p.Species           = p.Species.Trim();
        p.Breed             = string.IsNullOrWhiteSpace(p.Breed)             ? null : p.Breed.Trim();
        p.IdentificationTag = string.IsNullOrWhiteSpace(p.IdentificationTag) ? null : p.IdentificationTag.Trim();
        p.Notes             = string.IsNullOrWhiteSpace(p.Notes)             ? null : p.Notes.Trim();
        p.IsActive          = true;
        if (p.PetID == 0)
            p.PetID = _repo.Insert(p);
        else
            _repo.Update(p);
        return (true, "");
    }

    public (bool ok, string error) Deactivate(int id)
    {
        var p = _repo.GetById(id);
        if (p == null) return (false, "Pet not found.");
        p.IsActive = false;
        _repo.Update(p);
        return (true, "");
    }
}
