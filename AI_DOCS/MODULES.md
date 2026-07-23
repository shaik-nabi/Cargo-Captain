# CargoCaptain - Module Documentation

> **Detailed Specifications for all 8 Functional System Modules**

---

## 1. Authentication & Security Module (`Account`)
* **Purpose**: User login, shipper/consignee account registration, session teardown, role redirection, and access denial handling.
* **Controllers**: `AccountController`
* **Models**: `Login`
* **ViewModels**: `LoginViewModel`, `RegisterViewModel`
* **Views**: `Views/Account/Login.cshtml`, `Register.cshtml`, `AccessDenied.cshtml`
* **Key Features**: Cookie claims transformation, password hashing (`PasswordHasher<Login>`), sliding expiration, HTTPS enforcement.

---

## 2. Shipper & Booking Module (`ShipmentBooking`)
* **Purpose**: Enables Shippers to register new ocean freight bookings, edit pending details, cancel requests, and upload compliance documents.
* **Controllers**: `ShipmentBookingController`
* **Services**: `IShipmentBookingService` / `ShipmentBookingService`
* **Models**: `ShipmentBooking`, `Container`, `CargoEvent`, `FreightInvoice`
* **ViewModels**: `ShipperDashboardViewModel`, `BookingViewModel`, `BookingDetailsViewModel`
* **Views**: `Views/ShipmentBooking/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`
* **Key Features**: User data isolation, origin/destination validation, 5MB file upload validation (`.pdf`, `.jpg`, `.png`, `.docx`), document metadata tracking (`docs_metadata.json`).

---

## 3. Freight Forwarder & Container Module (`Container`)
* **Purpose**: Container fleet management, container allocation to pending bookings, booking status progression.
* **Controllers**: `ContainerController`
* **Services**: `IContainerService` / `ContainerService`
* **Models**: `Container`, `ShipmentBooking`
* **ViewModels**: `ForwarderDashboardViewModel`, `ContainerAllocationViewModel`
* **Views**: `Views/Container/Index.cshtml`, `Allocate.cshtml`, `Details.cshtml`
* **Key Features**: ISO container number uniqueness validation, container type assignment (`Standard20ft`, `Standard40ft`, `Reefer`, `FlatRack`, `OpenTop`), automatic booking status update to `CONFIRMED`.

---

## 4. Customs Clearance Module (`Customs`)
* **Purpose**: Regulates customs declarations, tariff classifications, duty calculations, and inspection approvals/rejections.
* **Controllers**: `CustomsController`
* **Services**: `ICustomsService` / `CustomsService`
* **Models**: `CustomsDeclaration`, `ShipmentBooking`
* **ViewModels**: `CustomsDashboardViewModel`, `FileDeclarationViewModel`
* **Views**: `Views/Customs/Index.cshtml`, `FileDeclaration.cshtml`, `Review.cshtml`
* **Key Features**: Auto-duty calculation based on declared value, clearance status workflow (`FILED` ➔ `UNDER_REVIEW` ➔ `CLEARED` / `HELD` / `REJECTED`).

---

## 5. Port Operations Module (`PortOperator`)
* **Purpose**: Real-time terminal tracking, logging physical cargo events, location tracking.
* **Controllers**: `PortOperatorController`
* **Services**: `IPortOperatorService` / `PortOperatorService`
* **Models**: `CargoEvent`, `Container`
* **ViewModels**: `PortDashboardViewModel`, `RecordMilestoneViewModel`
* **Views**: `Views/PortOperator/Index.cshtml`, `RecordMilestone.cshtml`, `ContainerDetails.cshtml`
* **Key Features**: Timestamped event recording (`GATE_IN`, `LOADED`, `SAILED`, `DISCHARGED`, `GATE_OUT`), dynamic container location update.

---

## 6. Tracking & Consignee Visibility Module (`Tracking`, `Consignee`)
* **Purpose**: Public and customer portal for searching live shipment status and milestone event timelines.
* **Controllers**: `TrackingController`, `ConsigneeController`
* **Services**: `ITrackingService` / `TrackingService`
* **ViewModels**: `TrackingDetailsViewModel`, `TrackingTimelineItem`
* **Views**: `Views/Tracking/Search.cshtml`, `Details.cshtml`, `Views/Consignee/Index.cshtml`, `MyShipments.cshtml`
* **Key Features**: Search by booking number or container number, visual timeline renderer, customs clearance status badge.

---

## 7. Freight Invoicing & Finance Module (`FreightInvoice`)
* **Purpose**: Freight billing, demurrage fee assessment, payment processing, invoice status tracking.
* **Controllers**: `FreightInvoiceController`
* **Services**: `IFreightInvoiceService` / `FreightInvoiceService`
* **Models**: `FreightInvoice`, `ShipmentBooking`
* **ViewModels**: `InvoiceDashboardViewModel`, `PaymentViewModel`
* **Views**: `Views/FreightInvoice/Index.cshtml`, `Generate.cshtml`, `Pay.cshtml`, `Details.cshtml`
* **Key Features**: Automated demurrage calculation (3 free days, $50/day thereafter), invoice total aggregation, split payment registration (`paidByUserId` and `demurragePaidByUserId`).

---

## 8. Reporting & Analytics Module (`Reports`)
* **Purpose**: Executive reporting and export capabilities for business intelligence.
* **Controllers**: `ReportsController`
* **Services**: `IReportsService` / `ReportsService`
* **ViewModels**: `ReportsViewModels`
* **Views**: `Views/Reports/Index.cshtml`, `ViewReport.cshtml`
* **Key Features**: Date range filtering, Excel export via **ClosedXML**, PDF document export via **PDFsharp**, strict role restriction (Forwarders denied access to revenue reports).

---

## 9. Admin & Staff Management Module (`Admin`)
* **Purpose**: Staff administration, employee profiles, account creation/deletion, system overview.
* **Controllers**: `AdminController`
* **Services**: `IEmployeeService` / `EmployeeService`
* **Models**: `Employee`, `Login`
* **ViewModels**: `AdminDashboardViewModel`, `EmployeeViewModel`, `EmployeeListViewModel`
* **Views**: `Views/Admin/Index.cshtml`, `Employees.cshtml`, `CreateEmployee.cshtml`, `EditEmployee.cshtml`
* **Key Features**: Employee CRUD operations, automatic login identity creation, email uniqueness check.
