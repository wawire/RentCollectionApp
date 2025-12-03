# Payment Flow Diagram - RentCollection System

## 🎯 Goal: Ensure Payments Reach Right Landlord & Identified for Right Unit

---

## Scenario 1: M-Pesa Paybill (RECOMMENDED - Automatic Identification)

### Setup Phase
```
┌─────────────────────────────────────────────────────────────┐
│ SYSTEM SETUP                                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Landlord: John Kamau                                       │
│  ├─ Has M-Pesa Paybill: 123456                             │
│  └─ Owns Property: Sunrise Apartments                       │
│                                                             │
│  Property: Sunrise Apartments                               │
│  ├─ Unit A101 → Payment Account Number: "A101"             │
│  ├─ Unit A102 → Payment Account Number: "A102"             │
│  ├─ Unit B201 → Payment Account Number: "B201"             │
│  └─ Unit B202 → Payment Account Number: "B202"             │
│                                                             │
│  Tenant: Mary Wanjiru                                       │
│  └─ Lives in Unit A101                                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Payment Flow - How It Works

```
┌────────────────┐
│  TENANT MARY   │
│  (Unit A101)   │
└────────┬───────┘
         │
         │ 1. Views Payment Instructions
         │    GET /api/tenants/payment-instructions
         │
         ├─ Returns:
         │  • Paybill: 123456
         │  • Account Number: A101 ← UNIT IDENTIFIER!
         │  • Amount: KSh 15,000
         │  • Landlord: John Kamau
         │
         ▼
┌────────────────────────────────────┐
│  TENANT'S M-PESA PHONE             │
│                                    │
│  Lipa na M-Pesa → Pay Bill         │
│  • Business Number: 123456         │
│  • Account Number: A101  ◄─────────┼── THIS IS THE MAGIC!
│  • Amount: 15,000                  │    Unit number = Account number
│  • Enter PIN                       │
│                                    │
└────────────┬───────────────────────┘
             │
             │ 2. Payment Sent to M-Pesa
             │
             ▼
