# RentCollection App - Testing Guide

## 🔐 Test Credentials

### Landlords
```
Email: landlord@example.com
Password: Landlord@123
Role: Landlord
Owns: Sunset Apartments Westlands, Parklands Heights

Email: mary.wanjiku@example.com
Password: Mary@123
Role: Landlord
Owns: Kileleshwa Gardens, Lavington Court

Email: david.kamau@example.com
Password: David@123
Role: Landlord
Owns: Utawala Maisonettes, Ruiru Bungalows
```

### Tenants (Use these to test Tenant Portal)
```
Email: peter.mwangi@gmail.com
Password: Tenant@123
Unit: B1 (Bedsitter) - Sunset Apartments
Rent: KSh 12,000

Email: grace.akinyi@yahoo.com
Password: Tenant@123
Unit: 1A (One-bedroom) - Sunset Apartments
Rent: KSh 18,000

Email: alice.wambui@gmail.com
Password: Tenant@123
Unit: K-2A (Two-bedroom) - Kileleshwa Gardens
Rent: KSh 35,000

Email: mkimani@outlook.com
Password: Tenant@123
Unit: K-3A (Three-bedroom) - Kileleshwa Gardens
Rent: KSh 45,000

Email: sarah.njeri@gmail.com
Password: Tenant@123
Unit: L301 (Luxury 3BR) - Lavington Court
Rent: KSh 70,000
```

### System Admin
```
Email: admin@example.com
Password: Admin@123
Role: SystemAdmin
```

---

## 🎯 Quick Answers to Your Questions

### 1. **Can landlords create their team (caretakers, etc.)?**
**YES!** ✅

**Endpoint**: `POST /api/Auth/register`
**Authorization**: Landlord or SystemAdmin roles

**Example - Create a Caretaker**:
```bash
POST http://localhost:5000/api/Auth/register
Authorization: Bearer {landlord_jwt_token}
Content-Type: application/json

{
  "email": "caretaker@sunset.com",
  "password": "Care@123",
  "firstName": "John",
  "lastName": "Omondi",
  "phoneNumber": "0722334455",
  "role": "Caretaker"
}
```

**Available Roles**:
- Caretaker
- Accountant
- Tenant

---

### 2. **Can we see tenants for all occupied rooms?**
**YES!** ✅

**New Endpoint**: `GET /api/Tenants/occupied`
**Returns**: All active tenants currently occupying units

**Test**:
```bash
GET http://localhost:5000/api/Tenants/occupied
Authorization: Bearer {landlord_jwt_token}
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 1,
      "fullName": "Peter Mwangi",
      "email": "peter.mwangi@gmail.com",
      "phoneNumber": "+254723870917",
      "unitNumber": "B1",
      "propertyName": "Sunset Apartments Westlands",
      "monthlyRent": 12000,
      "isActive": true
    },
    // ... more occupied tenants
  ]
}
```

**Other Tenant Endpoints**:
- `GET /api/Tenants` - All tenants (occupied + vacant units that had tenants)
- `GET /api/Tenants/{id}` - Specific tenant details
- `GET /api/Tenants/unit/{unitId}` - Tenants by unit

---

## 🏠 Tenant Portal Testing

### Test Scenario 1: View Payment Instructions

**Tenant**: Peter Mwangi (peter.mwangi@gmail.com)
**Unit**: B1 - Sunset Apartments
**Rent**: KSh 12,000

1. **Login as tenant**:
```bash
POST http://localhost:5000/api/Auth/login
Content-Type: application/json

{
  "email": "peter.mwangi@gmail.com",
  "password": "Tenant@123"
}
```

2. **Get payment instructions**:
```bash
GET http://localhost:5000/api/TenantPayments/instructions
Authorization: Bearer {tenant_jwt_token}
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": {
    "tenantName": "Peter Mwangi",
    "unitNumber": "B1",
    "propertyName": "Sunset Apartments Westlands",
    "monthlyRent": 12000,
    "landlordName": "John Landlord",
    "accountType": "MPesaPaybill",
    "paybillNumber": "123456",
    "paybillName": "John Landlord Properties",
    "accountNumber": "B1",
    "paymentInstructions": "Pay to M-Pesa Paybill 123456. Use your unit number (e.g., B1, 1A) as the Account Number."
  }
}
```

---

### Test Scenario 2: Record M-Pesa Payment

**After tenant pays via M-Pesa**, they record the payment:

