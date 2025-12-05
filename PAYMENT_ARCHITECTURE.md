# Payment Architecture - RentCollection System

## Overview
This document describes the payment architecture for handling rent payments from tenants to landlords in Kenya, with support for M-Pesa Paybill and other payment methods.

## Payment Identification Strategy

### Primary Method: M-Pesa Paybill with Unit Account Numbers

**Concept:** Each unit gets a unique account number under the landlord's Paybill.

```
Example:
Property: Sunrise Apartments
Landlord Paybill: 123456

Units:
- Unit A101 → Account Number: "A101"
- Unit A102 → Account Number: "A102"
- Unit B201 → Account Number: "B201"
```

**Tenant Payment Process:**
1. Go to M-Pesa → Lipa na M-Pesa → Pay Bill
2. Business Number: `123456`
3. Account Number: `A101` (their unit number)
4. Amount: `15000`
5. PIN → Confirm

**Result:** Payment automatically associated with Unit A101!

## Database Schema

### 1. LandlordPaymentAccount Entity

```csharp
public class LandlordPaymentAccount : BaseEntity
{
    // Landlord/Property Association
    public int LandlordId { get; set; }
    public int? PropertyId { get; set; }  // Can be property-specific

    // Account Details
    public string AccountName { get; set; }  // e.g., "Sunrise Apartments Paybill"
    public PaymentAccountType AccountType { get; set; }  // Paybill, TillNumber, BankAccount, etc.

    // M-Pesa Paybill Details
    public string? PaybillNumber { get; set; }  // e.g., "123456"
    public string? PaybillName { get; set; }    // Business name registered with Paybill

    // M-Pesa Till Number (Alternative - less common for rent)
    public string? TillNumber { get; set; }

    // M-Pesa Phone Number (For small landlords)
    public string? MPesaPhoneNumber { get; set; }

    // Bank Details (Alternative payment method)
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }

    // Settings
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool AutoReconciliation { get; set; }  // Enable automatic payment matching

    // Instructions for tenants
    public string? PaymentInstructions { get; set; }

    // Navigation
    public User Landlord { get; set; } = null!;
    public Property? Property { get; set; }
}

public enum PaymentAccountType
{
    MPesaPaybill = 1,      // M-Pesa Paybill with account numbers
    MPesaTillNumber = 2,   // M-Pesa Till Number
    MPesaPhone = 3,        // Personal M-Pesa number
    BankAccount = 4,       // Bank account
    Cash = 5               // Cash payments
}
```

### 2. Updated Payment Entity

```csharp
public class Payment : BaseEntity
{
    // Existing fields...
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }

    // NEW: Unit and Landlord tracking
    public int UnitId { get; set; }  // Which unit this payment is for
    public int LandlordAccountId { get; set; }  // Which account payment was sent to

    // NEW: Payment identification
    public string PaybillAccountNumber { get; set; }  // e.g., "A101"
    public string? TransactionReference { get; set; }  // M-Pesa confirmation code
    public string? MPesaPhoneNumber { get; set; }  // Tenant's phone that made payment

    // Existing fields...
    public string? PaymentProofUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedByUserId { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public LandlordPaymentAccount LandlordAccount { get; set; } = null!;
    public User? ConfirmedBy { get; set; }
}
```

### 3. Updated Unit Entity

```csharp
public class Unit : BaseEntity
{
    // Existing fields...
    public string UnitNumber { get; set; }
    public int PropertyId { get; set; }
    public decimal MonthlyRent { get; set; }

    // NEW: Payment account number for this unit
    public string PaymentAccountNumber { get; set; }  // e.g., "A101"

    // This can be auto-generated from UnitNumber or custom
    // Used as the Account Number in M-Pesa Paybill payments

    // Navigation properties
    public Property Property { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
```

## Payment Flows

### Flow 1: Tenant Makes Payment (Manual Recording)

**Scenario:** Tenant pays via M-Pesa and records payment in system

```
1. Tenant views payment instructions:
   GET /api/tenants/{id}/payment-instructions
   Returns:
   {
     "paybillNumber": "123456",
     "accountNumber": "A101",
     "unitNumber": "A101",
     "monthlyRent": 15000,
     "propertyName": "Sunrise Apartments",
     "landlordName": "John Kamau"
   }

2. Tenant makes M-Pesa payment (outside system)
   - Lipa na M-Pesa → Pay Bill
   - Business: 123456
   - Account: A101
   - Amount: 15000

3. Tenant records payment in system:
   POST /api/payments/tenant/record
   {
     "amount": 15000,
     "paymentDate": "2025-12-03",
     "paymentMethod": "MPesa",
     "transactionReference": "SKG8N9Q2RT",
     "mPesaPhoneNumber": "0712345678",
     "notes": "December 2025 rent"
   }

4. System auto-fills:
   - TenantId (from auth)
   - UnitId (from tenant record)
   - LandlordAccountId (from unit → property → landlord default account)
   - PaybillAccountNumber (from unit.PaymentAccountNumber)
   - Status: Pending

5. Landlord receives notification to confirm payment

6. Landlord confirms:
   PUT /api/payments/{id}/confirm
   - Checks M-Pesa statement
   - Sees: "Received KSh 15,000 from 0712345678 for account A101"
   - Confirms payment
   - Status: Completed
```

