using PetClinicLab3.Models;

namespace PetClinicLab3.Services;

public static class AuthService
{
    public static User? CurrentUser { get; private set; }
    public static bool IsAuthenticated => CurrentUser != null;
    public static bool IsAdmin         => CurrentUser?.Role == "Administrator";
    public static bool IsVeterinarian  => CurrentUser?.Role == "Veterinarian";
    public static bool HasRole(string role) => CurrentUser?.Role == role;
    public static bool CanManageMedicalRecords =>
        CurrentUser?.Role is "Veterinarian" or "Administrator";

    public static void Login(User user)  => CurrentUser = user;
    public static void Logout()          => CurrentUser = null;
}
