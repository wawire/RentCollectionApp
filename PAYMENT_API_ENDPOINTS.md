# Payment System API Endpoints

This document describes all the API endpoints for the RentCollection payment system, including tenant payment recording, landlord account management, and M-Pesa integration.

---

## 🎯 Overview

The payment system provides APIs for:
- **Tenants**: View payment instructions, record payments, initiate STK Push
- **Landlords**: Manage payment accounts (M-Pesa Paybill, Bank accounts), confirm/reject payments
- **M-Pesa Integration**: STK Push for automated payment collection

---

## 📱 Tenant Payment APIs

### Base Route: `/api/TenantPayments`
**Authorization**: Tenant role required

### 1. Get Payment Instructions
```http
GET /api/TenantPayments/instructions
```

**Description**: Get payment instructions for the current tenant including landlord's M-Pesa Paybill, Bank account, etc.

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "tenantId": 5,
    "tenantName": "Mary Wanjiru",
    "unitId": 12,
    "unitNumber": "A101",
    "propertyName": "Sunrise Apartments",
    "monthlyRent": 15000,
    "landlordName": "John Kamau",
    "landlordPhone": "0722123456",
    "landlordAccountId": 3,
    "accountType": "MPesaPaybill",
    "accountName": "Sunrise Apartments Paybill",
    "paybillNumber": "123456",
    "paybillName": "John Kamau Properties",
    "accountNumber": "A101",
    "paymentInstructions": "Pay to Paybill 123456, use your unit number A101 as the account number"
  }
}
```

---

### 2. Record Payment
```http
POST /api/TenantPayments/record
Content-Type: application/json
```

**Description**: Record a payment made by the tenant (M-Pesa, Bank transfer, Cash)

**Request Body**:
```json
{
  "amount": 15000,
  "paymentDate": "2025-12-03T10:30:00Z",
  "paymentMethod": "MPesa",
  "transactionReference": "SKG8N9Q2RT",
  "mPesaPhoneNumber": "0712345678",
  "notes": "December 2025 rent",
  "periodStart": "2025-12-01T00:00:00Z",
  "periodEnd": "2025-12-31T23:59:59Z",
  "paymentProofUrl": "https://storage.example.com/receipts/payment_123.jpg"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 145,
    "tenantId": 5,
    "unitId": 12,
    "landlordAccountId": 3,
    "amount": 15000,
    "paymentDate": "2025-12-03T10:30:00Z",
    "paymentMethod": "MPesa",
    "status": "Pending",
    "transactionReference": "SKG8N9Q2RT",
    "mPesaPhoneNumber": "0712345678",
    "paybillAccountNumber": "A101",
    "notes": "December 2025 rent",
    "periodStart": "2025-12-01T00:00:00Z",
    "periodEnd": "2025-12-31T23:59:59Z",
    "paymentProofUrl": "https://storage.example.com/receipts/payment_123.jpg",
    "createdAt": "2025-12-03T10:30:00Z"
  },
  "message": "Payment recorded successfully. Awaiting landlord confirmation."
}
```

---

### 3. Get Payment History
```http
GET /api/TenantPayments/history
```

**Description**: Get all payment history for the current tenant

**Response**:
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
      "transactionReference": "RKF7M8P1QS",
      "confirmedAt": "2025-11-02T14:20:00Z"
    }
  ]
}
```

---

### 4. Initiate M-Pesa STK Push
```http
POST /api/TenantPayments/stk-push
Content-Type: application/json
```

**Description**: Initiate M-Pesa STK Push to tenant's phone for rent payment

**Request Body**:
```json
{
  "phoneNumber": "0712345678",
  "amount": 15000,
  "periodStart": "2025-12-01T00:00:00Z",
  "periodEnd": "2025-12-31T23:59:59Z",
  "notes": "December 2025 rent"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "merchantRequestID": "29115-34620561-1",
    "checkoutRequestID": "ws_CO_191220191020363925",
    "resultCode": 0,
    "resultDesc": "The service request is processed successfully.",
    "responseCode": "0",
    "responseDescription": "Success. Request accepted for processing"
  },
  "message": "STK Push sent successfully. Please check your phone and enter M-Pesa PIN to complete payment."
}
```

---

### 5. Upload Payment Proof
```http
POST /api/TenantPayments/{paymentId}/upload-proof
Content-Type: multipart/form-data
```

**Description**: Upload payment proof (M-Pesa screenshot, bank receipt, etc.)

