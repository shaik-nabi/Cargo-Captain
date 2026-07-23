# CargoCaptain - Release Changelog

> **Version History, Milestone Completed Work, and Technical Refinements**

---

## [Split Billing Enhancement] - 2026-07-23

### Added
* **Split Freight & Demurrage Billing:** Implemented independent payment tracking for Ocean Freight (billed to Shipper) and Terminal Demurrage Fees (billed to Consignee) on the same invoice.
* **Role-Based Payment Gateways:** Created split `/FreightInvoice/Pay` workflows allowing Shippers to pay freight charges and Consignees to pay demurrage.
* **Consignee Dashboard Actions:** Enhanced the Consignee portal with direct demurrage status visibility and "Pay Demurrage" actions.

---

## [Phase 15 Completion] - 2026-07-21

### Added
* **Document Compliance Upload**: Implemented file upload functionality in `ShipmentBookingController.UploadDocument` supporting `.pdf`, `.jpg`, `.png`, and `.docx` up to 5MB with GUID filenames and JSON metadata tracking (`docs_metadata.json`).
* **Reporting Engine**: Integrated ClosedXML (v0.105.0) for Excel export (`.xlsx`) and PDFsharp (v6.2.4) for PDF export (`.pdf`) across all 6 report types in `ReportsController`.
* **Single Source of Truth Documentation**: Created comprehensive documentation suite in `AI_DOCS/` directory covering overview, architecture, database schema, business rules, workflows, modules, API reference, UI navigation, changelog, and roadmap.

### Refined & Verified
* **Bootstrap 5 UI Polish**: Updated all Razor view templates (`.cshtml`) across all 11 modules to use clean Bootstrap 5 cards, badge styling, responsive tables, and timeline controls.
* **Role Routing & Security**: Verified custom route mappings in `Program.cs` and enforced strict role checks across all controller endpoints.
* **Build Verification**: Executed full solution compilation resulting in **0 Warnings** and **0 Errors** (`net8.0`).

---

## [Phases 1 - 14 Summary]

### Phase 14: Report Console & Analytics Integration
* Created `ReportsService` and `ReportsController` for querying booking volumes, container fleet status, invoice aging, and revenue analytics.

### Phase 13: Freight Invoicing & Demurrage Engine
* Implemented `FreightInvoiceService` with automatic demurrage fee assessment ($50/day after 3 free days) and payment processing.

### Phase 12: Terminal Milestone Tracking & Port Operator Console
* Implemented `PortOperatorService` for logging container events (`GATE_IN` to `GATE_OUT`) and updating real-time container location.

### Phase 11: Customs Clearance Broker Module
* Built `CustomsService` for declaration filing, automated duty calculations (5% / 2.5%), and clearance review workflows (`CLEARED`, `HELD`, `REJECTED`).

### Phase 10: Freight Forwarder Container Allocation
* Built `ContainerService` for assigning containers (`Standard20ft`, `Standard40ft`, `Reefer`, etc.) to pending bookings, moving booking status to `CONFIRMED`.

### Phase 9: Shipper Booking Management
* Implemented `ShipmentBookingService` for creating, editing, and cancelling bookings with data isolation.

### Phase 8: Employee & User Administration
* Created `EmployeeService` and `AdminController` for staff profile management and login credential linking.

### Phases 1 - 7: Core Foundation & Database Schema
* Configured `ApplicationDbContext`, EF Core migrations, SQL Server precision `(18,2)`, seed accounts, and cookie authentication pipeline.