┌──────────────────────────────────────────┐
│  SAFARICOM M-PESA                        │
│                                          │
│  Processes payment:                      │
│  • From: 0712345678 (Mary's number)      │
│  • To Paybill: 123456 (John's Paybill)   │
│  • Account: A101 ◄─────────────────────  │ TRACKS THE UNIT!
│  • Amount: KSh 15,000                    │
│  • Confirmation: SKG8N9Q2RT              │
│                                          │
└────────────┬─────────────────────────────┘
             │
             │ 3. Money deposited to John's M-Pesa business account
             │    SMS to John: "Received KSh 15,000 from 0712345678
             │                 Account: A101, Ref: SKG8N9Q2RT"
             │
             ▼
┌──────────────────────────────────────────┐
│  LANDLORD JOHN'S M-PESA STATEMENT        │
│                                          │
│  Date: 03/12/2025                        │
│  From: 0712345678 (MARY WANJIRU)         │
│  Account: A101 ◄─────────────────────────┼── JOHN KNOWS IT'S UNIT A101!
│  Amount: KSh 15,000                      │
│  Balance: KSh 350,000                    │
│                                          │
└────────────┬─────────────────────────────┘
             │
             │ 4. Tenant records payment in system
             │    POST /api/payments/tenant/record
             │    {
             │      "transactionRef": "SKG8N9Q2RT",
             │      "amount": 15000,
             │      "mpesaPhone": "0712345678"
             │    }
             │
             ▼
┌──────────────────────────────────────────┐
│  SYSTEM AUTO-FILLS:                      │
│                                          │
│  • TenantId: 5 (Mary)                    │
│  • UnitId: 12 (Unit A101) ◄──────────────┼── FROM MARY'S TENANT RECORD
│  • LandlordAccountId: 3                  │
│  • PaybillAccountNumber: "A101"          │
│  • Status: PENDING                       │
│                                          │
└────────────┬─────────────────────────────┘
             │
             │ 5. Notification sent to landlord
             │
             ▼
┌──────────────────────────────────────────┐
│  LANDLORD CONFIRMS                       │
│                                          │
│  Checks M-Pesa statement:                │
│  ✓ Account A101 → Matches system         │
│  ✓ Amount 15,000 → Matches               │
│  ✓ Ref SKG8N9Q2RT → Matches              │
│                                          │
│  PUT /api/payments/{id}/confirm          │
│  Status: COMPLETED ✓                     │
│                                          │
└──────────────────────────────────────────┘
```

### ✅ Why This Works:
- **Unit Identification**: Account number (A101) = Unit number → Automatic tracking
- **Right Landlord**: Payment goes to landlord's Paybill (123456) → John's M-Pesa account
- **Right Tenant**: System knows Mary lives in A101 → Auto-fills tenant info
- **Verification**: Landlord can cross-check M-Pesa statement with system records

---

## Scenario 2: Bank Account (Manual Identification Required)

### Setup Phase
```
┌─────────────────────────────────────────────────────────────┐
│ SYSTEM SETUP                                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Landlord: Jane Muthoni                                     │
│  ├─ Has Bank Account: KCB Bank                             │
│  │  • Account Number: 1234567890                           │
│  │  • Account Name: Jane Muthoni                           │
│  └─ Owns Property: Westlands Villas                         │
│                                                             │
│  Property: Westlands Villas                                 │
│  ├─ Unit V101 → Payment Ref Code: "WV-V101"                │
│  ├─ Unit V102 → Payment Ref Code: "WV-V102"                │
│  └─ Unit V201 → Payment Ref Code: "WV-V201"                │
│                                                             │
│  Tenant: Peter Ochieng                                      │
│  └─ Lives in Unit V101                                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Payment Flow - How It Works

```
┌────────────────┐
│  TENANT PETER  │
│  (Unit V101)   │
└────────┬───────┘
         │
         │ 1. Views Payment Instructions
         │    GET /api/tenants/payment-instructions
         │
         ├─ Returns:
         │  • Bank: KCB Bank
         │  • Account Number: 1234567890
         │  • Account Name: Jane Muthoni
         │  • Reference: WV-V101 ◄───────── MUST USE THIS!
         │  • Amount: KSh 25,000
         │
         ▼
┌────────────────────────────────────┐
│  BANK TRANSFER                     │
│                                    │
│  • To Account: 1234567890          │
│  • Amount: 25,000                  │
│  • Reference/Narration: WV-V101 ◄──┼── CRITICAL! Identifies unit
│  • From: Peter Ochieng             │
│                                    │
└────────────┬───────────────────────┘
             │
             │ 2. Bank processes transfer
             │
             ▼
┌──────────────────────────────────────────┐
│  LANDLORD JANE'S BANK STATEMENT          │
│                                          │
│  Date: 03/12/2025                        │
│  Credit: KSh 25,000                      │
│  From: Peter Ochieng                     │
│  Reference: WV-V101 ◄─────────────────────┼── JANE SEES THE UNIT CODE!
│  Balance: KSh 550,000                    │
│                                          │
└────────────┬─────────────────────────────┘
             │
             │ 3. Tenant records payment + uploads bank receipt
             │    POST /api/payments/tenant/record
             │    {
             │      "transactionRef": "WV-V101",
             │      "amount": 25000,
             │      "paymentProofUrl": "receipt.pdf"
             │    }
             │
             ▼
┌──────────────────────────────────────────┐
│  SYSTEM AUTO-FILLS:                      │
│                                          │
│  • TenantId: 8 (Peter)                   │
│  • UnitId: 23 (Unit V101) ◄──────────────┼── FROM PETER'S RECORD
│  • LandlordAccountId: 5 (Jane's bank)    │
│  • TransactionRef: "WV-V101"             │
│  • Status: PENDING                       │
│                                          │
└────────────┬─────────────────────────────┘
             │
             │ 4. Notification + receipt sent to landlord
             │
             ▼
┌──────────────────────────────────────────┐
│  LANDLORD CONFIRMS                       │
│                                          │
│  Checks bank statement:                  │
│  ✓ Reference WV-V101 → Unit V101         │
│  ✓ Amount 25,000 → Matches               │
│  ✓ Receipt uploaded → Verified           │
│                                          │
│  PUT /api/payments/{id}/confirm          │
│  Status: COMPLETED ✓                     │
│                                          │
└──────────────────────────────────────────┘
```

### ⚠️ Why This Needs Manual Step:
- **No Automatic Account Numbers**: Banks don't support multiple "account numbers" like M-Pesa Paybill
- **Reference Field**: Tenant MUST include unit code in transfer reference
- **Receipt Upload**: Landlord needs proof to verify payment
- **Manual Confirmation**: Landlord must check bank statement and confirm

---

## Comparison: M-Pesa Paybill vs Bank Account

| Feature | M-Pesa Paybill | Bank Account |
|---------|----------------|--------------|
| **Unit Identification** | Automatic (Account Number = Unit) | Manual (Reference field) |
| **Payment to Right Landlord** | ✅ Paybill number unique to landlord | ✅ Bank account unique to landlord |
| **Tenant Experience** | Easy - Just use unit as account number | Harder - Must remember to add reference |
| **Landlord Verification** | ✅ M-Pesa statement shows account number | ⚠️ Must check reference in bank statement |
| **Proof Required** | Optional (M-Pesa confirmation) | Required (Upload receipt) |
| **Future Automation** | ✅ Can use M-Pesa webhooks | ❌ No bank webhooks in Kenya |
| **Recommended For** | All landlords with Paybill | Small landlords without Paybill |

---

## How System Ensures Right Landlord & Right Unit

### 1️⃣ Right Landlord Protection
```
Property → LandlordPaymentAccount → Landlord
   ↓              ↓
  Unit ──────> Payment
   ↓
 Tenant

When tenant records payment:
✓ System gets Unit from Tenant record
✓ System gets Property from Unit
✓ System gets LandlordPaymentAccount from Property
✓ Payment MUST be made to that specific account
✓ Different landlords = Different accounts = No mix-up!
```

### 2️⃣ Right Unit Protection
```
M-Pesa Paybill Method:
Tenant → Unit A101 → PaymentAccountNumber: "A101"
         When paying: Use Account "A101"
         Landlord sees: "Payment for A101"
         ✓ Automatic match!

Bank Account Method:
Tenant → Unit V101 → ReferenceCode: "WV-V101"
         When paying: Add Reference "WV-V101"
         Landlord sees: "Reference: WV-V101"
         ✓ Manual match (landlord confirms)
```

### 3️⃣ Database Relationships
```sql
Payment Table:
├─ TenantId (FK) → Ensures we know WHO paid
├─ UnitId (FK) → Ensures we know WHICH UNIT
├─ LandlordAccountId (FK) → Ensures WHICH LANDLORD ACCOUNT
└─ PaybillAccountNumber or Reference → Ensures VERIFICATION

Example Record:
{
  "paymentId": 145,
  "tenantId": 5,              ← Mary Wanjiru
  "unitId": 12,               ← Unit A101
  "landlordAccountId": 3,     ← John's Paybill 123456
  "amount": 15000,
  "paybillAccountNumber": "A101",
  "transactionRef": "SKG8N9Q2RT",
  "status": "Completed"
}

This ensures:
✓ Payment from Mary (tenantId: 5)
✓ For Unit A101 (unitId: 12)
✓ To John's account (landlordAccountId: 3)
✓ Verified by account number match
```

---

## 🎯 Summary: How Payments Are Protected

### M-Pesa Paybill (Recommended):
1. **Setup**: Each unit gets unique account number under landlord's Paybill
2. **Payment**: Tenant pays to Paybill + uses unit number as account
3. **Identification**: Account number automatically identifies unit
4. **Verification**: Landlord's M-Pesa statement shows exact account (unit)
5. **Confirmation**: Landlord confirms in system → Payment complete

### Bank Account (Fallback):
1. **Setup**: Each unit gets unique reference code
2. **Payment**: Tenant transfers to landlord's bank + adds reference code
3. **Identification**: Reference in transfer identifies unit
4. **Verification**: Landlord checks bank statement for reference
5. **Confirmation**: Landlord confirms + verifies receipt → Payment complete

### Why It Can't Go Wrong:
- ✅ **Tenant can only pay for their own unit** (system knows their unit)
- ✅ **Payment must go to correct landlord** (property → landlord account mapping)
- ✅ **Unit is automatically identified** (M-Pesa) or manually verified (Bank)
- ✅ **Landlord confirms before completion** (final safety check)
- ✅ **Full audit trail** (all payments logged with tenant, unit, landlord)

---

## Next Steps:
1. Implement this in database (entities + relationships)
2. Create payment instructions endpoint
3. Build tenant payment recording
4. Build landlord confirmation interface
5. Test with sample data