**Request**: Multipart form with file

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 145,
    "paymentProofUrl": "https://storage.example.com/receipts/payment_145.jpg"
  },
  "message": "Payment proof uploaded successfully"
}
```

---

## 🏦 Landlord Payment Account APIs

### Base Route: `/api/LandlordPaymentAccounts`
**Authorization**: SystemAdmin, Landlord roles

### 1. Get All My Payment Accounts
```http
GET /api/LandlordPaymentAccounts
```

**Description**: Get all payment accounts for the current landlord

**Response**:
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 3,
      "landlordId": 10,
      "propertyId": 5,
      "propertyName": "Sunrise Apartments",
      "accountName": "Sunrise Paybill",
      "accountType": "MPesaPaybill",
      "paybillNumber": "123456",
      "paybillName": "John Kamau Properties",
      "isDefault": true,
      "isActive": true,
      "autoReconciliation": false,
      "createdAt": "2025-11-01T00:00:00Z"
    },
    {
      "id": 4,
      "landlordId": 10,
      "propertyId": null,
      "accountName": "Equity Bank Account",
      "accountType": "BankAccount",
      "bankName": "Equity Bank",
      "bankAccountNumber": "1234567890",
      "bankAccountName": "John Kamau",
      "bankBranch": "Westlands",
      "isDefault": false,
      "isActive": true,
      "createdAt": "2025-11-15T00:00:00Z"
    }
  ]
}
```

---

### 2. Get Payment Accounts for Property
```http
GET /api/LandlordPaymentAccounts/property/{propertyId}
```

**Description**: Get all payment accounts for a specific property

**Response**: Same format as above, filtered by property

---

### 3. Get Payment Account by ID
```http
GET /api/LandlordPaymentAccounts/{id}
```

**Description**: Get details of a specific payment account

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 3,
    "landlordId": 10,
    "propertyId": 5,
    "propertyName": "Sunrise Apartments",
    "accountName": "Sunrise Paybill",
    "accountType": "MPesaPaybill",
    "paybillNumber": "123456",
    "paybillName": "John Kamau Properties",
    "isDefault": true,
    "isActive": true,
    "autoReconciliation": false,
    "paymentInstructions": "Pay to Paybill 123456, use your unit number as account number",
    "createdAt": "2025-11-01T00:00:00Z"
  }
}
```

---

### 4. Get Default Payment Account
```http
GET /api/LandlordPaymentAccounts/default?propertyId={propertyId}
```

**Description**: Get default payment account for landlord (optionally filtered by property)

**Response**: Same format as Get by ID

---

### 5. Create Payment Account
```http
POST /api/LandlordPaymentAccounts
Content-Type: application/json
```

**Description**: Create a new payment account

**Request Body (M-Pesa Paybill)**:
```json
{
  "propertyId": 5,
  "accountName": "Sunrise Paybill",
  "accountType": "MPesaPaybill",
  "paybillNumber": "123456",
  "paybillName": "John Kamau Properties",
  "mPesaConsumerKey": "your_consumer_key",
  "mPesaConsumerSecret": "your_consumer_secret",
  "mPesaShortCode": "123456",
  "mPesaPasskey": "your_passkey",
  "isDefault": true,
  "isActive": true,
  "autoReconciliation": false,
  "paymentInstructions": "Pay to Paybill 123456, use your unit number as account number"
}
```

**Request Body (Equity Bank)**:
```json
{
  "propertyId": null,
  "accountName": "Equity Bank Account",
  "accountType": "BankAccount",
  "bankName": "Equity Bank",
  "bankAccountNumber": "1234567890",
  "bankAccountName": "John Kamau",
  "bankBranch": "Westlands",
  "swiftCode": "EQBLKENA",
  "isDefault": false,
  "isActive": true,
  "paymentInstructions": "Transfer to Equity Bank Account 1234567890, use 'RENT-[UNIT_NUMBER]' as reference"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 5,
    "accountName": "Equity Bank Account",
    "accountType": "BankAccount",
    // ... full account details
  },
  "message": "Payment account created successfully"
}
```

---

### 6. Update Payment Account
```http
PUT /api/LandlordPaymentAccounts/{id}
Content-Type: application/json
```

**Description**: Update an existing payment account

**Request Body**: Same structure as Create

**Response**: Updated account details

---

### 7. Delete Payment Account
```http
DELETE /api/LandlordPaymentAccounts/{id}
```

**Description**: Delete a payment account

**Response**:
```http
204 No Content
```

---

### 8. Set Account as Default
```http
POST /api/LandlordPaymentAccounts/{id}/set-default
```

**Description**: Set a payment account as the default for the landlord

**Response**:
```json
{
  "isSuccess": true,
  "data": true,
  "message": "Account set as default successfully"
}
```

---

## 💰 Payment Confirmation APIs (Landlord)

### Base Route: `/api/Payments`
**Authorization**: SystemAdmin, Landlord, Caretaker roles

### 1. Get Pending Payments
```http
GET /api/Payments/pending?propertyId={propertyId}
```

**Description**: Get all payments awaiting landlord confirmation

**Query Parameters**:
- `propertyId` (optional): Filter by property

**Response**:
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 145,
      "tenantId": 5,
      "tenantName": "Mary Wanjiru",
      "unitId": 12,
      "unitNumber": "A101",
      "amount": 15000,
      "paymentDate": "2025-12-03T10:30:00Z",
      "paymentMethod": "MPesa",
      "status": "Pending",
      "transactionReference": "SKG8N9Q2RT",
      "mPesaPhoneNumber": "0712345678",
      "paybillAccountNumber": "A101",
      "paymentProofUrl": "https://storage.example.com/receipts/payment_145.jpg",
      "createdAt": "2025-12-03T10:30:00Z"
    }
  ]
}
```

