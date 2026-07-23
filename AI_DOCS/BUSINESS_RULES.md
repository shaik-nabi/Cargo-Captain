# CargoCaptain - Business Rules & Validations

> **Validation Logic, Financial Calculations, State Machine Rules, and Access Security**

---

## 1. Validation Rules & Constraints

### Shipment Booking Validations (`ShipmentBookingController` & `ShipmentBookingService`)
1. **Port Isolation Constraint**: `originPort` and `destinationPort` cannot be identical (case-insensitive trim check).
2. **Departure Date Constraint**: `bookingDate` cannot be in the past (`bookingDate.Date >= DateTime.UtcNow.Date`).
3. **Editable State Check**: Shippers can only edit or cancel bookings when status is `BookingStatus.PENDING`. Once `CONFIRMED`, edits are locked.
4. **Data Isolation Security**: Users can only view, edit, cancel, or upload files for bookings matching `booking.userId == currentUserId`. Mismatches return `NotFound()`.

### Document Upload Rules (`ShipmentBookingController.UploadDocument`)
1. **File Extension Whitelist**: Only `.pdf`, `.jpg`, `.png`, and `.docx` extensions are accepted.
2. **File Size Limit**: Maximum file size is 5MB (`5 * 1024 * 1024` bytes).
3. **Path Traversal Protection**: Uses `Path.GetFileName(file.FileName)` to strip path characters, saving files under unique GUID filenames (`{Guid}{extension}`).
4. **Metadata Storage**: Document records are indexed in `wwwroot/uploads/bookings/{bookingId}/docs_metadata.json`.

---

## 2. Business Logic Algorithms

### Customs Duty Calculation Algorithm (`CustomsService`)
Customs duty is computed dynamically during declaration filing based on declared value:
$$\text{Calculated Duty} = \text{Declared Value} \times \text{Duty Rate}$$

Standard rate tiers:
* Standard import tariff rate: **5.0%** (`0.05`)
* Reduced rate for specific HS codes: **2.5%** (`0.025`)
* Result is rounded to 2 decimal places (`decimal(18,2)`).

### Demurrage Fee Calculation Algorithm (`FreightInvoiceService`)
Demurrage charges are levied when containers remain at port terminal past allowed free days:
$$\text{Demurrage Fee} = \max(0, \text{Days at Port} - \text{Free Days}) \times \text{Daily Rate}$$

* **Free Terminal Allowance**: 3 days from `DISCHARGED` (or `GATE_IN` for export) event timestamp.
* **Daily Demurrage Rate**: **$50.00 / day / container**.
* **Payer Responsibility**: 
  *   **Shipper** pays **Ocean Freight Charges** and **Operational Surcharges**.
  *   **Consignee** pays **Terminal Demurrage Fees**.

### Invoice Net Total Algorithm
$$\text{Total Invoice Amount} = \text{Freight Charges} + \text{Surcharges} + \text{Demurrage Amount}$$

---

## 3. Lifecycle State Machine Transitions

### Booking Lifecycle
```
[DRAFT] ──> [PENDING] ──> [CONFIRMED] ──> [COMPLETED]
                │
                └──> [CANCELLED]
```
* `PENDING`: Created by Shipper.
* `CONFIRMED`: Container allocated by Freight Forwarder.
* `CANCELLED`: Cancelled by Shipper prior to confirmation.
* `COMPLETED`: All cargo events logged (`GATE_OUT`) and invoice status is `PAID`.

### Customs Clearance Lifecycle
```
[FILED] ──> [UNDER_REVIEW] ──> [CLEARED]
                 │
                 ├──> [HELD]
                 └──> [REJECTED]
```

### Cargo Event Tracking Lifecycle
```
[GATE_IN] ──> [LOADED] ──> [SAILED] ──> [DISCHARGED] ──> [GATE_OUT]
```
* `GATE_IN`: Container status set to `LOADED`.
* `SAILED`: Container status set to `IN_TRANSIT`.
* `DISCHARGED`: Container status set to `DISCHARGED`. Terminal stay timer begins for demurrage.
* `GATE_OUT`: Final container release.

### Invoice Lifecycle
```
[DRAFT] ──> [ISSUED] ──> [PAID]
               │
               └──> [OVERDUE]
```

---

## 4. Role Authorization Security Rules

1. **Revenue Analytics Protection**: Freight Forwarders are restricted from viewing financial revenue reports (`/Reports/ViewReport?type=revenue`). Controller enforces `User.IsInRole("FreightForwarder") ➔ Forbid()`.
2. **Resource Enumeration Prevention**: Unauthorized access attempts return `404 NotFound` instead of `403 Forbidden` to prevent malicious probing of booking numbers or invoice IDs.
