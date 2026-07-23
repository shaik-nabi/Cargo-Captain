# CargoCaptain - UI & Navigation Architecture

> **User Interface Layouts, Page Navigation Flow, Custom Routes, and View Catalog**

---

## 1. Master Layout Architecture

The application layout is structured around `Views/Shared/_Layout.cshtml`:

```
┌─────────────────────────────────────────────────────────────┐
│                      Top Navigation Bar                     │
│  [Logo: CargoCaptain]   [Dashboard Links]   [User Profile]  │
├─────────────────────────────────────────────────────────────┤
│                      Bootstrap Alerts                       │
│  [TempData["SuccessMessage"]] / [TempData["ErrorMessage"]]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                      Main Body Container                    │
│                      @RenderBody()                          │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│                           Footer                            │
│  © 2026 CargoCaptain - Maritime Logistics System            │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Page Navigation & Route Mapping

Routes are configured in `Program.cs` mapping friendly URLs to specific Controllers and Actions:

| Route Name | Custom URL Pattern | Target Controller & Action | Primary Role |
| :--- | :--- | :--- | :--- |
| `adminDashboard` | `/Admin/Dashboard` | `AdminController.Index` | `Admin` |
| `forwarderDashboard` | `/FreightForwarder/Dashboard` | `ContainerController.Index` | `FreightForwarder` |
| `customsDashboard` | `/CustomsBroker/Dashboard` | `CustomsController.Index` | `CustomsBroker` |
| `portOperatorDashboard` | `/PortOperator/Dashboard` | `PortOperatorController.Index` | `PortOperator` |
| `trackingSearch` | `/Tracking/Search` | `TrackingController.Search` | Public / All |
| `reportsConsole` | `/Reports` | `ReportsController.Index` | `Admin, FreightForwarder` |
| `invoiceDashboard` | `/FreightInvoice/Dashboard` | `FreightInvoiceController.Index` | `Admin, FreightForwarder` |
| `shipperDashboard` | `/Shipper/Dashboard` | `ShipmentBookingController.Index` | `Shipper` |
| `consigneeDashboard` | `/Consignee/Dashboard` | `ConsigneeController.Index` | `Consignee` |

---

## 3. Razor View Catalog

```
Views/
├── Account/
│   ├── Login.cshtml                 # Login screen with validation summaries
│   ├── Register.cshtml              # Shipper/Consignee registration
│   └── AccessDenied.cshtml          # Access forbidden response page
├── Admin/
│   ├── Index.cshtml                 # System metrics & admin cards
│   ├── Employees.cshtml             # Data table of all staff members
│   ├── CreateEmployee.cshtml        # Employee creation form
│   └── EditEmployee.cshtml          # Employee profile editor
├── Consignee/
│   ├── Index.cshtml                 # Consignee dashboard summary
│   └── MyShipments.cshtml           # Data table of inbound shipments
├── Container/
│   ├── Index.cshtml                 # Freight Forwarder booking queue
│   ├── Allocate.cshtml              # Container allocation form
│   └── Details.cshtml               # Container status viewer
├── Customs/
│   ├── Index.cshtml                 # Customs Broker queue
│   ├── FileDeclaration.cshtml       # Declaration filing form
│   └── Review.cshtml                # Inspection & status approval form
├── FreightInvoice/
│   ├── Index.cshtml                 # Finance dashboard summary
│   ├── Generate.cshtml              # Freight & demurrage invoice generator
│   ├── Pay.cshtml                   # Payment settlement screen
│   └── Details.cshtml               # Invoice viewer
├── Home/
│   ├── Index.cshtml                 # Public landing portal
│   └── Error.cshtml                 # Production error page
├── PortOperator/
│   ├── Index.cshtml                 # Port Operator terminal queue
│   ├── RecordMilestone.cshtml       # Cargo event entry form
│   └── ContainerDetails.cshtml      # Event timeline viewer
├── Reports/
│   ├── Index.cshtml                 # Reporting console with export buttons
│   └── ViewReport.cshtml            # Interactive HTML report viewer
├── Shared/
│   ├── _Layout.cshtml               # Global HTML master template
│   ├── _ValidationScriptsPartial.cshtml # Client-side validation scripts
│   └── _ViewImports.cshtml          # Global namespaces & tag helpers
├── ShipmentBooking/
│   ├── Index.cshtml                 # Shipper dashboard & booking list
│   ├── Create.cshtml                # Booking request form
│   ├── Edit.cshtml                  # Pending booking editor
│   └── Details.cshtml               # Booking details & upload document dropzone
└── Tracking/
    ├── Search.cshtml                # Search input form
    └── Details.cshtml               # Visual tracking timeline UI
```

---

## 4. UI Design System Principles

* **Bootstrap 5 Card Layouts**: All dashboards feature metric cards (Total, Pending, Completed, Financial totals) with Bootstrap icons.
* **Badges for Status**: Status values use color-coded badges:
  * `PENDING` / `UNDER_REVIEW` / `ISSUED`: Warning (Yellow)
  * `CONFIRMED` / `CLEARED` / `PAID`: Success (Green)
  * `CANCELLED` / `REJECTED` / `OVERDUE`: Danger (Red)
* **Responsive Tables**: All data listings utilize `.table .table-hover .table-striped` classes for readability.
* **Timeline Component**: Custom CSS timeline control for `Tracking/Details` displaying chronological `CargoEvent` nodes.
