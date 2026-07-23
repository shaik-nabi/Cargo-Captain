# CargoCaptain - Controller & API Reference

> **Complete Catalog of Controllers, Action Methods, Parameters, Authorization Rules, and Response Types**

---

## 1. AccountController
**Route Base**: `/Account` | **Authentication**: Public / Mixed

| Action Method | HTTP Verb | Authorization | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Login` | GET | Anonymous | `string returnUrl` | `ViewResult` | Displays login form |
| `Login` | POST | Anonymous | `LoginViewModel model, string returnUrl` | `IActionResult` | Validates credentials, signs in user |
| `Register` | GET | Anonymous | None | `ViewResult` | Displays registration form |
| `Register` | POST | Anonymous | `RegisterViewModel model` | `IActionResult` | Registers new Shipper or Consignee |
| `Logout` | POST | Authenticated | None | `RedirectToActionResult`| Signs out user, clears session cookie |
| `AccessDenied` | GET | Anonymous | None | `ViewResult` | Renders access denied page |

---

## 2. AdminController
**Route Base**: `/Admin` | **Authorization**: `[Authorize(Roles = "Admin")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Admin dashboard with system metrics |
| `Employees` | GET | None | `ViewResult` | Lists all staff employees |
| `CreateEmployee` | GET | None | `ViewResult` | Displays employee creation form |
| `CreateEmployee` | POST | `EmployeeViewModel model` | `IActionResult` | Creates employee and linked login account |
| `EditEmployee` | GET | `int id` | `IActionResult` | Displays edit form for employee |
| `EditEmployee` | POST | `int id, EmployeeViewModel model` | `IActionResult` | Updates employee profile |
| `DeleteEmployee` | POST | `int id` | `IActionResult` | Removes employee and linked credentials |

---

## 3. ShipmentBookingController
**Route Base**: `/ShipmentBooking` (Dashboard: `/Shipper/Dashboard`) | **Authorization**: `[Authorize(Roles = "Shipper")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Shipper dashboard with recent bookings |
| `Create` | GET | None | `ViewResult` | Displays booking creation form |
| `Create` | POST | `BookingViewModel model` | `IActionResult` | Submits new booking (`PENDING`) |
| `Details` | GET | `int id` | `IActionResult` | Displays booking details & document list |
| `Edit` | GET | `int id` | `IActionResult` | Displays edit form for pending booking |
| `Edit` | POST | `int id, BookingViewModel model` | `IActionResult` | Updates pending booking details |
| `Cancel` | POST | `int id` | `IActionResult` | Cancels pending booking |
| `UploadDocument` | POST | `int bookingId, IFormFile file` | `IActionResult` | Uploads compliance document (5MB max) |

---

## 4. ContainerController
**Route Base**: `/Container` (Dashboard: `/FreightForwarder/Dashboard`) | **Authorization**: `[Authorize(Roles = "FreightForwarder")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Forwarder dashboard & booking queue |
| `Allocate` | GET | `int bookingId` | `IActionResult` | Displays container allocation form |
| `Allocate` | POST | `ContainerAllocationViewModel model`| `IActionResult` | Assigns container to booking (`CONFIRMED`) |
| `Details` | GET | `int id` | `IActionResult` | Displays container info & associated events |

---

## 5. CustomsController
**Route Base**: `/Customs` (Dashboard: `/CustomsBroker/Dashboard`) | **Authorization**: `[Authorize(Roles = "CustomsBroker")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Customs Broker dashboard |
| `FileDeclaration` | GET | `int bookingId` | `IActionResult` | Displays declaration filing form |
| `FileDeclaration` | POST | `FileDeclarationViewModel model` | `IActionResult` | Files customs declaration & calculates duty |
| `Review` | GET | `int id` | `IActionResult` | Displays customs review page |
| `Review` | POST | `int id, ClearanceStatus status` | `IActionResult` | Updates clearance status (`CLEARED`, `HELD`, etc.) |

---

## 6. PortOperatorController
**Route Base**: `/PortOperator` (Dashboard: `/PortOperator/Dashboard`) | **Authorization**: `[Authorize(Roles = "PortOperator")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Port Operator dashboard |
| `RecordMilestone` | GET | `int containerId` | `IActionResult` | Displays milestone entry form |
| `RecordMilestone` | POST | `RecordMilestoneViewModel model` | `IActionResult` | Logs cargo event (`GATE_IN` to `GATE_OUT`) |
| `ContainerDetails`| GET | `int id` | `IActionResult` | Displays container event history |

---

## 7. FreightInvoiceController
**Route Base**: `/FreightInvoice` (Dashboard: `/FreightInvoice/Dashboard`) | **Authorization**: `[Authorize(Roles = "Admin,FreightForwarder")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Invoice dashboard summary |
| `Generate` | GET | `int bookingId` | `IActionResult` | Prepares invoice generation form |
| `Generate` | POST | `int bookingId, decimal freight, decimal surcharges` | `IActionResult` | Computes demurrage & creates invoice |
| `Pay` | GET | `int id` | `IActionResult` | Displays payment processing screen |
| `Pay` | POST | `PaymentViewModel model` | `IActionResult` | Processes invoice payment (`PAID`) |
| `Details` | GET | `int id` | `IActionResult` | Displays invoice details |

---

## 8. TrackingController
**Route Base**: `/Tracking` (Search: `/Tracking/Search`) | **Authorization**: Public / Authenticated

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Search` | GET/POST | `string query` | `IActionResult` | Searches cargo by booking or container # |
| `Details` | GET | `int bookingId` | `IActionResult` | Renders tracking timeline & status badges |

---

## 9. ReportsController
**Route Base**: `/Reports` | **Authorization**: `[Authorize(Roles = "Admin,FreightForwarder")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Reports console page |
| `ViewReport` | GET | `string type, DateTime? start, DateTime? end, InvoiceStatus? status` | `IActionResult` | Displays interactive HTML report |
| `ExportExcel` | GET | `string type, DateTime? start, DateTime? end, InvoiceStatus? status` | `FileResult` | Downloads ClosedXML `.xlsx` spreadsheet |
| `ExportPdf` | GET | `string type, DateTime? start, DateTime? end, InvoiceStatus? status` | `FileResult` | Downloads PDFsharp `.pdf` document |

---

## 10. ConsigneeController
**Route Base**: `/Consignee` (Dashboard: `/Consignee/Dashboard`) | **Authorization**: `[Authorize(Roles = "Consignee")]`

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Consignee dashboard |
| `MyShipments` | GET | None | `ViewResult` | Displays list of incoming shipments |

---

## 11. HomeController
**Route Base**: `/Home` | **Authorization**: Public

| Action Method | HTTP Verb | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Index` | GET | None | `ViewResult` | Public landing page |
| `Error` | GET | None | `ViewResult` | Production error handler page |