```bash
POST http://localhost:5000/api/TenantPayments/record
Authorization: Bearer {tenant_jwt_token}
Content-Type: application/json

{
  "amount": 12000,
  "paymentDate": "2025-12-04T10:30:00Z",
  "paymentMethod": "MPesa",
  "transactionReference": "SKG8N9Q2RT",
  "mPesaPhoneNumber": "0723870917",
  "notes": "December 2025 rent",
  "periodStart": "2025-12-01T00:00:00Z",
  "periodEnd": "2025-12-31T23:59:59Z"
}
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 10,
    "tenantId": 1,
    "unitId": 1,
    "landlordAccountId": 1,
    "amount": 12000,
    "paymentMethod": "MPesa",
    "status": "Pending",
    "transactionReference": "SKG8N9Q2RT",
    "paybillAccountNumber": "B1",
    "mPesaPhoneNumber": "0723870917"
  },
  "message": "Payment recorded successfully. Awaiting landlord confirmation."
}
```

---

### Test Scenario 3: View Payment History

```bash
GET http://localhost:5000/api/TenantPayments/history
Authorization: Bearer {tenant_jwt_token}
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 10,
      "amount": 12000,
      "paymentDate": "2025-12-04T10:30:00Z",
      "status": "Pending",
      "transactionReference": "SKG8N9Q2RT"
    },
    {
      "id": 1,
      "amount": 12000,
      "paymentDate": "2025-11-03T00:00:00Z",
      "status": "Completed",
      "confirmedAt": "2025-11-03T14:00:00Z"
    }
  ]
}
```

---

## 🏢 Landlord Dashboard Testing

### Test Scenario 4: View Pending Payments (Landlord)

**Landlord**: John Landlord (landlord@example.com)

1. **Login as landlord**:
```bash
POST http://localhost:5000/api/Auth/login
Content-Type: application/json

{
  "email": "landlord@example.com",
  "password": "Landlord@123"
}
```

2. **View pending payments**:
```bash
GET http://localhost:5000/api/Payments/pending
Authorization: Bearer {landlord_jwt_token}
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 10,
      "tenantName": "Peter Mwangi",
      "unitNumber": "B1",
      "amount": 12000,
      "paymentMethod": "MPesa",
      "status": "Pending",
      "transactionReference": "SKG8N9Q2RT",
      "mPesaPhoneNumber": "0723870917",
      "paybillAccountNumber": "B1",
      "paymentDate": "2025-12-04T10:30:00Z"
    }
  ]
}
```

---

### Test Scenario 5: Confirm Payment (Landlord)

**After checking M-Pesa statement and verifying the payment**:

```bash
PUT http://localhost:5000/api/Payments/10/confirm
Authorization: Bearer {landlord_jwt_token}
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 10,
    "status": "Completed",
    "confirmedAt": "2025-12-04T15:00:00Z",
    "confirmedByUserId": 2
  },
  "message": "Payment confirmed successfully"
}
```

---

### Test Scenario 6: Reject Payment (Landlord)

**If payment is invalid**:

```bash
PUT http://localhost:5000/api/Payments/10/reject
Authorization: Bearer {landlord_jwt_token}
Content-Type: application/json

{
  "reason": "Transaction reference not found in M-Pesa statement"
}
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 10,
    "status": "Rejected",
    "notes": "Rejected: Transaction reference not found in M-Pesa statement"
  },
  "message": "Payment rejected"
}
```

---

### Test Scenario 7: Manage Payment Accounts (Landlord)

**View all payment accounts**:
```bash
GET http://localhost:5000/api/LandlordPaymentAccounts
Authorization: Bearer {landlord_jwt_token}
```

**Create new M-Pesa Paybill account**:
```bash
POST http://localhost:5000/api/LandlordPaymentAccounts
Authorization: Bearer {landlord_jwt_token}
Content-Type: application/json

{
  "propertyId": 1,
  "accountName": "My New Paybill",
  "accountType": "MPesaPaybill",
  "paybillNumber": "654321",
  "paybillName": "My Properties",
  "isDefault": false,
  "isActive": true,
  "paymentInstructions": "Pay to Paybill 654321, Account: [UNIT_NUMBER]"
}
```

**Create Equity Bank account**:
```bash
POST http://localhost:5000/api/LandlordPaymentAccounts
Authorization: Bearer {landlord_jwt_token}
Content-Type: application/json

{
  "propertyId": 1,
  "accountName": "Equity Bank Account",
  "accountType": "BankAccount",
  "bankName": "Equity Bank",
  "bankAccountNumber": "0987654321",
  "bankAccountName": "John Landlord",
  "bankBranch": "Westlands",
  "swiftCode": "EQBLKENA",
  "isDefault": false,
  "isActive": true,
  "paymentInstructions": "Transfer to Equity 0987654321. Reference: RENT-[UNIT]"
}
```

---

## 📊 Test Data Summary

