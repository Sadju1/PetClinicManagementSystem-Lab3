using PetClinicLab3.Models;
using PetClinicLab3.Repositories;

namespace PetClinicLab3.Controllers;

public class RoomController
{
    private readonly ExaminationRoomRepository _repo = new();

    public List<ExaminationRoom> GetAll() => _repo.GetAll();
    public ExaminationRoom? GetById(int id) => _repo.GetById(id);
    public int GetCount() => _repo.GetCount();

    public (bool ok, string error) Save(ExaminationRoom room)
    {
        if (string.IsNullOrWhiteSpace(room.RoomName)) return (false, "Room name is required.");
        room.RoomName    = room.RoomName.Trim();
        room.RoomNumber  = string.IsNullOrWhiteSpace(room.RoomNumber)  ? null : room.RoomNumber.Trim();
        room.Description = string.IsNullOrWhiteSpace(room.Description) ? null : room.Description.Trim();
        if (room.RoomID == 0)
            room.RoomID = _repo.Insert(room);
        else
            _repo.Update(room);
        return (true, "");
    }

    public void SetAvailability(int id, bool available)
    {
        var room = _repo.GetById(id);
        if (room == null) return;
        room.IsAvailable = available;
        _repo.Update(room);
    }
}
