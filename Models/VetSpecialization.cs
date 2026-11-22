namespace PetClinicLab3.Models;

public class VetSpecialization
{
    public int VetSpecializationID { get; set; }
    public int VeterinarianID { get; set; }
    public int SpecializationID { get; set; }
    public string SpecializationName { get; set; } = "";
}
