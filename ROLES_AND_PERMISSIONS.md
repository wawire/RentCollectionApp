# User Roles & Permissions - Kenyan Rent Collection System

## Overview
This document defines the user roles and permissions appropriate for a Kenyan property management and rent collection system.

---

## User Roles

### 1. Landlord (Property Owner)
**Description**: Property owner with full access to their portfolio

**Typical User**: Individual or company that owns rental properties

**Permissions**:
- ✅ Full CRUD on properties and units
- ✅ View all tenants and lease agreements
- ✅ View all payments and financial reports
- ✅ Manage caretakers and assign properties
- ✅ Set rent prices and payment policies
- ✅ Configure late fee rules
- ✅ View all SMS/Email logs
- ✅ Access dashboard analytics
- ✅ Generate all reports (monthly, property, tenant, financial)
- ✅ Approve major decisions (lease terminations, refunds)
- ❌ Cannot access other landlords' data (in multi-tenant setup)

**Priority**: HIGH - Core user role

---

### 2. Caretaker (Property Manager)
**Description**: On-site manager handling day-to-day operations

**Typical User**: Person hired by landlord to manage properties, collect rent, handle tenant issues

**Permissions**:
- ✅ Add/edit/view tenants assigned to their properties
- ✅ Record rent payments (Cash, M-Pesa, Bank Transfer)
- ✅ Send rent reminder SMS to tenants
- ✅ Send payment receipts via SMS
- ✅ Handle maintenance requests
- ✅ View assigned properties and units
- ✅ Generate tenant reports
- ✅ View payment history for assigned properties
- ❌ Cannot delete properties
- ❌ Cannot modify rent prices (requires landlord approval)
- ❌ Cannot access financial analytics
- ❌ Cannot manage other caretakers
- ❌ Cannot delete payment records

**Priority**: HIGH - Essential for daily operations

---

### 3. Accountant/Bookkeeper
**Description**: Financial officer managing books and reconciliation

**Typical User**: Accountant or bookkeeper tracking financial records

**Permissions**:
- ✅ View all payment records
- ✅ View and download financial reports
- ✅ Track arrears and outstanding balances
- ✅ Generate tax reports
- ✅ Reconcile M-Pesa transactions
- ✅ Export financial data (CSV, Excel, PDF)
- ✅ View collection rates and revenue analytics
- ✅ Generate profit & loss statements
- ❌ Cannot add/edit properties or units
- ❌ Cannot add/edit tenants
- ❌ Cannot send SMS notifications
- ❌ Cannot record new payments (view only)
- ❌ Cannot delete payment records

**Priority**: MEDIUM - Important for larger portfolios

---

### 4. Agent (Property Agent)
**Description**: Agent helping landlord find tenants for vacant units

**Typical User**: Real estate agent working on commission

**Permissions**:
- ✅ View vacant units assigned to them
- ✅ Add tenant applications
- ✅ View their commission reports
- ✅ Update unit marketing details
- ❌ Cannot collect rent payments
- ❌ Cannot manage existing tenants
- ❌ Cannot access financial reports
- ❌ Cannot send SMS to tenants
- ❌ Cannot view occupied units' financial details

**Priority**: LOW - Optional feature for future

---

### 5. Tenant (Self-Service Portal)
**Description**: Tenant accessing their own portal

**Typical User**: Current tenant renting a unit

**Permissions**:
- ✅ View own lease agreement details
- ✅ View payment history
- ✅ Make M-Pesa rent payments
- ✅ Download payment receipts
- ✅ Submit maintenance requests
- ✅ View maintenance request status
- ✅ Update contact information (phone, email)
- ✅ Receive SMS/Email notifications
- ❌ Cannot view other tenants' information
- ❌ Cannot modify rent amount
- ❌ Cannot access landlord/property details
- ❌ Cannot delete payment records

**Priority**: MEDIUM - Phase 8.3 (Future Enhancement)

---

### 6. System Admin (Super Administrator)
**Description**: Technical administrator managing the entire system

**Typical User**: IT staff or system developer

