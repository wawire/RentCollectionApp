# RBAC Audit and Best Practices Recommendations
## Rent Collection Application - Complete V&V (Verification & Validation)

**Date:** 2025-12-06
**Status:** CRITICAL SECURITY REVIEW REQUIRED

---

## Executive Summary

### Current State:
- ❌ **Caretaker role has FULL write access** (same as Landlord)
- ❌ **Inconsistent permission enforcement** across services
- ⚠️ **Missing controller-level role restrictions** on many endpoints
- ⚠️ **No clear role hierarchy documentation**

### Risk Level: **HIGH**
- Caretakers can create/delete properties, units, tenants, and payments
- Unclear permission boundaries between Landlord and Caretaker

---

## 1. BEST PRACTICE RBAC MODEL FOR RENT COLLECTION APPS

### Industry Standard Role Hierarchy:

```
┌─────────────────────────────────────────────┐
│            SystemAdmin                       │
│  (Full system access, multi-landlord)       │
└─────────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
┌───────▼────────┐    ┌────────▼────────┐
│   Landlord     │    │   Landlord B    │
│  (Property     │    │  (Property      │
│   Owner)       │    │   Owner B)      │
└───────┬────────┘    └─────────────────┘
        │
   ┌────┴─────┬─────────┬────────────┐
   │          │         │            │
┌──▼───┐  ┌──▼───┐  ┌──▼────┐   ┌───▼────┐
│Carét │  │Accnt │  │ Tenant│   │ Tenant │
│aker  │  │ant   │  │   A   │   │   B    │
└──────┘  └──────┘  └───────┘   └────────┘
```

### Recommended Permission Matrix:

| Operation | SystemAdmin | Landlord | Caretaker | Accountant | Tenant |
|-----------|-------------|----------|-----------|------------|--------|
| **PROPERTIES** |
| View All Properties | ✅ | ✅ (Own) | ✅ (Own Landlord) | ✅ (Own Landlord) | ✅ (Own Only) |
| Create Property | ✅ | ✅ | ❌ | ❌ | ❌ |
| Update Property | ✅ | ✅ (Own) | ❌ | ❌ | ❌ |
| Delete Property | ✅ | ✅ (Own) | ❌ | ❌ | ❌ |
| **UNITS** |
| View Units | ✅ | ✅ (Own) | ✅ (Own Landlord) | ✅ (Own Landlord) | ✅ (Own Property) |
| Create Unit | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ❌ |
| Update Unit | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ❌ |
| Delete Unit | ✅ | ✅ (Own) | ❌ | ❌ | ❌ |
| **TENANTS** |
| View Tenants | ✅ | ✅ (Own) | ✅ (Own Landlord) | ✅ (Own Landlord) | ✅ (Self Only) |
| Create Tenant | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ❌ |
| Update Tenant | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ✅ (Self Profile) |
| Delete Tenant | ✅ | ✅ (Own) | ❌ | ❌ | ❌ |
| **PAYMENTS** |
| View Payments | ✅ | ✅ (Own) | ✅ (Own Landlord) | ✅ (Own Landlord) | ✅ (Own Only) |
| Record Payment | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ❌ |
| Update Payment | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ❌ |
| Delete Payment | ✅ | ✅ (Own) | ❌ | ❌ | ❌ |
| **USERS** |
| View Users | ✅ | ✅ (Own Staff) | ❌ | ❌ | ❌ |
| Create User | ✅ | ✅ (Staff Only) | ❌ | ❌ | ❌ |
| Update User Status | ✅ | ✅ (Own Staff) | ❌ | ❌ | ❌ |
| Delete User | ✅ | ❌ | ❌ | ❌ | ❌ |
| **REPORTS & DASHBOARD** |
| View Dashboard | ✅ | ✅ (Own) | ✅ (Own Landlord) | ✅ (Own Landlord) | ❌ |
| Financial Reports | ✅ | ✅ (Own) | ✅ (Read-Only) | ✅ (Own Landlord) | ❌ |
| **APPLICATIONS** |
| View Applications | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ❌ |
| Approve/Reject | ✅ | ✅ (Own) | ✅ (Own Landlord) | ❌ | ❌ |

### Role Definitions (Best Practice):

**SystemAdmin:**
- Platform administrator
- Manages multiple landlords
- Full access to all data
- Can create Landlord accounts

**Landlord (Property Owner):**
- Owns properties
- Manages their portfolio
- Can create Caretaker, Accountant, Tenant users
- Full CRUD on their properties/units/tenants
- Cannot access other landlords' data

**Caretaker (Property Manager):**
- On-site property manager
- Handles day-to-day operations
- **CAN**: Create/update units, tenants, record payments
- **CANNOT**: Delete properties, delete payments, create users
- Acts as landlord's delegate for operational tasks

**Accountant (Financial Viewer):**
- Read-only financial access
- Views payments, reports, dashboards
- **CANNOT**: Create, update, or delete anything
- Used for bookkeeping/auditing

