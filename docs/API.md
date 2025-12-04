# API Reference

Base URL: `http://localhost:5000/api`

All endpoints require JWT Bearer token authentication (except `/Auth/login` and `/Auth/register`):
```
Authorization: Bearer {jwt_token}
```

## Authentication

### Login
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "landlord@example.com",
  "password": "Landlord@123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "landlord@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Landlord"
  }
}
```

### Register User
```http
POST /api/Auth/register
Authorization: Bearer {admin_or_landlord_token}

{
  "email": "caretaker@property.com",
  "password": "Secure@123",
  "firstName": "Jane",
  "lastName": "Smith",
  "phoneNumber": "0722123456",
  "role": "Caretaker"
}
```

**Allowed roles:** Caretaker, Accountant, Tenant

---

## Tenant Payment APIs

### Get Payment Instructions
```http
GET /api/TenantPayments/instructions
Authorization: Bearer {tenant_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "tenantName": "Peter Mwangi",
    "unitNumber": "A101",
    "propertyName": "Sunrise Apartments",
    "monthlyRent": 15000,
    "landlordName": "John Kamau",
    "accountType": "MPesaPaybill",
    "paybillNumber": "123456",
    "accountNumber": "A101",
    "paymentInstructions": "Pay to Paybill 123456, use your unit number A101 as account number"
  }
}
```

### Record Payment
```http
POST /api/TenantPayments/record
Authorization: Bearer {tenant_token}

{
  "amount": 15000,
  "paymentDate": "2025-12-03T10:30:00Z",
  "paymentMethod": "MPesa",
  "transactionReference": "SKG8N9Q2RT",
  "mPesaPhoneNumber": "0712345678",
  "notes": "December 2025 rent",
  "periodStart": "2025-12-01T00:00:00Z",
  "periodEnd": "2025-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": 145,
    "amount": 15000,
    "status": "Pending",
    "transactionReference": "SKG8N9Q2RT"
  },
  "message": "Payment recorded successfully. Awaiting landlord confirmation."
}
```

### Get Payment History
```http
GET /api/TenantPayments/history
Authorization: Bearer {tenant_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 145,
      "amount": 15000,
      "paymentDate": "2025-12-03T10:30:00Z",
      "status": "Pending",
      "transactionReference": "SKG8N9Q2RT"
    },
    {
      "id": 144,
      "amount": 15000,
      "paymentDate": "2025-11-02T09:15:00Z",
      "status": "Completed",
      "confirmedAt": "2025-11-02T14:20:00Z"
    }
  ]
}
```

### Initiate M-Pesa STK Push
```http
POST /api/TenantPayments/stk-push
Authorization: Bearer {tenant_token}

{
  "phoneNumber": "0712345678",
  "amount": 15000,
  "periodStart": "2025-12-01T00:00:00Z",
  "periodEnd": "2025-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "checkoutRequestID": "ws_CO_191220191020363925",
    "responseCode": "0",
    "responseDescription": "Success. Request accepted for processing"
  },
  "message": "STK Push sent. Check your phone and enter M-Pesa PIN."
}
```

---

## Landlord Payment Account APIs

### Get All Payment Accounts
```http
GET /api/LandlordPaymentAccounts
Authorization: Bearer {landlord_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 3,
      "accountName": "Sunrise Paybill",
      "accountType": "MPesaPaybill",
      "paybillNumber": "123456",
      "paybillName": "John Kamau Properties",
      "isDefault": true,
      "isActive": true
    }
  ]
}
```

### Create Payment Account
```http
POST /api/LandlordPaymentAccounts
Authorization: Bearer {landlord_token}

{
  "propertyId": 5,
  "accountName": "Sunrise Paybill",
  "accountType": "MPesaPaybill",
  "paybillNumber": "123456",
  "paybillName": "John Kamau Properties",
  "isDefault": true,
  "isActive": true,
  "paymentInstructions": "Pay to Paybill 123456, use unit number as account"
}
```

**For Bank Account:**
```json
{
  "accountName": "Equity Bank Account",
  "accountType": "BankAccount",
  "bankName": "Equity Bank",
  "bankAccountNumber": "1234567890",
  "bankAccountName": "John Kamau",
  "bankBranch": "Westlands",
  "swiftCode": "EQBLKENA",
  "isDefault": false,
  "isActive": true
}
```

### Update Payment Account
```http
PUT /api/LandlordPaymentAccounts/{id}
Authorization: Bearer {landlord_token}

