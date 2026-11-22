using PetClinicLab3.Models;
using PetClinicLab3.Repositories;

namespace PetClinicLab3.Controllers;

public class OwnerController
{
    private readonly OwnerRepository _repo = new();

    public List<Owner> GetAll(bool includeInactive = false) => _repo.GetAll(includeInactive);
    public List<Owner> Search(string term) => string.IsNullOrWhiteSpace(term)
        ? _repo.GetAll()
        : _repo.Search(term.Trim());
    public Owner? GetById(int id) => _repo.GetById(id);
    public int GetCount() => _repo.GetCount();

    public (bool ok, string error) Save(Owner o)
    {
        if (string.IsNullOrWhiteSpace(o.FirstName)) return (false, "First name is required.");
        if (string.IsNullOrWhiteSpace(o.LastName))  return (false, "Last name is required.");
        o.FirstName = o.FirstName.Trim();
        o.LastName  = o.LastName.Trim();
        o.Phone     = string.IsNullOrWhiteSpace(o.Phone)   ? null : o.Phone.Trim();
        o.Email     = string.IsNullOrWhiteSpace(o.Email)   ? null : o.Email.Trim();
        o.Address   = string.IsNullOrWhiteSpace(o.Address) ? null : o.Address.Trim();
        o.IsActive  = true;
        if (o.OwnerID == 0)
            o.OwnerID = _repo.Insert(o);
        else
            _repo.Update(o);
        return (true, "");
    }

    public (bool ok, string error) Deactivate(int id)
    {
        var o = _repo.GetById(id);
        if (o == null) return (false, "Owner not found.");
        o.IsActive = false;
        _repo.Update(o);
        return (true, "");
    }
}
