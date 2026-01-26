# System Architecture

## Overview

The RentCollection system uses Clean Architecture with Domain-Driven Design (DDD) principles, separating concerns into distinct layers.

## Architecture Layers

```
┌─────────────────────────────────────────────┐
│           Presentation Layer                │
│  (API Controllers, Next.js Frontend)        │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│         Application Layer                   │
│  (Services, DTOs, Business Logic)           │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│           Domain Layer                      │
│  (Entities, Enums, Domain Logic)            │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│       Infrastructure Layer                  │
│  (Data Access, External APIs, Services)     │
└─────────────────────────────────────────────┘
```

### Domain Layer

Core business entities and logic:
- `User`, `Property`, `Unit`, `Tenant`
- `Payment`, `LandlordPaymentAccount`
- Enums: `UserRole`, `PaymentMethod`, `PaymentStatus`, `PaymentAccountType`

### Application Layer

Business services and operations:
- `IAuthService`, `IPropertyService`, `IUnitService`
- `ITenantService`, `IPaymentService`
- `ILandlordPaymentAccountService`, `IMPesaService`
- DTOs for API requests/responses

### Infrastructure Layer

Data persistence and external integrations:
- Entity Framework Core with SQL Server
- M-Pesa Daraja API client
- SMS service (Africa's Talking)
- PDF generation service
- File storage

### Presentation Layer

User interfaces and API:
- REST API (ASP.NET Core)
- Next.js frontend (TypeScript + React)

## Database Schema

### Core Entities

**Users**
- SystemAdmin, Landlord, Caretaker, Accountant, Tenant roles
- JWT-based authentication
- Role-based authorization

**Property Management**
```
Landlord (User)
  └── Properties
        └── Units
              └── Tenants
```

### Payment System

**Payment Account Strategy**

Each landlord can have multiple payment accounts (M-Pesa Paybill, Bank Account, etc.):

```csharp
public class LandlordPaymentAccount
{
    public int Id { get; set; }
    public int LandlordId { get; set; }
    public int? PropertyId { get; set; }
    public string AccountName { get; set; }
    public PaymentAccountType AccountType { get; set; }

    // M-Pesa Paybill
    public string? PaybillNumber { get; set; }
    public string? PaybillName { get; set; }

    // Bank Account
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public enum PaymentAccountType
{
    MPesaPaybill = 1,
    MPesaTillNumber = 2,
    MPesaPhone = 3,
    BankAccount = 4,
    Cash = 5
}
```

**Unit Payment Identification**

Each unit has a unique payment account number:

```csharp
public class Unit
{
    public int Id { get; set; }
    public string UnitNumber { get; set; }
    public string PaymentAccountNumber { get; set; }  // e.g., "A101"
    public int PropertyId { get; set; }
    public decimal MonthlyRent { get; set; }
}
```

**Payment Tracking**

```csharp
public class Payment
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int UnitId { get; set; }
    public int LandlordAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }

    // Payment identification
    public string PaybillAccountNumber { get; set; }
    public string? TransactionReference { get; set; }
    public string? MPesaPhoneNumber { get; set; }

    // Confirmation
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedByUserId { get; set; }
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Rejected = 3,
    Failed = 4
}
```

**Invoicing and Allocation**

Invoices track monthly rent obligations and arrears. Payments are allocated FIFO to outstanding invoices.

```csharp
public class Invoice
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int UnitId { get; set; }
    public int PropertyId { get; set; }
    public int LandlordId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Balance { get; set; }
    public InvoiceStatus Status { get; set; }
}

public class PaymentAllocation
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
}
```

Unique constraint: `TenantId + PeriodStart + PeriodEnd` to guarantee idempotent invoice generation.

## Background Jobs

The API host runs background services for recurring workflows:

- **RentReminderBackgroundService**: schedules reminders hourly and sends due reminders every 5 minutes.
- **MpesaStkReconciliationBackgroundService**: checks pending STK pushes every 10 minutes with a 2-minute minimum age, batch size 50.
- **InvoiceGenerationBackgroundService**: generates monthly invoices on a 12-hour interval with idempotency checks.

## Payment Flow Architecture

### M-Pesa Paybill Flow (Recommended)

**Setup:**
1. Landlord registers M-Pesa Paybill business account
2. Each unit gets unique account number (e.g., Unit A101 → Account "A101")
3. System stores paybill number and unit account numbers

**Payment Process:**
```
1. Tenant views payment instructions
   └─> GET /api/TenantPayments/instructions
   └─> Returns: Paybill: 123456, Account: A101

2. Tenant pays via M-Pesa
   └─> Lipa na M-Pesa → Pay Bill
   └─> Business: 123456, Account: A101

3. M-Pesa processes payment
   └─> Money → Landlord's Paybill account
   └─> SMS to landlord: "Received KSh 15,000 for account A101"

4. Tenant records payment in system
   └─> POST /api/TenantPayments/record
   └─> Status: Pending

5. Landlord confirms payment
   └─> Checks M-Pesa statement for account A101
   └─> PUT /api/Payments/{id}/confirm
   └─> Status: Completed
```

**Benefits:**
- Automatic unit identification via account number
- Clear landlord M-Pesa statements
- Ready for webhook automation (future)

### Bank Account Flow

**Setup:**
1. Landlord provides bank account details
2. Each unit gets unique reference code (e.g., "RENT-A101")

**Payment Process:**
```
1. Tenant views payment instructions
   └─> Shows bank account + reference code

2. Tenant makes bank transfer
   └─> Must include reference code in narration

3. Tenant records payment + uploads receipt
   └─> POST /api/TenantPayments/record

4. Landlord verifies bank statement
   └─> Checks reference code matches
   └─> Confirms or rejects payment
```

## Data Isolation & Security

### Role-Based Access Control

| Role | Access Scope |
|------|--------------|
| **SystemAdmin** | All properties (all landlords) |
| **Landlord** | Only properties they own (`Property.LandlordId = User.Id`) |
| **Caretaker** | Single property (`Property.Id = User.PropertyId`) |
| **Accountant** | All properties (read-only) |
| **Tenant** | Own tenant record only |

### Payment Security

1. **Webhook Authentication**: Validate M-Pesa webhook signatures
2. **Payment Proof**: Require screenshot upload for manual entries
3. **Duplicate Prevention**: Check for duplicate transaction references
4. **Amount Validation**: Ensure payment matches expected rent
5. **Audit Trail**: Log all payment confirmations and modifications

## External Integrations

### M-Pesa Daraja API

**STK Push (Lipa na M-Pesa Online)**
- Initiate payment from system
- Push PIN prompt to tenant's phone
- Callback confirms payment status

**C2B (Customer to Business) - Future**
- Automatic webhook when payment received
- No manual confirmation needed
- Real-time payment reconciliation

### SMS Service (Africa's Talking)

- Payment reminders
- Payment confirmation notifications
- Overdue rent alerts

## Folder Structure

```
src/
├── RentCollection.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Property.cs
│   │   ├── Unit.cs
│   │   ├── Tenant.cs
│   │   ├── Payment.cs
│   │   └── LandlordPaymentAccount.cs
│   ├── Enums/
│   └── Interfaces/
│
├── RentCollection.Application/
│   ├── DTOs/
│   ├── Services/
│   │   ├── IAuthService.cs
│   │   ├── IPaymentService.cs
│   │   ├── ITenantPaymentService.cs
│   │   └── IMPesaService.cs
│   └── Mappings/
│
├── RentCollection.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── Services/
│   │   ├── PaymentService.cs
│   │   ├── MPesaService.cs
│   │   └── SmsService.cs
│   └── Repositories/
│
├── RentCollection.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── PropertiesController.cs
│   │   ├── TenantsController.cs
│   │   ├── PaymentsController.cs
│   │   ├── TenantPaymentsController.cs
│   │   └── LandlordPaymentAccountsController.cs
│   └── Program.cs
│
└── RentCollection.WebApp/
    ├── app/
    │   ├── dashboard/
    │   ├── properties/
    │   ├── payments/
    │   └── tenant-portal/
    ├── lib/
    │   ├── services/
    │   └── types/
    └── components/
```

## Design Decisions

### Why M-Pesa Paybill Over Till Number?

**Paybill advantages:**
- Supports multiple account numbers (one per unit)
- Automatic unit identification
- Better for businesses with multiple units

**Till Number limitations:**
- Single account, no sub-accounts
- Harder to track which unit paid
- Better for single-unit rentals only

### Why Manual Confirmation?

While M-Pesa webhooks can automate this, manual confirmation:
- Prevents fraudulent payment claims
- Gives landlords control over reconciliation
- Allows verification before marking complete
- Can be automated later with webhook integration

### Why Separate Payment Accounts?

- Supports landlords with multiple properties
- Different payment methods per property
- Bank account for one property, M-Pesa for another
- Flexibility for landlord preferences