---

### 2. Confirm Payment
```http
PUT /api/Payments/{id}/confirm
```

**Description**: Confirm a payment after verifying M-Pesa/Bank statement

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 145,
    "status": "Completed",
    "confirmedAt": "2025-12-03T14:20:00Z",
    "confirmedByUserId": 10
  },
  "message": "Payment confirmed successfully"
}
```

---

### 3. Reject Payment
```http
PUT /api/Payments/{id}/reject
Content-Type: application/json
```

**Description**: Reject a payment with reason

**Request Body**:
```json
{
  "reason": "Transaction reference not found in M-Pesa statement"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": 145,
    "status": "Rejected",
    "notes": "Transaction reference not found in M-Pesa statement"
  },
  "message": "Payment rejected"
}
```

---

## 🔐 Authentication

All endpoints require JWT Bearer token authentication:

```http
Authorization: Bearer {jwt_token}
```

### Role-Based Access:
- **Tenant**: Can access `/api/TenantPayments/*`
- **Landlord**: Can access `/api/LandlordPaymentAccounts/*` and `/api/Payments/pending`, `/api/Payments/{id}/confirm`, `/api/Payments/{id}/reject`
- **SystemAdmin**: Can access all endpoints

---

## 📋 Payment Account Types

```csharp
public enum PaymentAccountType
{
    MPesaPaybill = 1,      // M-Pesa Paybill with account numbers
    MPesaTillNumber = 2,   // M-Pesa Till Number
    MPesaPhone = 3,        // Personal M-Pesa number
    BankAccount = 4,       // Bank account (Equity, KCB, etc.)
    Cash = 5               // Cash payments
}
```

---

## 🔄 Payment Flow Summary

### Tenant Side:
1. `GET /api/TenantPayments/instructions` → Get payment details
2. Make payment via M-Pesa/Bank
3. `POST /api/TenantPayments/record` → Record payment in system
4. `POST /api/TenantPayments/{id}/upload-proof` → Upload proof (optional)

### Landlord Side:
1. `GET /api/Payments/pending` → See pending payments
2. Check M-Pesa/Bank statement for verification
3. `PUT /api/Payments/{id}/confirm` → Confirm if match found
4. `PUT /api/Payments/{id}/reject` → Reject if no match

---

## 🚀 Next Steps for Implementation

1. **Service Layer Implementation**: Implement the service interfaces:
   - `IPaymentService` - Payment recording, confirmation, rejection
   - `ILandlordPaymentAccountService` - Payment account CRUD
   - `IMPesaService` - M-Pesa STK Push and webhook handling

2. **Database Migration**: Run migration to create:
   - `LandlordPaymentAccounts` table
   - Update `Payments` table with new fields
   - Update `Units` table with `PaymentAccountNumber`

3. **M-Pesa Integration**:
   - Register for M-Pesa Daraja API
   - Implement STK Push
   - Set up C2B webhooks for automatic payment detection

4. **File Upload**: Implement payment proof file upload (use cloud storage like AWS S3 or local file system)

5. **Notifications**: Send SMS/Email notifications:
   - To landlord when tenant records payment
   - To tenant when payment is confirmed/rejected

6. **Frontend**: Build tenant portal and landlord dashboard to consume these APIs

---

## 📞 Support

For questions or issues with the payment APIs, contact the development team.
