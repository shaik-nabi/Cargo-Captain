# CargoCaptain - High-Level Project Context

> **AI Assistant & Developer Quick-Reference Overview**

---

## 1. Project Overview
* **Project Name**: CargoCaptain (`CargoCaptain.csproj`, root folder: `projectcargo`)
* **Purpose**: An enterprise-grade Maritime Cargo and Logistics Management System designed for end-to-end management of ocean freight operations.
* **Business Domain**: Ocean shipping, port terminal handling, container logistics, customs clearance brokerage, freight billing, and tracking.
* **Main Objectives**:
  * Streamline the shipment lifecycle from initial customer booking through to port discharge and final consignee delivery.
  * Provide role-based operational dashboards for Shippers, Freight Forwarders, Customs Brokers, Port Operators, Consignees, Finance Teams, and System Administrators.
  * Automate container assignment, tracking milestone events, customs duty processing, demurrage calculations, and exportable business reports.

---

## 2. Technology Stack

| Category | Technology / Library | Version / Details |
| :--- | :--- | :--- |
| **Framework** | .NET / C# | .NET 8.0 (`net8.0`), C# 12 |
| **App Model** | ASP.NET Core MVC | Model-View-Controller with Razor Views (`.cshtml`) |
| **ORM** | Entity Framework Core | EF Core 8.0 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| **Database** | SQL Server | Relational DB with explicit decimal precision `(18,2)` |
| **Authentication** | Cookie Authentication | Claims-based identity with `PasswordHasher<T>` security |
| **Frontend UI** | Vanilla CSS & Bootstrap 5 | Responsive grid, cards, tables, badges, and icons |
| **Reporting (Excel)**| ClosedXML | Version 0.105.0 for Excel `.xlsx` generation |
| **Reporting (PDF)**  | PDFsharp | Version 6.2.4 for PDF `.pdf` generation |

---

## 3. Solution Structure

```
AI_DOCS/                   # Comprehensive AI & Technical documentation suite
├── PROJECT_CONTEXT.md     # High-level overview & tech stack summary (This file)
├── ARCHITECTURE.md        # Architecture, patterns, DI, & auth pipeline
├── DATABASE.md            # Schema, entities, relationships, & enums
├── BUSINESS_RULES.md      # Validations, duty & demurrage rules, security
├── WORKFLOW.md            # Step-by-step shipment lifecycle
├── MODULES.md             # In-depth module documentation
├── API_REFERENCE.md       # Controller & action method specifications
├── UI_NAVIGATION.md       # Navigation, routes, & Razor view catalog
├── CHANGELOG.md           # Implementation history & recent updates
└── TODO.md                # Completed work, backlog & future roadmap

projectcargo/ (Root)
├── CargoCaptain.csproj     # Project file & package references
├── Program.cs              # DI container, middleware pipeline, & route mappings
├── appsettings.json        # Main configuration file (DB Connection strings)
├── Controllers/            # 11 ASP.NET Core MVC Controllers
├── Services/               # 8 Business logic service implementations
├── Interfaces/             # 8 Service interface contracts
├── Models/                 # 8 Domain entity models
├── ViewModels/             # 19 Strongly-typed ViewModels
├── Data/                   # ApplicationDbContext, EF Core config, & seed data
├── Enums/                  # 8 Core system enums
├── Helpers/                # Utility helpers (File, Date, Currency, Number generators)
├── Views/                  # Razor Views organized by module subdirectories
└── wwwroot/                # Static assets & document upload store
```

---

## 4. Pre-Seeded Demonstration Accounts

| Role | Email / Username | Password | Linked Entity |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@cargocaptain.com` | `admin` | Employee ID: 1 |
| **Freight Forwarder** | `forwarder@cargocaptain.com` | `forwarder` | Employee ID: 2 |
| **Customs Broker** | `broker@cargocaptain.com` | `broker` | Employee ID: 3 |
| **Port Operator** | `operator@cargocaptain.com` | `operator` | Employee ID: 4 |

---

## 5. Documentation Suite Index

Refer to the remaining files in this `AI_DOCS/` directory for detailed specifications:
* See [ARCHITECTURE.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/ARCHITECTURE.md) for architectural patterns and DI setup.
* See [DATABASE.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/DATABASE.md) for database schema and entity diagrams.
* See [BUSINESS_RULES.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/BUSINESS_RULES.md) for business logic and validation constraints.
* See [WORKFLOW.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/WORKFLOW.md) for the end-to-end shipment lifecycle.
* See [MODULES.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/MODULES.md) for module-by-module features.
* See [API_REFERENCE.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/API_REFERENCE.md) for complete Controller action method signatures.
* See [UI_NAVIGATION.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/UI_NAVIGATION.md) for page routing and UI layout structure.
* See [CHANGELOG.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/CHANGELOG.md) for recent changes and version history.
* See [TODO.md](file:///c:/Users/SHAIK%20NABI%20RASOOL/Desktop/example/projectcargo/AI_DOCS/TODO.md) for current feature status and pending enhancements.
