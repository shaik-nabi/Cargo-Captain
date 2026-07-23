# CargoCaptain - Database Schema & Design

> **Entity Relationship Diagram, Tables, Primary/Foreign Keys, Constraints, & Enums**

---

## 1. Database Context & Provider
* **DbContext**: `ApplicationDbContext` (`CargoCaptain.Data`)
* **Provider**: SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`)
* **Connection String Key**: `DefaultConnection` in `appsettings.json`

---

## 2. Table Schemas & Entities

### A. Logins (`Logins`)
Stores authentication accounts and user roles.

| Column | Data Type | Nullable | Key / Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| `UserId` | `int` | No | PK, Auto-Increment | User Primary Key |
| `Password` | `string` | No | Hashed | Hashed password via `PasswordHasher<T>` |
| `Role` | `UserRole` (int)| No | Enum | `Admin`, `Shipper`, `FreightForwarder`, `CustomsBroker`, `PortOperator`, `Consignee` |
| `AssociatedName` | `string` | No | - | Display name of user/entity |

### B. Employees (`Employees`)
Stores internal staff profile details.

| Column | Data Type | Nullable | Key / Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| `employeeId` | `int` | No | PK, Auto-Increment | Employee Primary Key |
| `firstName` | `string` | No | - | First name |
| `lastName` | `string` | No | - | Last name |
| `email` | `string` | No | Unique Index | Employee email address |
| `phoneNumber` | `string` | No | - | Phone number |
| `department` | `string` | Yes | - | Department name |
| `designation` | `string` | Yes | - | Job title / designation |
| `userId` | `int` | No | FK ➔ `Logins.UserId` | Unique FK, Cascade Delete |

### C. ShipmentBookings (`ShipmentBookings`)
Stores cargo shipment booking contracts.

| Column | Data Type | Nullable | Key / Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| `bookingId` | `int` | No | PK, Auto-Increment | Booking Primary Key |
| `bookingNumber` | `string` | No | Unique Index | Booking reference (e.g. `BKG-100234`) |
| `consigneeName` | `string` | No | - | Recipient name |
| `originPort` | `string` | No | - | Port of origin |
| `destinationPort` | `string` | No | - | Destination port |
| `cargoWeight` | `decimal(18,2)`| No | Precision (18,2) | Total cargo weight in metric tons |
| `cargoDescription` | `string` | No | - | Freight description |
| `bookingStatus` | `BookingStatus` | No | Enum | `DRAFT`, `PENDING`, `CONFIRMED`, `CANCELLED`, `COMPLETED` |
| `bookingDate` | `DateTime` | No | - | Target departure date |
| `userId` | `int` | No | FK ➔ `Logins.UserId` | Created by Shipper, Restrict Delete |

### D. Containers (`Containers`)
Stores container inventory linked to bookings.

| Column | Data Type | Nullable | Key / Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| `containerId` | `int` | No | PK, Auto-Increment | Container Primary Key |
| `containerNumber` | `string` | No | Unique Index | ISO Container Number (e.g. `MSCU1234567`) |
| `containerType` | `ContainerType` | No | Enum | `Standard20ft`, `Standard40ft`, `Reefer`, `FlatRack`, `OpenTop` |
| `containerStatus` | `ContainerStatus`| No | Enum | `EMPTY`, `LOADED`, `IN_TRANSIT`, `DISCHARGED` |
| `currentLocation` | `string` | Yes | - | Terminal / Vessel / Yard location |
| `bookingId` | `int` | No | FK ➔ `ShipmentBookings.bookingId` | Belongs to booking, Cascade Delete |

### E. CustomsDeclarations (`CustomsDeclarations`)
Stores regulatory customs clearance documents.

| Column | Data Type | Nullable | Key / Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| `declarationId` | `int` | No | PK, Auto-Increment | Declaration Primary Key |
| `declarationNumber` | `string` | No | - | Declaration reference |
| `declarationType` | `DeclarationType`| No | Enum | `IMPORT`, `EXPORT`, `TRANSIT` |
| `declaredValue` | `decimal(18,2)`| No | Precision (18,2) | Declared cargo value in USD |
| `hsCode` | `string` | No | - | Harmonized System Tariff Code |
| `calculatedDuty` | `decimal(18,2)`| No | Precision (18,2) | Computed duty charge |
| `clearanceStatus` | `ClearanceStatus`| No | Enum | `FILED`, `UNDER_REVIEW`, `CLEARED`, `HELD`, `REJECTED` |
| `bookingId` | `int` | No | FK ➔ `ShipmentBookings.bookingId` | 1-to-1 relationship, Cascade Delete |

### F. CargoEvents (`CargoEvents`)
Stores granular tracking events for containers.

| Column | Data Type | Nullable | Key / Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| `eventId` | `int` | No | PK, Auto-Increment | Event Primary Key |
| `eventType` | `CargoEventType` | No | Enum | `GATE_IN`, `LOADED`, `SAILED`, `DISCHARGED`, `GATE_OUT` |
| `eventTimestamp` | `DateTime` | No | - | Date/time event occurred |
| `location` | `string` | No | - | Terminal or port location |
| `description` | `string` | Yes | - | Event comments |
| `recordedBy` | `string` | No | - | Operator identity |
| `containerId` | `int` | No | FK ➔ `Containers.containerId` | Linked container, Cascade Delete |

### G. FreightInvoices (`FreightInvoices`)
Stores billing invoices and payment details.

| Column | Data Type | Nullable | Key / Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| `invoiceId` | `int` | No | PK, Auto-Increment | Invoice Primary Key |
| `invoiceNumber` | `string` | No | Unique Index | Invoice reference (e.g. `INV-2026-001`) |
| `freightCharges` | `decimal(18,2)`| No | Precision (18,2) | Base freight cost |
| `surchargeAmount` | `decimal(18,2)`| No | Precision (18,2) | Fuel/bunker surcharges |
| `demurrageAmount` | `decimal(18,2)`| No | Precision (18,2) | Overdue container storage fees |
| `totalAmount` | `decimal(18,2)`| No | Precision (18,2) | Net invoice total |
| `invoiceStatus` | `InvoiceStatus` | No | Enum | `DRAFT`, `ISSUED`, `PAID`, `OVERDUE` |
| `dueDate` | `DateTime` | No | - | Payment due date |
| `paymentDate` | `DateTime?` | Yes | - | Timestamp when paid |
| `bookingId` | `int` | No | FK ➔ `ShipmentBookings.bookingId` | 1-to-1 relationship, Cascade Delete |
| `paidByUserId` | `int?` | Yes | FK ➔ `Logins.UserId` | Paying account ID, Restrict Delete |

---

## 3. System Enums

1. **UserRole**: `Admin` (0), `Shipper` (1), `FreightForwarder` (2), `CustomsBroker` (3), `PortOperator` (4), `Consignee` (5)
2. **BookingStatus**: `DRAFT` (0), `PENDING` (1), `CONFIRMED` (2), `CANCELLED` (3), `COMPLETED` (4)
3. **ContainerType**: `Standard20ft` (0), `Standard40ft` (1), `Reefer` (2), `FlatRack` (3), `OpenTop` (4)
4. **ContainerStatus**: `EMPTY` (0), `LOADED` (1), `IN_TRANSIT` (2), `DISCHARGED` (3)
5. **DeclarationType**: `IMPORT` (0), `EXPORT` (1), `TRANSIT` (2)
6. **ClearanceStatus**: `FILED` (0), `UNDER_REVIEW` (1), `CLEARED` (2), `HELD` (3), `REJECTED` (4)
7. **CargoEventType**: `GATE_IN` (0), `LOADED` (1), `SAILED` (2), `DISCHARGED` (3), `GATE_OUT` (4)
8. **InvoiceStatus**: `DRAFT` (0), `ISSUED` (1), `PAID` (2), `OVERDUE` (3)
