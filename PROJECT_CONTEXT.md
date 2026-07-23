# CargoCaptain - Comprehensive Project Context

> **Single Source of Truth** for developers and AI assistants working on the CargoCaptain codebase.

---

## 1. Project Overview

* **Project Name**: CargoCaptain (`CargoCaptain.csproj`, folder: `projectcargo`)
* **Purpose**: An enterprise-grade, web-based Maritime Cargo and Freight Logistics Management System.
* **Business Domain**: Maritime shipping, container freight management, customs clearance, terminal logistics tracking, and freight billing.
* **Main Objectives**:
  * Streamline the shipment booking lifecycle from shipper request to final delivery.
  * Provide role-tailored operational dashboards for Shippers, Freight Forwarders, Customs Brokers, Port Terminal Operators, Consignees, Financial Officers, and System Administrators.
  * Automate container allocation, tracking milestone updates, customs duty declarations, demurrage billing, and reporting.

---

## 2. Technology Stack

* **Target Framework**: .NET 8.0 (`net8.0`, C# 12)
* **Web Framework**: ASP.NET Core MVC (Model-View-Controller)
* **ORM & Database**: Entity Framework Core 8.0 (`Microsoft.EntityFrameworkCore.SqlServer`) targeting SQL Server.
* **Authentication & Security**: ASP.NET Core Cookie-based Authentication (`Microsoft.AspNetCore.Authentication.Cookies`) with ASP.NET Core `PasswordHasher<T>` for credential security.
* **Frontend UI**: Razor Views (`.cshtml`), HTML5, CSS3, Bootstrap 5 responsive layout framework, and Bootstrap Icons.
* **Third-Party Libraries**:
  * **ClosedXML (v0.105.0)**: Used for dynamic Excel report generation (`.xlsx`).
  * **PDFsharp (v6.2.4)**: Used for dynamic PDF document export (`.pdf`).
  * **Microsoft.EntityFrameworkCore.Design / Tools (v8.0)**: EF Core migration tooling.

---

## 3. Solution Structure

```
c:\Users\SHAIK NABI RASOOL\Desktop\example\projectcargo\
├── CargoCaptain.csproj            # .NET 8 Web Project file with package references
├── Program.cs                     # Application entry point, DI setup, HTTP pipeline & route maps
├── appsettings.json               # Main configuration file (Connection strings, Logging)
├── appsettings.Development.json   # Development environment configuration
├── task.md                        # Active development task checklist
├── PROJECT_CONTEXT.md             # Single Source of Truth documentation (This file)
│
├── Configurations/                # Custom configuration models (if needed)
├── Constants/                     # Shared application constants
├── Controllers/                   # 11 MVC Controllers managing HTTP requests
├── Data/                          # ApplicationDbContext & Entity Configuration / Seed Data
├── Enums/                         # System Enums (Booking, Container, Customs, Invoice, Roles)
├── Extensions/                    # C# Extension methods
├── Helpers/                       # Utility helper classes (File, Currency, Date, Number generators)
├── Interfaces/                    # Service layer interface definitions (8 interfaces)
├── Middleware/                    # Custom ASP.NET Core middleware components
├── Migrations/                    # EF Core Database Migration snapshots
├── Models/                        # Entity domain models (8 models)
├── Properties/                    # Assembly properties and launchSettings.json
├── Repositories/                  # Data access repository implementations
├── RepositoryInterfaces/          # Data access repository interfaces
├── Services/                      # Business logic service implementations (8 services)
├── Utilities/                     # Additional helper utilities
├── ViewModels/                    # Strongly-typed ViewModels for Razor Views (19 ViewModels)
├── Views/                         # Razor View templates organized by Controller subfolders
│   ├── Account/                   # Login, Register, AccessDenied
│   ├── Admin/                     # Admin Dashboard, Employee CRUD
│   ├── Consignee/                 # Consignee Dashboard & Incoming Cargo list
│   ├── Container/                 # Forwarder Dashboard, Container Allocation
│   ├── Customs/                   # Customs Broker Dashboard, File Declaration, Review
│   ├── FreightInvoice/            # Finance Dashboard, Generate Invoice, Process Payment
│   ├── Home/                      # Landing page, Error page
│   ├── PortOperator/              # Port Operator Dashboard, Milestone Logging
│   ├── Reports/                   # Reports Console & View Report template
│   ├── ShipmentBooking/           # Shipper Dashboard, Create/Edit Booking, Details
│   ├── Tracking/                  # Public Shipment Search & Timeline Details
│   └── Shared/                    # Shared Layouts (_Layout.cshtml), Navigation, Validation
└── wwwroot/                       # Static web assets (CSS, JS, images, uploaded documents)
    └── uploads/
        └── bookings/              # Uploaded booking compliance documents
```

---

## 4. System Architecture

### Architectural Pattern
CargoCaptain follows a clean **N-Tier Layered Architecture** with strict **Separation of Concerns**:

```
[ Presentation Layer: Razor Views & ViewModels ]
                      │
[ Controller Layer: ASP.NET Core MVC Controllers ]
                      │
[ Service Layer: Interface-backed Business Logic Services ]
                      │
[ Data Access Layer: EF Core DbContext & Domain Models ]
                      │
[ Database Layer: SQL Server ]
```

1. **Presentation Layer**: Razor Views consume strongly-typed ViewModels. No direct database queries or raw domain entities are exposed directly to views.
2. **Controller Layer**: Handles HTTP requests, user context resolution (Claims), form validation, mapping between ViewModels and Domain Models, and delegates business logic to services.
3. **Service Layer**: Implements core business logic, validation rules, data transformations, and state machine transitions using `async`/`await`.
4. **Data Access Layer**: `ApplicationDbContext` manages SQL Server persistence, entity relationships, decimal precisions, and indexes via EF Core.
5. **Authentication Flow**: Cookie-based authentication stores identity claims (`NameIdentifier`, `Role`, `AssociatedName`). Controllers are decorated with `[Authorize(Roles = "...")]` attributes.

---

## 5. Database Design & Entity Model

The relational database schema is managed via `ApplicationDbContext` with explicit decimal precision (`18,2`), unique indexes, foreign keys, and cascading rules.

### Entity Relationship Diagram (Mental Model)
* `Login` (1) ↔ (1) `Employee` *(Cascade Delete)*
* `Login` (1) ── (N) `ShipmentBooking` *(Restrict Delete)*
* `ShipmentBooking` (1) ── (N) `Container` *(Cascade Delete)*
* `Container` (1) ── (N) `CargoEvent` *(Cascade Delete)*
* `ShipmentBooking` (1) ↔ (1) `CustomsDeclaration` *(Cascade Delete)*
* `ShipmentBooking` (1) ↔ (1) `FreightInvoice` *(Cascade Delete)*
* `Login` (1) ── (N) `FreightInvoice` (PaidByUser) *(Restrict Delete)*

### Table / Entity Summary

| Entity Name | Primary Key | Key Foreign Keys | Unique Constraints | Key Attributes |
| :--- | :--- | :--- | :--- | :--- |
| **Login** | `UserId` (int) | None | Username / Role combo | `Password` (hashed), `Role`, `AssociatedName` |
| **Employee** | `employeeId` (int) | `userId` ➔ `Login` | `email` | `firstName`, `lastName`, `email`, `phoneNumber`, `department`, `designation` |
| **ShipmentBooking** | `bookingId` (int) | `userId` ➔ `Login` | `bookingNumber` | `bookingNumber`, `consigneeName`, `originPort`, `destinationPort`, `cargoWeight` (18,2), `cargoDescription`, `bookingStatus`, `bookingDate` |
| **Container** | `containerId` (int) | `bookingId` ➔ `ShipmentBooking` | `containerNumber` | `containerNumber`, `containerType`, `containerStatus`, `currentLocation` |
| **CustomsDeclaration**| `declarationId` (int)| `bookingId` ➔ `ShipmentBooking` | `declarationNumber` | `declarationNumber`, `declarationType`, `declaredValue` (18,2), `hsCode`, `calculatedDuty` (18,2), `clearanceStatus` |
| **CargoEvent** | `eventId` (int) | `containerId` ➔ `Container` | None | `eventType`, `eventTimestamp`, `location`, `description`, `recordedBy` |
| **FreightInvoice** | `invoiceId` (int) | `bookingId` ➔ `ShipmentBooking`, `paidByUserId` ➔ `Login` | `invoiceNumber` | `invoiceNumber`, `freightCharges` (18,2), `surchargeAmount` (18,2), `demurrageAmount` (18,2), `totalAmount` (18,2), `invoiceStatus`, `dueDate`, `paymentDate` |

### System Enums

1. **UserRole**: `Admin`, `Shipper`, `FreightForwarder`, `CustomsBroker`, `PortOperator`, `Consignee`
2. **BookingStatus**: `DRAFT`, `PENDING`, `CONFIRMED`, `CANCELLED`, `COMPLETED`
3. **ContainerType**: `Standard20ft`, `Standard40ft`, `Reefer`, `FlatRack`, `OpenTop`
4. **ContainerStatus**: `EMPTY`, `LOADED`, `IN_TRANSIT`, `DISCHARGED`
5. **DeclarationType**: `IMPORT`, `EXPORT`, `TRANSIT`
6. **ClearanceStatus**: `FILED`, `UNDER_REVIEW`, `CLEARED`, `HELD`, `REJECTED`
7. **CargoEventType**: `GATE_IN`, `LOADED`, `SAILED`, `DISCHARGED`, `GATE_OUT`
8. **InvoiceStatus**: `DRAFT`, `ISSUED`, `PAID`, `OVERDUE`

---

## 6. User Roles & Permissions Matrix

| Role | Accessible Modules / Dashboards | Core Permissions & Key Capabilities | Restricted Actions |
| :--- | :--- | :--- | :--- |
| **Admin** | Admin Dashboard (`/Admin/Dashboard`), Reports Console (`/Reports`), System Management | Full system visibility. Create, Edit, and Delete Employee accounts. Access revenue & financial analytics reports. | Cannot modify operational container tracking events directly. |
| **Shipper** | Shipper Dashboard (`/Shipper/Dashboard`), Booking Creation/Edit | Register new shipment bookings, update pending bookings, cancel bookings, upload compliance documents (PDF/JPG/PNG/DOCX up to 5MB). | Cannot assign containers, approve customs, or issue invoices. |
| **Freight Forwarder** | Forwarder Dashboard (`/FreightForwarder/Dashboard`), Container Allocation, Reports | View pending bookings, allocate containers to shipments, change booking status to CONFIRMED, view non-financial reports. | Cannot view revenue analytics reports (denied with `Forbid()`). |
| **Customs Broker** | Customs Dashboard (`/CustomsBroker/Dashboard`), Customs Review | File customs declarations, calculate duties, update clearance status (`UNDER_REVIEW`, `CLEARED`, `HELD`, `REJECTED`). | Cannot record terminal milestones or process invoice payments. |
| **Port Operator** | Port Dashboard (`/PortOperator/Dashboard`), Terminal Milestone Logging | Record container movement milestones (`GATE_IN`, `LOADED`, `SAILED`, `DISCHARGED`, `GATE_OUT`), update current container location. | Cannot alter booking contract details or customs valuations. |
| **Consignee** | Consignee Dashboard (`/Consignee/Dashboard`), Cargo Search | View incoming cargo assigned to them, track live shipment status and milestone history. | Cannot create bookings or edit shipping details. |

---

## 7. End-to-End Business Workflow

1. **User Authentication**:
   - User logs in via `AccountController.Login`.
   - On success, claims are assigned and user is routed to their role-specific dashboard.
2. **Booking Registration (Shipper)**:
   - Shipper submits a new booking (`Origin`, `Destination`, `CargoWeight`, `Consignee`).
   - System validates ports (Origin ≠ Destination) and date (cannot be in past).
   - Booking is stored with `BookingStatus.PENDING` and a generated `bookingNumber` (e.g., `BKG-XXXXXXXX`).
3. **Container Assignment (Freight Forwarder)**:
   - Freight Forwarder selects the pending booking and assigns containers (`ContainerType`, `ContainerNumber`).
   - Booking transitions to `BookingStatus.CONFIRMED`.
4. **Customs Filing & Duty Calculation (Customs Broker)**:
   - Broker files a declaration (`DeclarationType`, `DeclaredValue`, `HSCode`).
   - Service automatically calculates estimated duty (e.g., 5% base rate).
   - Status moves from `FILED` ➔ `UNDER_REVIEW` ➔ `CLEARED`.
5. **Terminal Operations & Milestone Logging (Port Operator)**:
   - Port Operator logs container milestone events (`GATE_IN`, `LOADED`, `SAILED`, `DISCHARGED`, `GATE_OUT`) with location & timestamps.
   - Container status updates synchronously (`EMPTY` ➔ `LOADED` ➔ `IN_TRANSIT` ➔ `DISCHARGED`).
6. **Invoicing & Settlement (Finance / Forwarder)**:
   - Forwarder/Finance generates a `FreightInvoice`.
   - Freight charges, surcharges, and demurrage (calculated if container discharge exceeds free days) are totaled.
   - Invoice is `ISSUED`. Once paid, status changes to `PAID`.
7. **Tracking & Completion**:
   - Consignees and Shippers track cargo in real time via `TrackingController`.
   - Once all events are completed and invoice is paid, booking reaches `BookingStatus.COMPLETED`.

---

## 8. Module Documentation

### A. Authentication & Account Module
* **Purpose**: User login, user registration (Shipper/Consignee roles), session management, logout, and access control enforcement.
* **Controller**: `AccountController`
* **Views**: `Views/Account/Login.cshtml`, `Register.cshtml`, `AccessDenied.cshtml`

### B. Shipper & Booking Module
* **Purpose**: Managing shipment requests, editing pending orders, uploading compliance documents.
* **Controller**: `ShipmentBookingController`
* **Service**: `ShipmentBookingService`
* **Views**: `Views/ShipmentBooking/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`

### C. Freight Forwarder & Container Module
* **Purpose**: Allocating containers to confirmed bookings, managing fleet inventory.
* **Controller**: `ContainerController`
* **Service**: `ContainerService`
* **Views**: `Views/Container/Index.cshtml`, `Allocate.cshtml`, `Details.cshtml`

### D. Customs Clearance Module
* **Purpose**: Customs declaration filing, regulatory inspections, duty calculation, approval/rejection workflows.
* **Controller**: `CustomsController`
* **Service**: `CustomsService`
* **Views**: `Views/Customs/Index.cshtml`, `FileDeclaration.cshtml`, `Review.cshtml`

### E. Port Operations & Tracking Module
* **Purpose**: Terminal milestone tracking, container location management, public tracking search timeline.
* **Controllers**: `PortOperatorController`, `TrackingController`, `ConsigneeController`
* **Services**: `PortOperatorService`, `TrackingService`
* **Views**: `Views/PortOperator/Index.cshtml`, `RecordMilestone.cshtml`, `Views/Tracking/Search.cshtml`, `Details.cshtml`, `Views/Consignee/Index.cshtml`

### F. Freight Invoicing & Finance Module
* **Purpose**: Invoice creation, demurrage fee calculation, payment processing, revenue tracking.
* **Controller**: `FreightInvoiceController`
* **Service**: `FreightInvoiceService`
* **Views**: `Views/FreightInvoice/Index.cshtml`, `Generate.cshtml`, `Pay.cshtml`, `Details.cshtml`

### G. Reporting & Analytics Module
* **Purpose**: Dynamic reporting for shipments, revenue, containers, demurrage, and exports to Excel/PDF.
* **Controller**: `ReportsController`
* **Service**: `ReportsService`
* **Views**: `Views/Reports/Index.cshtml`, `ViewReport.cshtml`

### H. Administration & Employee Module
* **Purpose**: Staff management, user role setup, system performance monitoring.
* **Controller**: `AdminController`
* **Service**: `EmployeeService`
* **Views**: `Views/Admin/Index.cshtml`, `Employees.cshtml`, `CreateEmployee.cshtml`, `EditEmployee.cshtml`

---

## 9. Page Navigation & Route Map

| Custom Route Name | URL Pattern | Controller / Action | Access Rule |
| :--- | :--- | :--- | :--- |
| `adminDashboard` | `/Admin/Dashboard` | `AdminController.Index` | `Admin` |
| `forwarderDashboard` | `/FreightForwarder/Dashboard` | `ContainerController.Index` | `FreightForwarder` |
| `customsDashboard` | `/CustomsBroker/Dashboard` | `CustomsController.Index` | `CustomsBroker` |
| `portOperatorDashboard` | `/PortOperator/Dashboard` | `PortOperatorController.Index` | `PortOperator` |
| `trackingSearch` | `/Tracking/Search` | `TrackingController.Search` | Public / Authenticated |
| `reportsConsole` | `/Reports` | `ReportsController.Index` | `Admin, FreightForwarder` |
| `invoiceDashboard` | `/FreightInvoice/Dashboard` | `FreightInvoiceController.Index` | `Admin, FreightForwarder` |
| `shipperDashboard` | `/Shipper/Dashboard` | `ShipmentBookingController.Index` | `Shipper` |
| `consigneeDashboard` | `/Consignee/Dashboard` | `ConsigneeController.Index` | `Consignee` |
| `default` | `/{controller=Home}/{action=Index}/{id?}` | `HomeController.Index` | Public |

---

## 10. API & Controller Summary

1. **AccountController**: `Login` (GET/POST), `Register` (GET/POST), `Logout` (POST), `AccessDenied` (GET).
2. **AdminController**: `Index` (GET), `Employees` (GET), `CreateEmployee` (GET/POST), `EditEmployee` (GET/POST), `DeleteEmployee` (POST).
3. **ConsigneeController**: `Index` (GET), `MyShipments` (GET).
4. **ContainerController**: `Index` (GET), `Allocate` (GET/POST), `Details` (GET).
5. **CustomsController**: `Index` (GET), `FileDeclaration` (GET/POST), `Review` (GET/POST).
6. **FreightInvoiceController**: `Index` (GET), `Generate` (GET/POST), `Pay` (GET/POST), `Details` (GET).
7. **HomeController**: `Index` (GET), `Error` (GET).
8. **PortOperatorController**: `Index` (GET), `RecordMilestone` (GET/POST), `ContainerDetails` (GET).
9. **ReportsController**: `Index` (GET), `ViewReport` (GET), `ExportExcel` (GET), `ExportPdf` (GET).
10. **ShipmentBookingController**: `Index` (GET), `Create` (GET/POST), `Edit` (GET/POST), `Cancel` (POST), `Details` (GET), `UploadDocument` (POST).
11. **TrackingController**: `Search` (GET/POST), `Details` (GET).

---

## 11. Service Layer Summary

* **IEmployeeService / EmployeeService**: Handles CRUD operations for employees, updates linked login credentials, and verifies email uniqueness.
* **IShipmentBookingService / ShipmentBookingService**: Manages shipment creation, status updates, booking cancellation, document metadata indexing, and user isolation filters.
* **IContainerService / ContainerService**: Assigns containers to bookings, updates container status, enforces booking container capacity checks.
* **ICustomsService / CustomsService**: Creates declarations, computes duties based on declared values and tariff codes, updates customs clearance statuses.
* **IPortOperatorService / PortOperatorService**: Records container milestone events, maintains event timeline, updates current location of containers.
* **ITrackingService / TrackingService**: Queries shipment history by booking number or container number to construct full tracking timeline ViewModels.
* **IFreightInvoiceService / FreightInvoiceService**: Generates invoices, computes demurrage charges based on terminal stay duration, registers payment transactions.
* **IReportsService / ReportsService**: Queries data for custom date-range reports on shipments, invoices, containers, demurrage, and revenue stats.

---

## 12. Domain Model Summary

1. **Login**: Represents security credentials and role identities (`UserId`, `Password`, `Role`, `AssociatedName`).
2. **Employee**: Represents staff profile details (`employeeId`, `firstName`, `lastName`, `email`, `phoneNumber`, `department`, `designation`, `userId`).
3. **ShipmentBooking**: Represents cargo shipment contracts (`bookingId`, `bookingNumber`, `consigneeName`, `originPort`, `destinationPort`, `cargoWeight`, `cargoDescription`, `bookingStatus`, `bookingDate`, `userId`).
4. **Container**: Represents shipping container units (`containerId`, `containerNumber`, `containerType`, `containerStatus`, `currentLocation`, `bookingId`).
5. **CustomsDeclaration**: Represents regulatory filings (`declarationId`, `declarationNumber`, `declarationType`, `declaredValue`, `hsCode`, `calculatedDuty`, `clearanceStatus`, `bookingId`).
6. **CargoEvent**: Represents granular tracking events (`eventId`, `eventType`, `eventTimestamp`, `location`, `description`, `recordedBy`, `containerId`).
7. **FreightInvoice**: Represents financial invoices (`invoiceId`, `invoiceNumber`, `freightCharges`, `surchargeAmount`, `demurrageAmount`, `totalAmount`, `invoiceStatus`, `dueDate`, `paymentDate`, `bookingId`, `paidByUserId`).
8. **ErrorViewModel**: Captures exception Request ID for display on error pages.

---

## 13. ViewModel Summary

* **LoginViewModel / RegisterViewModel**: Authentication request forms.
* **AdminDashboardViewModel**: System stats overview for administrators.
* **EmployeeViewModel / EmployeeListViewModel**: Staff form binding & list displays.
* **ShipperDashboardViewModel / BookingViewModel / BookingDetailsViewModel**: Shipper dashboards & booking management.
* **ForwarderDashboardViewModel / ContainerAllocationViewModel**: Freight forwarder views & container binding.
* **CustomsDashboardViewModel / FileDeclarationViewModel**: Customs broker forms & clearance queues.
* **PortDashboardViewModel / RecordMilestoneViewModel**: Port operator terminal feeds & event entry.
* **InvoiceDashboardViewModel / PaymentViewModel**: Billing summary & payment processing.
* **TrackingDetailsViewModel / TrackingTimelineItem**: Public & customer tracking views.
* **ReportsViewModels**: Data transfer containers for Report views, Excel exports, and PDF generation.

---

## 14. Application Configuration & Pipeline

### Dependency Injection (`Program.cs`)
```csharp
// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Business Services (Scoped)
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IShipmentBookingService, ShipmentBookingService>();
builder.Services.AddScoped<IContainerService, ContainerService>();
builder.Services.AddScoped<ICustomsService, CustomsService>();
builder.Services.AddScoped<IPortOperatorService, PortOperatorService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<IFreightInvoiceService, FreightInvoiceService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
```

### Middleware Pipeline Order
1. `UseDeveloperExceptionPage()` (Dev) or `UseExceptionHandler()` / `UseHsts()` (Prod)
2. `UseHttpsRedirection()`
3. `UseStaticFiles()`
4. `UseRouting()`
5. `UseAuthentication()`
6. `UseAuthorization()`
7. `MapControllerRoute()` (Role routes + default route)

---

## 15. Seed Data

The database seeds initial demonstration accounts in `ApplicationDbContext.OnModelCreating`:

| User Role | Username / Email | Default Password | Associated Entity |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@cargocaptain.com` | `admin` | Employee ID: 1 |
| **Freight Forwarder** | `forwarder@cargocaptain.com` | `forwarder` | Employee ID: 2 |
| **Customs Broker** | `broker@cargocaptain.com` | `broker` | Employee ID: 3 |
| **Port Operator** | `operator@cargocaptain.com` | `operator` | Employee ID: 4 |

---

## 16. Error Handling Strategy

* **Development vs Production**: Development presents full stack trace diagnostic pages. Production redirects to `/Home/Error` with unique `RequestId` tracing.
* **User Feedback**: Controller actions set `TempData["SuccessMessage"]` or `TempData["ErrorMessage"]` which are rendered as Bootstrap alerts in `_Layout.cshtml`.
* **Resource Isolation**: If a user attempts to access a booking ID or invoice ID that does not belong to them, controllers return `NotFound()` (404) rather than `Forbid()` (403) to prevent unauthorized resource enumeration attacks.

---

## 17. Security Features

* **Anti-Forgery Protection**: Form submission POST actions are guarded with `[ValidateAntiForgeryToken]`.
* **Authentication Security**: Passwords stored via ASP.NET Core `PasswordHasher<Login>`. Cookies marked `HttpOnly` and `SecurePolicy.Always`.
* **Path Traversal Defense**: File uploads extract sanitized filenames (`Path.GetFileName`) and store files under unique GUIDs.
* **Role Authorization**: Actions are protected by strict `[Authorize(Roles = "...")]` attributes.
* **SQL Injection Prevention**: Entity Framework Core parameterizes all SQL queries automatically.

---

## 18. File Upload Logic

* **Location**: Managed in `ShipmentBookingController.UploadDocument`.
* **Destination**: Saved to `wwwroot/uploads/bookings/{bookingId}/{guid_filename}`.
* **Validation Rules**:
  * **Allowed Extensions**: `.pdf`, `.jpg`, `.png`, `.docx`.
  * **Max File Size**: 5 MB (`5 * 1024 * 1024` bytes).
* **Metadata Tracking**: Metadata (original name, stored name, upload date, size) is persisted in `wwwroot/uploads/bookings/{bookingId}/docs_metadata.json`.

---

## 19. Reporting Features

* **Formats**: Interactive HTML Tables, Microsoft Excel (`.xlsx` via **ClosedXML**), and PDF Documents (`.pdf` via **PDFsharp**).
* **Report Types**:
  1. Shipment Status Report
  2. Freight Invoice & Aging Report
  3. Revenue Analytics Report (Admin only)
  4. Demurrage Fee Summary Report
  5. Booking Volume Report
  6. Container Fleet Utilization Report
* **Security**: `ReportsController` enforces role checks (`FreightForwarder` access to revenue/analytics reports returns `Forbid()`).

---

## 20. Current Project Status

* **Status**: Complete & Verified (Phase 15 finished).
* **Build Verification**: Clean build with `0 Warnings` and `0 Errors`.
* **UI Polish**: Fully responsive Razor views using Bootstrap 5 with styled cards, tables, badges, timelines, and navigation layouts.

---

## 21. Known Limitations

1. **Document Metadata Storage**: Document metadata is stored in a JSON file (`docs_metadata.json`) per booking rather than a dedicated SQL table.
2. **Single Currency**: All monetary values (`declaredValue`, `freightCharges`, `totalAmount`) assume USD ($) without multi-currency rate conversion.
3. **Payment Gateway Integration**: Payment processing simulates transaction completion by updating invoice status to `PAID` without live credit card gateway APIs (e.g., Stripe/PayPal).

---

## 22. Future Enhancements

1. **Database Migration for Uploads**: Transition file metadata tracking into a dedicated `BookingDocument` database entity.
2. **Real-Time Push Notifications**: Integrate ASP.NET Core SignalR for real-time tracking milestone updates on dashboard views.
3. **Multi-Currency Support**: Add currency exchange rate calculation for international customs duty processing.
4. **Automated Email Service**: Integrate SMTP / SendGrid for automated invoice generation and status email alerts.

---

## 23. Coding Standards

* **C# / .NET Conventions**: Standard C# 12 conventions (PascalCase methods/classes, camelCase variables).
* **Asynchronous Execution**: Async methods end with `Async` (e.g., `GetBookingByIdAsync`) and return `Task` or `Task<T>`.
* **Dependency Injection**: Dependencies injected exclusively via constructor injection.

---

## 24. Naming Conventions

* **Controllers**: PascalCase ending with `Controller` (e.g., `ShipmentBookingController`).
* **Services & Interfaces**: Interfaces prefixed with `I` (e.g., `IShipmentBookingService`), implementations matching interface without `I` (e.g., `ShipmentBookingService`).
* **ViewModels**: Suffix with `ViewModel` (e.g., `BookingViewModel`).
* **Database Properties**: Entity properties mapped using camelCase in C# models (e.g., `bookingNumber`, `consigneeName`) matching database column definitions.

---

## 25. Important Implementation Details

* **Precision Configuration**: EF Core explicitly configures precision `(18,2)` on all financial and decimal fields (`declaredValue`, `calculatedDuty`, `freightCharges`, `surchargeAmount`, `demurrageAmount`, `totalAmount`, `cargoWeight`) in `ApplicationDbContext.OnModelCreating`.
* **Cascade Delete Rules**: Cascading deletes are configured for child entities (`Containers`, `CargoEvents`, `CustomsDeclaration`, `FreightInvoice`). However, relationship deletes from `Login` to `ShipmentBooking` and `FreightInvoice` use `DeleteBehavior.Restrict` to preserve historical shipping and financial audit logs.
* **Custom Route Definitions**: Specific custom route patterns are registered in `Program.cs` ahead of the default route to provide clean, REST-style role URLs (`/Admin/Dashboard`, `/Shipper/Dashboard`, `/FreightForwarder/Dashboard`, `/CustomsBroker/Dashboard`, `/PortOperator/Dashboard`, `/Consignee/Dashboard`, `/Reports`, `/Tracking/Search`).