{
  "accountName": "Updated Account Name",
  "isActive": true
}
```

### Delete Payment Account
```http
DELETE /api/LandlordPaymentAccounts/{id}
Authorization: Bearer {landlord_token}
```

**Response:** `204 No Content`

### Set Default Account
```http
POST /api/LandlordPaymentAccounts/{id}/set-default
Authorization: Bearer {landlord_token}
```

---

## Payment Confirmation APIs

### Get Pending Payments
```http
GET /api/Payments/pending?propertyId={propertyId}
Authorization: Bearer {landlord_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 145,
      "tenantName": "Mary Wanjiru",
      "unitNumber": "A101",
      "amount": 15000,
      "paymentDate": "2025-12-03T10:30:00Z",
      "paymentMethod": "MPesa",
      "status": "Pending",
      "transactionReference": "SKG8N9Q2RT",
      "mPesaPhoneNumber": "0712345678"
    }
  ]
}
```

### Confirm Payment
```http
PUT /api/Payments/{id}/confirm
Authorization: Bearer {landlord_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": 145,
    "status": "Completed",
    "confirmedAt": "2025-12-03T14:20:00Z"
  },
  "message": "Payment confirmed successfully"
}
```

### Reject Payment
```http
PUT /api/Payments/{id}/reject
Authorization: Bearer {landlord_token}

{
  "reason": "Transaction reference not found in M-Pesa statement"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": 145,
    "status": "Rejected"
  },
  "message": "Payment rejected"
}
```

---

## Property APIs

### Get All Properties
```http
GET /api/Properties
Authorization: Bearer {token}
```

### Create Property
```http
POST /api/Properties
Authorization: Bearer {landlord_token}

{
  "name": "Sunset Apartments",
  "address": "Westlands, Nairobi",
  "description": "Modern 2-bedroom apartments"
}
```

### Update Property
```http
PUT /api/Properties/{id}
Authorization: Bearer {landlord_token}
```

### Delete Property
```http
DELETE /api/Properties/{id}
Authorization: Bearer {landlord_token}
```

---

## Unit APIs

### Get Units by Property
```http
GET /api/Units/property/{propertyId}
Authorization: Bearer {token}
```

### Create Unit
```http
POST /api/Units
Authorization: Bearer {landlord_token}

{
  "propertyId": 1,
  "unitNumber": "A101",
  "numberOfBedrooms": 2,
  "numberOfBathrooms": 1,
  "squareFootage": 850,
  "monthlyRent": 15000,
  "isOccupied": false
}
```

### Update Unit
```http
PUT /api/Units/{id}
Authorization: Bearer {landlord_token}
```

### Delete Unit
```http
DELETE /api/Units/{id}
Authorization: Bearer {landlord_token}
```

---

## Tenant APIs

### Get All Tenants
```http
GET /api/Tenants
Authorization: Bearer {landlord_token}
```

### Get Occupied Tenants
```http
GET /api/Tenants/occupied
Authorization: Bearer {landlord_token}
```

Returns only active tenants currently occupying units.

### Get Tenant by ID
```http
GET /api/Tenants/{id}
Authorization: Bearer {landlord_token}
```

### Create Tenant
```http
POST /api/Tenants
Authorization: Bearer {landlord_token}

{
  "userId": 10,
  "unitId": 5,
  "leaseStartDate": "2025-01-01",
  "leaseEndDate": "2025-12-31",
  "monthlyRent": 15000,
  "depositAmount": 30000,
  "isActive": true
}
```

### Update Tenant
```http
PUT /api/Tenants/{id}
Authorization: Bearer {landlord_token}
```

### Delete Tenant
```http
DELETE /api/Tenants/{id}
Authorization: Bearer {landlord_token}
```

---

## Common Response Format

All API responses follow this structure:

**Success:**
```json
{
  "isSuccess": true,
  "data": { ... },
  "message": "Operation successful"
}
```

**Error:**
```json
{
  "isSuccess": false,
  "message": "Error description",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

## Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 201 | Created |
| 204 | No Content (successful deletion) |
| 400 | Bad Request (validation errors) |
| 401 | Unauthorized (missing/invalid token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 500 | Internal Server Error |

## Payment Enums

### PaymentMethod
```
MPesa = 1
BankTransfer = 2
Cash = 3
Cheque = 4
```

### PaymentStatus
```
Pending = 1
Completed = 2
Rejected = 3
Failed = 4
```

### PaymentAccountType
```
MPesaPaybill = 1
MPesaTillNumber = 2
MPesaPhone = 3
BankAccount = 4
Cash = 5
```