**Permissions**:
- ✅ Full access to all system features
- ✅ Manage all users and roles
- ✅ Configure system settings
- ✅ Manage M-Pesa integration settings
- ✅ Configure SMS provider (Africa's Talking)
- ✅ View audit logs and system logs
- ✅ Access all organizations (multi-tenant setup)
- ✅ Database backup and restore
- ✅ Manage email templates
- ✅ Configure late fee policies (global)

**Priority**: HIGH - Required for system management

---

## Role Hierarchy

```
System Admin (Super)
    ├── Landlord
    │   ├── Caretaker
    │   ├── Accountant
    │   └── Agent
    └── Tenant (Separate Portal)
```

---

## Implementation in Code

### Roles Enum
```csharp
public static class UserRoles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string Landlord = "Landlord";
    public const string Caretaker = "Caretaker";
    public const string Accountant = "Accountant";
    public const string Agent = "Agent";
    public const string Tenant = "Tenant";
}
```

### Controller Authorization Examples

**Properties Controller**:
```csharp
[Authorize(Roles = "SystemAdmin,Landlord,Caretaker")] // View properties
[HttpGet]
public async Task<IActionResult> GetProperties() { }

[Authorize(Roles = "SystemAdmin,Landlord")] // Create property
[HttpPost]
public async Task<IActionResult> CreateProperty() { }

[Authorize(Roles = "SystemAdmin,Landlord")] // Delete property
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteProperty(int id) { }
```

**Payments Controller**:
```csharp
[Authorize(Roles = "SystemAdmin,Landlord,Caretaker")] // Record payment
[HttpPost]
public async Task<IActionResult> RecordPayment() { }

[Authorize(Roles = "SystemAdmin,Landlord,Accountant,Caretaker")] // View payments
[HttpGet]
public async Task<IActionResult> GetPayments() { }

[Authorize(Roles = "SystemAdmin,Landlord")] // Delete payment
[HttpDelete("{id}")]
public async Task<IActionResult> DeletePayment(int id) { }
```

**Dashboard Controller**:
```csharp
[Authorize(Roles = "SystemAdmin,Landlord,Accountant")] // Financial analytics
[HttpGet("stats")]
public async Task<IActionResult> GetDashboardStats() { }

[Authorize(Roles = "SystemAdmin,Landlord,Caretaker")] // Basic stats
[HttpGet("summary")]
public async Task<IActionResult> GetSummary() { }
```

---

## Kenyan Context Considerations

### Payment Methods
- **M-Pesa** - Primary payment method in Kenya
- **Cash** - Common for caretaker collections
- **Bank Transfer** - For corporate tenants
- **Cheque** - Rare but supported

### Typical Workflows

**Landlord Workflow**:
1. Adds properties and units
2. Assigns caretaker to manage property
3. Reviews financial reports monthly
4. Approves major decisions

**Caretaker Workflow**:
1. Adds new tenants when units become vacant
2. Sends rent reminders via SMS before due date
3. Records payments received (cash or M-Pesa confirmation)
4. Sends SMS receipts to tenants
5. Reports to landlord

**Accountant Workflow**:
1. Reconciles M-Pesa payments monthly
2. Generates financial reports
3. Tracks arrears
4. Prepares tax documents

**Tenant Workflow** (Portal):
1. Logs in to tenant portal
2. Initiates M-Pesa payment
3. Downloads receipt
4. Submits maintenance request if needed

---

## Recommended Default Users (Seed Data)

### Development/Demo Environment
```
1. System Admin
   Email: admin@rentpro.co.ke
   Password: Admin@123
   Role: SystemAdmin

2. Landlord (Demo)
   Email: landlord@example.com
   Password: Landlord@123
   Role: Landlord

3. Caretaker (Demo)
   Email: caretaker@example.com
   Password: Caretaker@123
   Role: Caretaker

4. Accountant (Demo)
   Email: accountant@example.com
   Password: Accountant@123
   Role: Accountant
```

---

## Future Enhancements

### Multi-Landlord Support (SaaS)
- Each landlord has isolated data
- Each landlord can create their own caretakers/accountants
- System admin can access all landlords' data

### Organization/Company Support
- Support for property management companies
- Multiple landlords under one organization
- Organization-level settings and branding

### Advanced Permissions
- Custom roles (create your own permissions)
- Property-specific permissions (caretaker manages only assigned properties)
- Time-bound access (temporary accountant access during audit)

---

## Migration from Current Roles

If you have existing users with old roles:

| Old Role | New Role | Action |
|----------|----------|--------|
| Admin | Landlord or SystemAdmin | Map to SystemAdmin for full access, Landlord for property owner |
| PropertyManager | Caretaker | Direct mapping |
| Viewer | Accountant | Map to Accountant if they need financial access |

---

## Security Best Practices

1. **Principle of Least Privilege**: Give users minimum permissions needed
2. **Role-Based Access Control (RBAC)**: Use roles, not individual permissions
3. **Audit Logging**: Log all actions, especially financial transactions
4. **Multi-Factor Authentication**: Consider for Landlord and SystemAdmin roles
5. **Session Management**: Auto-logout after inactivity
6. **Data Isolation**: Ensure landlords can only see their own data

---

## Questions to Consider

1. **Should caretakers see financial analytics?**
   - Recommendation: No, to protect landlord's financial privacy

2. **Should accountants record payments?**
   - Recommendation: No, only view for reconciliation. Caretakers record payments.

3. **Can a user have multiple roles?**
   - Recommendation: Yes, small landlords may be their own caretaker

4. **Should there be property-specific permissions?**
   - Recommendation: Yes, for Phase 8.11 (Multi-Tenancy)

---

**Last Updated**: 2025-01-XX
**Status**: Draft - Ready for Implementation
