# CargoCaptain - End-to-End Business Workflow

> **Complete Operational Lifecycle from User Authentication to Final Shipment Settlement**

---

## 1. Workflow Architecture Diagram

```
┌─────────────────┐     ┌──────────────────────┐     ┌───────────────────────┐
│  1. Authentication│ ──> │ 2. Shipment Booking  │ ──> │3. Container Allocation│
│  (Login / Claims)│     │ (Shipper: PENDING)   │     │ (Forwarder: CONFIRMED)│
└─────────────────┘     └──────────────────────┘     └───────────┬───────────┘
                                                                 │
┌─────────────────┐     ┌──────────────────────┐     ┌───────────▼───────────┐
│6. Final Delivery│ <── │ 5. Freight Invoicing │ <── │ 4. Customs Clearance  │
│  & Completion   │     │ (Finance: ISSUED/PAID│     │ (Broker: CLEARED)     │
└─────────────────┘     └──────────────────────┘     └───────────────────────┘
```

---

## 2. Detailed Step-by-Step Workflow

### Phase 1: Authentication & Role Routing
1. User navigates to `/Account/Login` and enters credentials.
2. `AccountController.Login` verifies password hash with `PasswordHasher<Login>`.
3. Claims are assigned (`NameIdentifier`, `Role`, `AssociatedName`) and written to the authentication cookie.
4. User is redirected to their role's default dashboard route:
   * `Admin` ➔ `/Admin/Dashboard`
   * `Shipper` ➔ `/Shipper/Dashboard`
   * `FreightForwarder` ➔ `/FreightForwarder/Dashboard`
   * `CustomsBroker` ➔ `/CustomsBroker/Dashboard`
   * `PortOperator` ➔ `/PortOperator/Dashboard`
   * `Consignee` ➔ `/Consignee/Dashboard`

### Phase 2: Booking Registration (Shipper)
1. Shipper clicks **New Booking** (`/ShipmentBooking/Create`).
2. Fills out consignee name, origin port, destination port, cargo weight, description, and departure date.
3. System validates ports (Origin ≠ Destination) and departure date (`>= Today`).
4. System creates `ShipmentBooking` entity with a unique `bookingNumber` (`BKG-XXXXXXXX`) and status `BookingStatus.PENDING`.
5. Shipper can upload compliance documents (BOL, packing list, certificates) up to 5MB (`.pdf`, `.jpg`, `.png`, `.docx`).

### Phase 3: Container Allocation (Freight Forwarder)
1. Freight Forwarder views pending bookings on `/FreightForwarder/Dashboard`.
2. Selects booking and navigates to `/Container/Allocate/{bookingId}`.
3. Enters container number (e.g. `MSCU9876543`), container type (`Standard40ft`, `Reefer`, etc.), and initial status (`LOADED`).
4. Upon container creation, `ShipmentBookingService` advances booking status to `BookingStatus.CONFIRMED`.

### Phase 4: Customs Declaration & Duty Clearance (Customs Broker)
1. Customs Broker accesses `/CustomsBroker/Dashboard`.
2. Files declaration via `/Customs/FileDeclaration/{bookingId}` specifying declaration type (`IMPORT`/`EXPORT`), declared value, and HS Code.
3. System calculates duty amount and saves `CustomsDeclaration` with status `ClearanceStatus.FILED`.
4. Broker reviews declaration on `/Customs/Review/{declarationId}` and advances status: `UNDER_REVIEW` ➔ `CLEARED` (or `HELD`/`REJECTED`).

### Phase 5: Terminal Handling & Milestone Tracking (Port Operator)
1. Port Operator logs physical container movements at `/PortOperator/RecordMilestone/{containerId}`.
2. Selects milestone event:
   * `GATE_IN`: Container received at port terminal.
   * `LOADED`: Container loaded onto cargo vessel.
   * `SAILED`: Vessel departs port terminal.
   * `DISCHARGED`: Container unloaded at destination port terminal (demurrage clock starts).
   * `GATE_OUT`: Container picked up for final delivery.
3. Updates `Container.currentLocation` and `ContainerStatus` dynamically.

### Phase 6: Freight Invoicing & Financial Settlement (Finance / Forwarder)
1. Forwarder/Finance team opens `/FreightInvoice/Dashboard`.
2. Clicks `/FreightInvoice/Generate/{bookingId}`.
3. System computes base freight charges, surcharges, and assesses demurrage fees if discharge-to-gateout duration exceeds 3 free days ($50/day).
4. Invoice is generated with status `InvoiceStatus.ISSUED`.
5. Shipper pays the freight portion via `/FreightInvoice/Pay/{invoiceId}`, setting `invoiceStatus` to `InvoiceStatus.PAID`. If demurrage is incurred, the Consignee pays the demurrage portion via `/FreightInvoice/Pay/{invoiceId}`, setting `demurrageStatus` to `InvoiceStatus.PAID`. When both are paid, the booking is marked complete.

### Phase 7: Tracking Visibility & Completion
1. Shippers and Consignees search tracking history at `/Tracking/Search` using `bookingNumber` or `containerNumber`.
2. System displays a visual timeline of all `CargoEvent` entries, customs clearance badge, container location, and invoice status.
3. When all milestones reach `GATE_OUT` and invoice is `PAID`, booking is marked `BookingStatus.COMPLETED`.
