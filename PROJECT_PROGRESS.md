# PROJECT_PROGRESS.md

## Status: BUILD SUCCEEDED

`dotnet build` — 0 errors, 15 warnings (all nullable/NuGet advisories, no functional issues)

---

## Completed

- [x] .csproj — net10.0-windows, UseWindowsForms, MySqlConnector 2.5.1
- [x] Configuration/AppConfig.cs
- [x] Models (9): User, Owner, Pet, Veterinarian, Specialization, VetSpecialization, ExaminationRoom, Consultation, MedicalRecord
- [x] Services/PasswordHelper.cs — PBKDF2-SHA256, 600,000 iterations
- [x] Services/AuthService.cs — static session, role helpers
- [x] Services/DatabaseInitializer.cs — CREATE TABLE IF NOT EXISTS (all 9), seed specializations + rooms
- [x] Repositories (9): User, Owner, Pet, Veterinarian, Specialization, VetSpecialization, ExaminationRoom, Consultation, MedicalRecord
- [x] Controllers (7): Owner, Pet, Veterinarian, Room, Consultation, MedicalRecord, User
- [x] Program.cs — DB check → Initialize → SetupForm → LoginForm → MainForm
- [x] Views/UITheme.cs — color palette, factory helpers
- [x] Views (15 forms): SetupForm, LoginForm, MainForm, DashboardForm, OwnersForm, OwnerDetailForm, PetsForm, PetDetailForm, ConsultationsForm, ConsultationDetailForm, MedicalRecordForm, MedicalRecordsForm, VeterinariansForm, VeterinarianDetailForm, ExaminationRoomsForm (+ ExaminationRoomDetailForm inline), ReportsForm, UsersForm (+ UserDetailForm, ChangePasswordForm inline)
- [x] .gitignore
- [x] README.md

---

## Build output

```
Build succeeded.
15 Warning(s)
0 Error(s)
```

### Warnings (non-blocking)

| Code | Location | Note |
|---|---|---|
| NU1603 | .csproj | MySqlConnector 2.4.1 not found; resolved to 2.5.0 |
| NU1902 | .csproj | MySqlConnector 2.5.0 moderate advisory (GHSA-473q-m89c-ghf8) |
| CS8601 | Repositories, Controllers | Nullable assignment in `?` navigation properties from LEFT JOIN |
| CS8605 | Views | DataGridView `SelectedValue` unbox — always set by DropDownList binding |

---

## Next steps (before submission)

1. Start XAMPP, verify MariaDB is on port 3306
2. Create `PetClinicDB` database in phpMyAdmin
3. Run `dotnet run` — app will create tables and seed data automatically
4. Create admin account via SetupForm
5. Test all 15 flows end-to-end
6. `git init && git add . && git commit -m "Initial commit: Lab 3 Pet Clinic"`
7. Push to GitHub (user's responsibility)

---

## Architecture decisions

- MySqlConnector (not MySQL.Data) — BSD license, async-first, native ADO.NET
- DateOnly for DATE columns, TimeSpan for TIME (MySqlConnector mapping)
- Soft-delete via IsActive on Owners, Pets, Veterinarians, Users
- Status machine: Booked → Arrived → Treated/Missed/Cancelled; terminal states locked
- At-most-one medical record per consultation enforced at controller level
- TOCTOU overlap check: documented as acceptable for single-user desktop
