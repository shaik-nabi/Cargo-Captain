# CargoCaptain - Project Backlog & Feature Roadmap

> **Current Project Status, Pending Items, and Future Enhancements**

---

## 1. Current Project Status Summary

* **Phase 15 Verification**: Completed.
* **Build Status**: **0 Warnings, 0 Errors** (`dotnet build` verified).
* **Core Modules**: 8 functional modules complete (Auth, Admin, Shipper, Forwarder, Customs, Port Operations, Invoicing, Reports).

---

## 2. Completed Features Checklist

- [x] ASP.NET Core 8 MVC Application Setup & Clean N-Tier Architecture
- [x] Entity Framework Core SQL Server configuration with `decimal(18,2)` precision
- [x] Cookie-based authentication with `PasswordHasher<T>` password security
- [x] Pre-seeded demonstration accounts for Admin, Forwarder, Broker, and Operator roles
- [x] Shipper booking creation, editing, cancellation, and data isolation
- [x] Document upload functionality (5MB limit, `.pdf`, `.jpg`, `.png`, `.docx`)
- [x] Forwarder container fleet allocation & booking status progression (`CONFIRMED`)
- [x] Customs declaration filing, auto-duty calculation, and clearance approval workflow
- [x] Port Operator terminal event recording (`GATE_IN` to `GATE_OUT`) & timeline updates
- [x] Freight invoicing, automated demurrage calculation ($50/day after 3 free days), and split payment settlement (Shipper/Consignee)
- [x] Public & Consignee live tracking search with visual timeline UI
- [x] ClosedXML Excel spreadsheet generation & PDFsharp document export
- [x] Responsive Bootstrap 5 Razor views across all 11 Controllers

---

## 3. Immediate Maintenance Tasks (Near-Term)

- [ ] **Document Table Migration**: Migrate document metadata from `docs_metadata.json` into a dedicated `BookingDocument` EF Core SQL Server table.
- [ ] **Automated Integration Tests**: Add an xUnit test project (`CargoCaptain.Tests`) for testing service layer logic (`CustomsService`, `FreightInvoiceService`).
- [ ] **Serilog Integration**: Replace default console logger with Serilog for structured file logging.

---

## 4. Future Enhancements & Feature Roadmap

### A. Real-Time Updates (SignalR Integration)
* Implement ASP.NET Core SignalR hub (`TrackingHub`) to push real-time container milestone updates to active tracking screens without page refreshes.

### B. Multi-Currency & Automated Exchange Rates
* Expand financial entities to support multiple currencies (EUR, GBP, JPY, SGD) with automatic exchange rate conversion for international customs duty processing.

### C. Live Payment Gateway Integration
* Integrate Stripe or PayPal SDKs into `FreightInvoiceController.Pay` to support real credit card and bank transfer processing.

### D. Automated SMTP Notification Engine
* Integrate SendGrid / MailKit for sending automated email notifications to shippers when customs status changes or invoices are generated.

### E. Interactive Terminal Map (GIS Integration)
* Embed Leaflet.js or Google Maps API on tracking details pages to display vessel location coordinates dynamically during transit.