### Flow 2: M-Pesa Webhook (Future - Automatic)

**Scenario:** M-Pesa automatically notifies system when payment received

```
1. Tenant makes M-Pesa payment
   - Lipa na M-Pesa → Pay Bill
   - Business: 123456
   - Account: A101
   - Amount: 15000

2. M-Pesa sends webhook to system:
   POST /api/webhooks/mpesa/c2b
   {
     "TransactionType": "Pay Bill",
     "TransID": "SKG8N9Q2RT",
     "TransTime": "20251203143045",
     "TransAmount": 15000.00,
     "BusinessShortCode": "123456",
     "BillRefNumber": "A101",  ← Unit account number!
     "MSISDN": "254712345678",
     "FirstName": "JOHN",
     "LastName": "DOE"
   }

3. System automatically:
   - Finds Unit by PaymentAccountNumber = "A101"
   - Finds Tenant for that unit
   - Creates Payment record (Status: Completed)
   - Sends notification to tenant and landlord

4. No manual confirmation needed! ✅
```

## Implementation Phases

### Phase 1: Manual Payment Recording (Implement Now)
- ✅ Add LandlordPaymentAccount entity
- ✅ Add PaymentAccountNumber to Unit
- ✅ Create payment instructions endpoint
- ✅ Tenant records payment manually
- ✅ Landlord confirms payment manually

**Timeline:** 1-2 days
**Benefit:** Immediate use, no M-Pesa integration needed

### Phase 2: M-Pesa Daraja API Integration (Future)
- ⏳ Register for M-Pesa Daraja API
- ⏳ Implement C2B (Customer to Business) API
- ⏳ Set up webhook endpoint
- ⏳ Implement automatic payment matching
- ⏳ Add payment reconciliation dashboard

**Timeline:** 1 week
**Benefit:** Fully automated, no manual confirmation

### Phase 3: Advanced Features (Optional)
- ⏳ Payment reminders via SMS
- ⏳ Partial payment support
- ⏳ Payment plans/installments
- ⏳ Automatic late payment penalties
- ⏳ Financial reporting per unit

## API Endpoints Needed

### Tenant-Facing
```
GET    /api/tenants/{id}/payment-instructions          # Get payment details
POST   /api/payments/tenant/record                     # Record payment
GET    /api/payments/tenant/{tenantId}/history         # Payment history
POST   /api/payments/{id}/upload-proof                 # Upload payment proof
```

### Landlord-Facing
```
GET    /api/payments/pending                           # Payments awaiting confirmation
PUT    /api/payments/{id}/confirm                      # Confirm payment
PUT    /api/payments/{id}/reject                       # Reject payment
GET    /api/payments/property/{propertyId}/summary     # Payment summary
```

### Admin/Setup
```
POST   /api/landlord-accounts                          # Create payment account
PUT    /api/landlord-accounts/{id}                     # Update account
GET    /api/landlord-accounts/landlord/{landlordId}    # Get landlord accounts
POST   /api/units/{id}/assign-account-number           # Assign unit account number
```

### Webhooks (Phase 2)
```
POST   /api/webhooks/mpesa/c2b                         # M-Pesa payment callback
POST   /api/webhooks/mpesa/validation                  # M-Pesa validation callback
```

## Security Considerations

1. **Webhook Authentication:** Validate M-Pesa webhook signatures
2. **Payment Proof:** Require screenshot upload for manual entries
3. **Duplicate Prevention:** Check for duplicate transaction references
4. **Amount Validation:** Ensure payment matches expected rent
5. **Audit Trail:** Log all payment confirmations and modifications

## Benefits of This Architecture

✅ **Clear Unit Identification** - Account number = Unit number
✅ **Automatic Tracking** - M-Pesa does the work
✅ **Scalable** - Works for any number of units
✅ **Tenant-Friendly** - Simple payment process
✅ **Landlord-Friendly** - Clear M-Pesa statements
✅ **Audit Trail** - Full payment history per unit
✅ **Future-Proof** - Ready for webhook automation

## Example: Real-World Usage

**Sunrise Apartments - 20 Units**

```
Landlord: John Kamau
Paybill: 123456
Property: Sunrise Apartments

Units & Account Numbers:
A101 → Account: "SUN-A101"
A102 → Account: "SUN-A102"
...
B201 → Account: "SUN-B201"

Tenant Mary in Unit A101:
- Sees instructions: "Pay to Paybill 123456, Account SUN-A101"
- Makes payment
- Landlord's M-Pesa: "Received 15,000 from MARY WANJIRU for SUN-A101"
- System matches payment to Unit A101 automatically
```

## Next Steps

1. Create database migration for new entities
2. Implement LandlordPaymentAccount CRUD
3. Add PaymentAccountNumber to Unit setup
4. Create payment instructions endpoint
5. Update payment recording to include unit tracking
6. Build landlord confirmation workflow
7. Test with sample data
8. Document for end users

---

**Priority:** Implement Phase 1 first - it's simple, effective, and gets you operational immediately!
