using PetClinicLab3.Models;
using PetClinicLab3.Repositories;
using PetClinicLab3.Services;

namespace PetClinicLab3.Controllers;

public class UserController
{
    private readonly UserRepository _repo = new();

    public List<User> GetAll() => _repo.GetAll();
    public User? GetById(int id) => _repo.GetById(id);

    public (bool ok, string error) CreateAdmin(string username, string fullName, string password)
    {
        if (string.IsNullOrWhiteSpace(username)) return (false, "Username is required.");
        if (string.IsNullOrWhiteSpace(fullName))  return (false, "Full name is required.");
        if (string.IsNullOrWhiteSpace(password))  return (false, "Password is required.");
        if (password.Length < 8) return (false, "Password must be at least 8 characters.");

        if (_repo.GetByUsername(username) != null)
            return (false, "Username already exists.");

        var user = new User
        {
            Username = username.Trim(),
            FullName = fullName.Trim(),
            Password = PasswordHelper.HashPassword(password),
            Role     = "Administrator",
            IsActive = true,
        };
        _repo.Insert(user);
        return (true, "");
    }

    public (bool ok, string error) Save(User u, string? newPassword = null)
    {
        if (!AuthService.IsAdmin) return (false, "Only administrators can manage users.");
        if (string.IsNullOrWhiteSpace(u.Username)) return (false, "Username is required.");
        if (string.IsNullOrWhiteSpace(u.FullName))  return (false, "Full name is required.");
        if (u.Role is not ("Administrator" or "Veterinarian" or "Receptionist"))
            return (false, "Invalid role.");

        u.Username = u.Username.Trim();
        u.FullName  = u.FullName.Trim();

        if (u.UserID == 0)
        {
            if (string.IsNullOrWhiteSpace(newPassword)) return (false, "Password is required for new users.");
            if (newPassword.Length < 8) return (false, "Password must be at least 8 characters.");
            if (_repo.GetByUsername(u.Username) != null) return (false, "Username already exists.");
            u.Password = PasswordHelper.HashPassword(newPassword);
            u.IsActive = true;
            u.UserID   = _repo.Insert(u);
        }
        else
        {
            _repo.Update(u);
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword.Length < 8) return (false, "Password must be at least 8 characters.");
                _repo.UpdatePassword(u.UserID, PasswordHelper.HashPassword(newPassword));
            }
        }
        return (true, "");
    }

    public (bool ok, string error) ChangeOwnPassword(string currentPassword, string newPassword)
    {
        if (AuthService.CurrentUser == null) return (false, "Not logged in.");
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return (false, "New password must be at least 8 characters.");

        var user = _repo.GetById(AuthService.CurrentUser.UserID);
        if (user == null) return (false, "User not found.");
        if (!PasswordHelper.VerifyPassword(currentPassword, user.Password))
            return (false, "Current password is incorrect.");

        _repo.UpdatePassword(user.UserID, PasswordHelper.HashPassword(newPassword));
        return (true, "");
    }

    public (bool ok, User? user) Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, null);
        var user = _repo.GetByUsername(username.Trim());
        if (user == null || !PasswordHelper.VerifyPassword(password, user.Password))
            return (false, null);
        AuthService.Login(user);
        return (true, user);
    }

    public void Logout() => AuthService.Logout();
}
