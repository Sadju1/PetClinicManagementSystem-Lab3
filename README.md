# Pet Clinic Management System

A desktop application for managing the daily operations of a veterinary clinic. Built with C# / .NET 10, Windows Forms, MariaDB, and ADO.NET following a strict Model-View-Controller architecture.

---

## Table of Contents

1. [Overview](#overview)
2. [Features](#features)
3. [User Roles](#user-roles)
4. [Technology Stack](#technology-stack)
5. [MVC Architecture](#mvc-architecture)
6. [Project Structure](#project-structure)
7. [Prerequisites](#prerequisites)
8. [Database Setup](#database-setup)
9. [Installation and Running](#installation-and-running)
10. [First-Run Setup](#first-run-setup)
11. [Security Implementation](#security-implementation)
12. [Known Limitations](#known-limitations)
13. [Academic Context](#academic-context)

---

## Overview

The Pet Clinic Management System is a fully-featured veterinary clinic management application that allows clinic staff to manage owners, pets, veterinarians, examination rooms, appointments, medical records, and user accounts through a professional graphical interface.

The system is designed around realistic clinic workflows: a receptionist books appointments, a veterinarian records examinations and diagnoses, a clinic manager reviews daily performance, and an administrator manages all system settings and accounts.

---

## Features

### Dashboard
- Live statistics: registered owners, active pets, today's appointment count, awaiting patients, and completed consultations
- Today's appointment schedule with status-based color coding (green = completed, yellow = arrived, red = missed)
- Graceful empty state when no data exists

### Owner / Client Management
- Register, edit, and soft-delete client records
- Contact information: name, phone, email, address
- View all pets associated with an owner

### Pet Management
- Register pets linked to an owner
- Species, breed, date of birth, identification tag, and notes
- Soft-delete (deactivate) while preserving medical history

### Veterinarian Management
- Register veterinarians with contact and license details
- Assign multiple specializations per veterinarian
- Soft-delete while preserving appointment history
- Link veterinarian to a user account for login

### Examination Room Management
- Register and manage clinic examination rooms
- Track room availability
- Optionally assign a primary veterinarian to each room

### Appointment and Consultation Management
- Book appointments: select owner → pet → veterinarian → examination room → date and time
- Status tracking with enforced transitions:
  - **Booked** → Arrived, Missed, or Cancelled
  - **Arrived** → Treated, Missed, or Cancelled
  - **Treated / Missed / Cancelled** → terminal (no further changes)
- UI enforces valid transitions; controller independently validates all changes
- Conflict detection to prevent double-booking the same veterinarian at the same time

### Medical Records
- One medical record per completed (Treated) consultation
- Fields: diagnosis, treatment, prescription, follow-up date, notes
- Search by pet name, owner name, or veterinarian
- Veterinarians can only manage records for their own consultations
- Administrators have full access

### User Account Management
- Create and manage user accounts with assigned roles
- Password change functionality
- Soft-delete (deactivate) user accounts

### Reports
- Built-in reports with date range filtering
- Consultation summaries and status breakdowns

---

## User Roles

| Role | Permissions |
|---|---|
| **Administrator** | Full access to all modules: owners, pets, veterinarians, rooms, consultations, medical records, users, reports, dashboard |
| **Receptionist** | Manage owners, pets, and consultations (book, check in, cancel); view dashboard |
| **Veterinarian** | View their own consultations; create and manage medical records for their own consultations; view dashboard |
| **ClinicManager** | Read-only access to consultations; full access to reports; dashboard |

Role permissions are enforced in both the UI (navigation items and buttons are shown or hidden per role) and in the controllers (server-side validation of every operation).

---

## Technology Stack

| Component | Technology |
|---|---|
| Language | C# 13 |
| Framework | .NET 10 (Windows) |
| UI | Windows Forms (WinForms) |
| Database | MariaDB (via XAMPP) |
| Data Access | ADO.NET with MySqlConnector 2.6.0 |
| Password Hashing | PBKDF2-SHA256, 600,000 iterations (built-in `Rfc2898DeriveBytes.Pbkdf2`) |
| Architecture | Model-View-Controller (MVC) |

---

## MVC Architecture

The application follows a strict three-layer MVC separation:

```
View (WinForms)
  │  calls
  ▼
Controller (validation + business rules)
  │  calls
  ▼
Repository (SQL queries via ADO.NET)
  │  reads/writes
  ▼
MariaDB Database
```

**Contract enforced throughout the codebase:**
- **Views** contain no SQL and no business logic. They call controllers and display results.
- **Repositories** contain no UI references. They execute parameterized SQL and return model objects.
- **Controllers** own all validation, authorization checks, and business rule enforcement.
- **Models** are plain C# classes with no behavior — data only.

---

## Project Structure

```
PetClinicLab3/
│
├── Configuration/
│   └── AppConfig.cs                  Connection string
│
├── Models/
│   ├── Consultation.cs
│   ├── ExaminationRoom.cs
│   ├── MedicalRecord.cs
│   ├── Owner.cs
│   ├── Pet.cs
│   ├── Specialization.cs
│   ├── User.cs
│   ├── VetSpecialization.cs
│   └── Veterinarian.cs
│
├── Repositories/
│   ├── ConsultationRepository.cs
│   ├── ExaminationRoomRepository.cs
│   ├── MedicalRecordRepository.cs
│   ├── OwnerRepository.cs
│   ├── PetRepository.cs
│   ├── SpecializationRepository.cs
│   ├── UserRepository.cs
│   ├── VetSpecializationRepository.cs
│   └── VeterinarianRepository.cs
│
├── Controllers/
│   ├── ConsultationController.cs
│   ├── MedicalRecordController.cs
│   ├── OwnerController.cs
│   ├── PetController.cs
│   ├── RoomController.cs
│   ├── UserController.cs
│   └── VeterinarianController.cs
│
├── Services/
│   ├── AuthService.cs                Session management, role checks
│   ├── DatabaseInitializer.cs        Auto-creates tables and seeds data
│   └── PasswordHelper.cs             PBKDF2-SHA256 hashing and verification
│
├── Views/
│   ├── UITheme.cs                    Shared color palette, fonts, control factories
│   ├── SetupForm.cs                  First-run administrator account creation
│   ├── LoginForm.cs                  Authentication form
│   ├── MainForm.cs                   Shell window with role-filtered navigation
│   ├── DashboardForm.cs              Live statistics and today's schedule
│   ├── OwnersForm.cs                 Owner list with search
│   ├── OwnerDetailForm.cs            Create/edit owner
│   ├── PetsForm.cs                   Pet list with search
│   ├── PetDetailForm.cs              Create/edit pet
│   ├── VeterinariansForm.cs          Veterinarian list
│   ├── VeterinarianDetailForm.cs     Create/edit veterinarian + specializations
│   ├── ExaminationRoomsForm.cs       Room list and room detail dialog
│   ├── ConsultationsForm.cs          Appointment list with filters
│   ├── ConsultationDetailForm.cs     Book/edit appointment; status transitions
│   ├── MedicalRecordsForm.cs         Medical record list with search
│   ├── MedicalRecordForm.cs          Create/edit medical record
│   ├── ReportsForm.cs                Reports with date range filtering
│   └── UsersForm.cs                  User account management, password change
│
├── Program.cs                        Entry point; DB check → init → setup → login
├── PetClinicLab3.csproj
├── .gitignore
└── README.md
```

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 10.0 or later |
| Windows | 10 or 11 |
| XAMPP | Any recent version with MariaDB |

Download .NET 10 SDK: https://dotnet.microsoft.com/download  
Download XAMPP: https://www.apachefriends.org/

---

## Database Setup

The application connects to MariaDB on `127.0.0.1:3306` with user `root` and an empty password — the XAMPP default.

**Step 1 — Start XAMPP**

Open the XAMPP Control Panel and start the **MySQL** service. Confirm it shows "Running" on port 3306.

**Step 2 — Create the database**

Open phpMyAdmin (`http://localhost/phpmyadmin`) or the MySQL command line and run:

```sql
CREATE DATABASE PetClinicDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

**Step 3 — The application handles everything else**

On first launch, the application automatically:
- Creates all 9 database tables (using `CREATE TABLE IF NOT EXISTS`)
- Seeds 8 veterinary specializations (General Practice, Surgery, Dermatology, Cardiology, Orthopedics, Ophthalmology, Dentistry, Emergency & Critical Care)
- Seeds 3 default examination rooms (Exam Room 1, Exam Room 2, Surgery Suite)

No manual SQL scripts need to be executed beyond creating the empty database.

**Connection string** (in `Configuration/AppConfig.cs`):
```
Server=127.0.0.1;Port=3306;Database=PetClinicDB;User ID=root;Password=;
```

If your MariaDB uses a different port or password, update `AppConfig.cs` before running.

---

## Installation and Running

```bash
# 1. Clone the repository
git clone <your-repository-url>
cd PetClinicLab3

# 2. Restore dependencies
dotnet restore

# 3. Build
dotnet build

# 4. Run (requires XAMPP MariaDB running and PetClinicDB created)
dotnet run
```

Alternatively, open `PetClinicLab3.csproj` in Visual Studio 2022 (or later) and press **F5**.

---

## First-Run Setup

1. Start XAMPP and ensure MariaDB is running.
2. Create the `PetClinicDB` database (see Database Setup above).
3. Run the application with `dotnet run`.
4. The application tests the database connection on startup. If it cannot connect, an error message explains what to check.
5. On the very first run (no administrator account exists), the **Initial Setup** form appears automatically. Enter a username, full name, and password to create the Administrator account.
6. The application then opens the **Login** form. Log in with the credentials you just created.
7. The main application window opens maximized with role-appropriate navigation.

---

## Security Implementation

All security measures are confirmed in the source code:

**Password hashing**
- Algorithm: PBKDF2-SHA256 (built-in `Rfc2898DeriveBytes.Pbkdf2`, no additional packages)
- Iterations: 600,000 (OWASP 2023 recommendation)
- Salt: 16 bytes, cryptographically random per password
- Storage format: `600000:saltBase64:hashBase64` in a `VARCHAR(255)` column
- Comparison: `CryptographicOperations.FixedTimeEquals` (constant-time, resistant to timing attacks)
- Verification is hardened against malformed stored values: null/empty guard, part count check, iteration range check, Base64 format guard, zero-length guard

**SQL injection prevention**
- All database queries use parameterized ADO.NET parameters (`cmd.Parameters.Add(...)`)
- No string concatenation of user input into SQL anywhere in the codebase

**Role-based access control**
- `AuthService` holds the authenticated session in memory (static `CurrentUser`)
- Every controller method that requires a specific role checks `AuthService.CurrentUser?.Role`
- The UI hides navigation items and buttons per role; the controller enforces the same rules independently

---

## Known Limitations

- **TOCTOU gap in appointment overlap check**: The overlap check (SELECT) and the appointment INSERT are not in a single database transaction. For a multi-user web application this would be a concurrency problem. For this single-user desktop application it is acceptable and is documented in the schema design.
- **MySqlConnector advisory**: MySqlConnector 2.6.0 carries a moderate security advisory (GHSA-473q-m89c-ghf8). This is acceptable for an academic project. Update the version in `PetClinicLab3.csproj` when a patched release is available.
- **No printing or PDF export**: Reports are displayed on-screen only.
- **No email or SMS notifications**: Appointment reminders are not implemented.
- **Single clinic**: The system is designed for one clinic location; multi-site support is not included.

---

## Academic Context

This project was developed as **Lab 3** for a university course assignment requiring:

- A Windows Forms desktop application
- A relational database implemented and connected
- User interaction with the database through a GUI
- MVC (Model-View-Controller) architecture
- Code hosted on Git/GitHub

The project exceeds the minimum requirements by implementing role-based access control, secure password hashing, automated database initialization, consultation status state machine enforcement, and a live statistics dashboard.