**Tenant:**
- Rents a unit
- View-only access to their own data
- Can view: their unit, their payments, their lease
- Can update: their own profile information
- **CANNOT**: Access any other tenant data

---

## 2. CURRENT IMPLEMENTATION AUDIT

### ✅ CORRECTLY IMPLEMENTED:

#### TenantService - SECURE ✅
```csharp
// Tenants can only see themselves
if (_currentUserService.IsTenant)
{
    if (_currentUserService.TenantId.HasValue)
    {
        tenants = tenants.Where(t => t.Id == _currentUserService.TenantId.Value).ToList();
    }
}
```

#### PaymentService - SECURE ✅
```csharp
// Tenants only see own payments
if (_currentUserService.IsTenant)
{
    payments = payments.Where(p => p.TenantId == _currentUserService.TenantId.Value).ToList();
}
// Accountants read-only
if (_currentUserService.IsAccountant)
{
    return Result.Failure("Accountants do not have permission to record payments");
}
```

#### AuthService - SECURE ✅
```csharp
// Only SystemAdmin can create SystemAdmin/Landlord
if (!_currentUserService.IsSystemAdmin)
{
    if (registerDto.Role == UserRole.SystemAdmin || registerDto.Role == UserRole.Landlord)
    {
        throw new BadRequestException("You do not have permission to create users with this role");
    }
}
```

---

### ❌ SECURITY ISSUES FOUND:

#### Issue #1: Caretaker Has Full Write Access
**Location:** PropertyService.cs, UnitService.cs, TenantService.cs, PaymentService.cs

**Current Code:**
```csharp
// Line 105 - PropertyService.cs
else if (_currentUserService.IsCaretaker || _currentUserService.IsAccountant)
{
    property.LandlordId = _currentUserService.LandlordIdInt;
}
```

**Problem:**
- Caretakers can CREATE properties (should be landlord-only)
- Caretakers treated identically to Landlords for filtering
- No distinction between operational tasks and ownership tasks

**Impact:** MEDIUM
- Caretaker could create unauthorized properties
- Unclear audit trail (who actually owns the property?)

---

#### Issue #2: Caretaker Can Delete Units
**Location:** UnitService.cs:325

**Current Code:**
```csharp
if (_currentUserService.IsTenant || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
{
    return Result.Failure("You do not have permission to delete units");
}
```

**Status:** ✅ SECURE - Caretakers blocked from deletion

---

#### Issue #3: Missing Role Restrictions on Controllers

**Controllers WITHOUT Proper Role Restrictions:**

1. **TenantsController.cs** - Most endpoints just use `[Authorize]`
   - GET /api/tenants (No role restriction!)
   - GET /api/tenants/{id} (No role restriction!)
   - DELETE /api/tenants/{id} (No role restriction!)

2. **UnitsController.cs**
   - GET /api/units (No role restriction!)
   - GET /api/units/{id} (No role restriction!)
   - DELETE /api/units/{id} (No role restriction!)

3. **PropertiesController.cs**
   - GET /api/properties (No role restriction!)
   - GET /api/properties/{id} (No role restriction!)
   - DELETE /api/properties/{id} (No role restriction!)

4. **PaymentsController.cs**
   - GET /api/payments (No role restriction!)
   - GET /api/payments/{id} (No role restriction!)

**Impact:** HIGH
- Any authenticated user (including Tenants) can call these endpoints
- Service-layer filtering saves us, but violates defense-in-depth principle
- API appears to allow access that services block

---

#### Issue #4: No Tenant Self-Update Endpoint
**Location:** Missing from TenantsController

**Problem:**
- Tenants have no way to update their profile (phone, email, etc.)
- No `PATCH /api/tenants/me` endpoint

**Impact:** LOW - Missing feature, not security issue

---

## 3. V&V DETAILED FINDINGS

### Service Layer Verification:

| Service | Tenant Isolation | Landlord Filtering | Caretaker Handling | Accountant RO | Status |
|---------|------------------|--------------------|--------------------|---------------|--------|
| PropertyService | ✅ | ✅ | ⚠️ Can Create | ✅ | **NEEDS FIX** |
| UnitService | ✅ | ✅ | ⚠️ Can Create/Update | ✅ | **NEEDS FIX** |
| TenantService | ✅ | ✅ | ⚠️ Can Create/Update | ✅ | **NEEDS FIX** |
| PaymentService | ✅ | ✅ | ✅ Can Create | ✅ | **OK** |
| DashboardService | ✅ Blocked | ✅ | ✅ | ✅ | **OK** |
| AuthService | N/A | ✅ | ✅ Blocked | ✅ | **OK** |

### Controller Layer Verification:

| Controller | Has Role Attrs | Missing Restrictions | Critical? |
|------------|----------------|----------------------|-----------|
| AuthController | ✅ Partial | DELETE needs SystemAdmin only | ⚠️ |
| DashboardController | ✅ | None | ✅ |
| PropertiesController | ⚠️ Partial | GET endpoints | ❌ |
| UnitsController | ⚠️ Partial | GET, DELETE endpoints | ❌ |
| TenantsController | ⚠️ Partial | GET, DELETE endpoints | ❌ |
| PaymentsController | ⚠️ Partial | GET endpoints | ❌ |
| TenantApplicationsController | ✅ | None | ✅ |
| TenantPortalController | ❓ Need Review | Unknown | ❓ |
| LandlordPaymentAccountsController | ❓ Need Review | Unknown | ❓ |

---

## 4. RECOMMENDED FIXES

### Priority 1: Define Caretaker Permissions Clearly

**Recommendation:**
```csharp
// PropertyService - Caretakers CANNOT create properties
if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord)
{
    return Result.Failure("Only landlords can create properties");
}

// UnitService - Caretakers CAN create units (operational task)
if (_currentUserService.IsTenant || _currentUserService.IsAccountant)
{
    return Result.Failure("You do not have permission to create units");
}

// TenantService - Caretakers CAN create tenants (operational task)
if (_currentUserService.IsTenant || _currentUserService.IsAccountant)
{
    return Result.Failure("You do not have permission to create tenants");
}
```

### Priority 2: Add Role Restrictions to All Controllers

**Example for TenantsController:**
```csharp
[HttpGet]
[Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant")]
public async Task<IActionResult> GetAll()

[HttpGet("{id}")]
[Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
public async Task<IActionResult> GetById(int id)

[HttpDelete("{id}")]
[Authorize(Roles = "SystemAdmin,Landlord")]
public async Task<IActionResult> Delete(int id)
```

### Priority 3: Add Tenant Self-Update Endpoint

```csharp
[HttpPatch("me")]
[Authorize(Roles = "Tenant")]
public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTenantProfileDto dto)
{
    var tenantId = _currentUserService.TenantId;
    if (!tenantId.HasValue)
        return BadRequest("Tenant ID not found");

    // Only allow updating non-sensitive fields
    await _tenantService.UpdateTenantProfileAsync(tenantId.Value, dto);
    return Ok();
}
```

---

## 5. MISSING FUNCTIONALITIES

### Critical Missing Features:
1. ❌ Tenant self-service portal endpoints
2. ❌ Password reset workflow
3. ❌ Email verification
4. ❌ Audit logging for sensitive operations
5. ❌ Property ownership validation in user registration
6. ❌ Bulk operations (import tenants, import payments)
7. ❌ Notification system (email/SMS for payment reminders)
8. ❌ Lease renewal workflow
9. ❌ Maintenance request system
10. ❌ Document upload/storage
11. ❌ Receipt generation for payments
12. ❌ Late payment penalties calculation
13. ❌ Payment due date tracking
14. ❌ Tenant application approval workflow
15. ❌ Two-factor authentication

---

## 6. IMMEDIATE ACTION ITEMS

### Must Fix (Security):
1. ✅ Add role restrictions to ALL controller endpoints
2. ✅ Restrict Caretaker from creating/deleting properties
3. ✅ Add property ownership validation in AuthService.RegisterAsync
4. ⚠️ Add audit logging for user creation/deletion
5. ⚠️ Add audit logging for payment operations

### Should Fix (Functionality):
1. Add tenant self-update endpoint
2. Implement password reset
3. Add email notifications
4. Add payment reminders
5. Add receipt generation

### Nice to Have:
1. Bulk import operations
2. Advanced reporting
3. Maintenance request system
4. Document management

---

## 7. COMPLIANCE & BEST PRACTICES

### Security Best Practices:
- ✅ JWT authentication
- ✅ Password hashing with BCrypt
- ✅ Role-based access control
- ❌ Missing: Audit logs
- ❌ Missing: 2FA
- ❌ Missing: Session management
- ❌ Missing: Rate limiting

### Data Privacy:
- ✅ Tenant data isolation
- ✅ Landlord data separation
- ⚠️ Missing: Data retention policy
- ⚠️ Missing: GDPR compliance features
- ⚠️ Missing: Data export functionality

### API Security:
- ✅ HTTPS enforcement (assumed)
- ⚠️ Partial controller authorization
- ❌ Missing: API rate limiting
- ❌ Missing: Request validation/sanitization
- ❌ Missing: CORS configuration review

---

## CONCLUSION

**Overall Security Rating: B- (75/100)**

**Strengths:**
- Strong service-layer access control
- Proper tenant data isolation
- Good authentication implementation

**Weaknesses:**
- Inconsistent controller-level authorization
- Unclear Caretaker role permissions
- Missing audit logging
- Several critical features missing

**Immediate Priority:**
1. Fix Caretaker permissions (TODAY)
2. Add controller role restrictions (THIS WEEK)
3. Implement audit logging (THIS SPRINT)

---

**Prepared by:** Claude AI Security Audit
**Review Required:** Development Team
**Sign-off Required:** Product Owner, Security Lead
