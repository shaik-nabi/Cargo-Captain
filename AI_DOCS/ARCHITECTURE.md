# CargoCaptain - System Architecture

> **Architecture, Design Patterns, Dependency Injection, and Security Pipeline**

---

## 1. Architectural Overview

CargoCaptain follows an **N-Tier Layered Architecture** with strict Separation of Concerns (SoC).

```
┌─────────────────────────────────────────────────────────────┐
│                 Presentation Layer (Razor)                  │
│       - Razor Views (.cshtml) & ViewModels                  │
│       - Bootstrap 5 Layouts & UI Components                 │
└──────────────────────────────┬──────────────────────────────┘
                               │ HTTP Requests / Form Submissions
┌──────────────────────────────▼──────────────────────────────┐
│                    Controller Layer (MVC)                   │
│       - 11 ASP.NET Core MVC Controllers                      │
│       - Auth Claim Resolution & Request Validation          │
└──────────────────────────────┬──────────────────────────────┘
                               │ Invokes Async Services
┌──────────────────────────────▼──────────────────────────────┐
│                     Service Layer (BLL)                     │
│       - 8 Business Logic Services & Interfaces              │
│       - Duty / Demurrage Logic & State Machine Rules        │
└──────────────────────────────┬──────────────────────────────┘
                               │ Interacts via EF Core
┌──────────────────────────────▼──────────────────────────────┐
│                   Data Access Layer (DAL)                   │
│       - ApplicationDbContext (DbSets & Fluent API)          │
│       - Entity Domain Models (Models/)                      │
└──────────────────────────────┬──────────────────────────────┘
                               │ SQL Queries & Commands
┌──────────────────────────────▼──────────────────────────────┐
│                      Database Layer                         │
│       - SQL Server Database                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Component Layer Responsibilities

### Presentation Layer (`Views/`, `ViewModels/`)
* **Razor Views (`.cshtml`)**: Strongly typed to ViewModels. Render UI using Bootstrap 5 cards, forms, tables, timeline controls, and modal dialogs. No direct SQL or DbContext access.
* **ViewModels (`ViewModels/`)**: 19 Data Transfer Objects tailored to specific UI interactions. Isolate domain models from presentation requirements.

### Controller Layer (`Controllers/`)
* **Request Handling**: Accepts HTTP requests (`[HttpGet]`, `[HttpPost]`), validates anti-forgery tokens (`[ValidateAntiForgeryToken]`), inspects `ModelState`, and maps user identity claims.
* **Security & Isolation**: Enforces role checks via `[Authorize(Roles = "...")]`. Ensures data isolation (e.g. users can only access their own bookings by verifying `booking.userId == currentUserId`, returning `NotFound()` if mismatched).

### Service Layer (`Services/`, `Interfaces/`)
* **Business Logic Encapsulation**: Interface-backed services injected into controllers via Dependency Injection.
* **Transaction Management**: Performs complex operations like generating unique booking numbers, computing customs duty based on declared value, calculating demurrage fees, and updating shipment states.

### Data Access Layer (`Data/`, `Models/`)
* **`ApplicationDbContext`**: EF Core `DbContext` configuring tables, unique indexes, relationships, decimal precision specs `(18,2)`, and model seed data.

---

## 3. Dependency Injection (DI) Registration

All service abstractions are registered as **Scoped** dependencies in `Program.cs`:

```csharp
// Business Service Registrations
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IShipmentBookingService, ShipmentBookingService>();
builder.Services.AddScoped<IContainerService, ContainerService>();
builder.Services.AddScoped<ICustomsService, CustomsService>();
builder.Services.AddScoped<IPortOperatorService, PortOperatorService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<IFreightInvoiceService, FreightInvoiceService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
```

---

## 4. Authentication & Security Pipeline

1. **Authentication Scheme**: Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`).
2. **Cookie Options**:
   * `LoginPath = "/Account/Login"`
   * `AccessDeniedPath = "/Account/AccessDenied"`
   * `ExpireTimeSpan = TimeSpan.FromMinutes(60)`
   * `SlidingExpiration = true`
   * `Cookie.HttpOnly = true`
   * `Cookie.SecurePolicy = CookieSecurePolicy.Always`
3. **Claims Identity**: Includes `ClaimTypes.NameIdentifier` (User ID), `ClaimTypes.Name` (Associated Name), and `ClaimTypes.Role` (User Role).
4. **Password Hashing**: Uses ASP.NET Core `PasswordHasher<Login>` for secure PBKDF2 password hashing.

---

## 5. Design Patterns Applied

* **Repository / Service Pattern**: Business logic decoupled from data access using `Interface`-backed service layers (`IShipmentBookingService`, etc.).
* **Model-View-ViewModel (MVVM / DTO Pattern)**: Strong separation between database entities (`Models/`) and presentation objects (`ViewModels/`).
* **Factory / Helper Pattern**: Helper classes (`NumberGeneratorHelper`, `CurrencyHelper`, `FileHelper`) generate formatted numbers and handle file streams safely.