### Properties (6 Total)
1. **Sunset Apartments Westlands** (John Landlord) - 5 units, 2 occupied
2. **Parklands Heights** (John Landlord) - 3 units, 1 occupied
3. **Kileleshwa Gardens** (Mary Wanjiku) - 4 units, 2 occupied
4. **Lavington Court** (Mary Wanjiku) - 2 units, 1 occupied
5. **Utawala Maisonettes** (David Kamau) - 3 units, 1 occupied
6. **Ruiru Bungalows** (David Kamau) - 3 units, 2 occupied

### Payment Accounts (5 Total)
1. **M-Pesa Paybill 123456** - Sunset Apartments (Default)
2. **Equity Bank** - Parklands Heights
3. **M-Pesa Paybill 789012** - Kileleshwa Gardens (Default)
4. **KCB Bank** - Lavington Court (Default)
5. **M-Pesa Paybill 345678** - David Kamau (All properties, Default)

### Tenants (9 Total, all active/occupied)
- All have payment history
- All have assigned payment account numbers
- Ready for payment testing

---

## 🚀 Quick Start Testing Steps

1. **Start the API**:
```bash
cd src/RentCollection.API
dotnet run
```

2. **Login as Tenant** (Peter Mwangi):
```bash
curl -X POST http://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"peter.mwangi@gmail.com","password":"Tenant@123"}'
```

3. **Get Payment Instructions**:
```bash
curl http://localhost:5000/api/TenantPayments/instructions \
  -H "Authorization: Bearer {token_from_step_2}"
```

4. **Record a Payment**:
```bash
curl -X POST http://localhost:5000/api/TenantPayments/record \
  -H "Authorization: Bearer {token_from_step_2}" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 12000,
    "paymentDate": "2025-12-04T10:00:00Z",
    "paymentMethod": "MPesa",
    "transactionReference": "TEST123",
    "mPesaPhoneNumber": "0723870917",
    "periodStart": "2025-12-01T00:00:00Z",
    "periodEnd": "2025-12-31T23:59:59Z"
  }'
```

5. **Login as Landlord** (John Landlord):
```bash
curl -X POST http://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"landlord@example.com","password":"Landlord@123"}'
```

6. **View Pending Payments**:
```bash
curl http://localhost:5000/api/Payments/pending \
  -H "Authorization: Bearer {landlord_token}"
```

7. **Confirm the Payment**:
```bash
curl -X PUT http://localhost:5000/api/Payments/{payment_id}/confirm \
  -H "Authorization: Bearer {landlord_token}"
```

---

## ✅ What's Working

### Tenant Portal APIs ✅
- ✅ GET /api/TenantPayments/instructions
- ✅ POST /api/TenantPayments/record
- ✅ GET /api/TenantPayments/history
- ✅ POST /api/TenantPayments/stk-push (structure ready, needs M-Pesa credentials)

### Landlord Dashboard APIs ✅
- ✅ GET /api/Payments/pending
- ✅ PUT /api/Payments/{id}/confirm
- ✅ PUT /api/Payments/{id}/reject
- ✅ GET /api/LandlordPaymentAccounts
- ✅ POST /api/LandlordPaymentAccounts (Create M-Pesa/Bank accounts)
- ✅ PUT /api/LandlordPaymentAccounts/{id} (Update)
- ✅ DELETE /api/LandlordPaymentAccounts/{id}

### Team Management ✅
- ✅ POST /api/Auth/register (Landlords can create Caretakers, Accountants)

### Tenant Management ✅
- ✅ GET /api/Tenants (All tenants)
- ✅ GET /api/Tenants/occupied (Active tenants in occupied units) **NEW!**
- ✅ GET /api/Tenants/{id}
- ✅ GET /api/Tenants/unit/{unitId}

---

## 📝 Notes

1. **Database Seeding**: The database includes 9 sample tenants, 6 properties, 5 payment accounts, and 8 payment records
2. **Payment Account Numbers**: All units have unique payment account numbers (B1, 1A, K-2A, L301, M1, BG1, etc.)
3. **M-Pesa Integration**: STK Push structure is ready but needs actual Daraja API credentials
4. **Default Passwords**: All test users use password pattern `{Role}@123` (e.g., Tenant@123, Landlord@123)

---

## 🔧 Troubleshooting

**Issue**: "Payment account not configured"
**Solution**: Ensure the property has a payment account. Check `/api/LandlordPaymentAccounts`

**Issue**: "Tenant does not have an assigned unit"
**Solution**: Verify tenant has a unitId assigned. Check `/api/Tenants/{id}`

**Issue**: "401 Unauthorized"
**Solution**: Ensure JWT token is included in Authorization header: `Bearer {token}`

---

Happy Testing! 🎉
