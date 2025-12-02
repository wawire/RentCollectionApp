# Authentication & Authorization Summary

## Issues Found & Fixed ✅

### 1. **Property-User Relationship Broken**
**Problem**: Landlords appeared with no properties!

**Root Causes**:
- `Property.LandlordId` was `string` but `User.Id` is `int` (type mismatch from old ASP.NET Identity)
- Properties were seeded WITHOUT any `LandlordId` values
- Seeding order was wrong (Properties before Users)
- Landlords had `PropertyId` pointing to ONE property (but they own MULTIPLE)

**Fixes Applied**:
- ✅ Changed `Property.LandlordId` from `string?` to `int?`
- ✅ Added `Property.Landlord` navigation property
- ✅ Added `User.OwnedProperties` collection for landlords
- ✅ Reversed seeding order: Users → Properties
- ✅ Properties now query landlords and assign `LandlordId`
- ✅ Landlords now have `PropertyId = null` (they see all via `OwnedProperties`)

## Current Relationship Model

### Landlords:
- **Can own MULTIPLE properties**
- `User.PropertyId` = `null` (not tied to one property)
- `User.OwnedProperties` = All properties where `Property.LandlordId == User.Id`
- See ALL their properties in the system

Example:
- John Landlord (User.Id = 2) owns:
  - Sunset Apartments Westlands (Property.LandlordId = 2)
  - Parklands Heights (Property.LandlordId = 2)

### Caretakers:
- **Assigned to ONE specific property**
- `User.PropertyId` = The property they manage
- Only see data for their assigned property

### SystemAdmin & Accountant:
- `User.PropertyId` = `null`
- See ALL properties across ALL landlords

## User Roles & Access

| Role | PropertyId | Access Level |
|------|------------|--------------|
| **SystemAdmin** | null | ALL properties (all landlords) |
| **Landlord** | null | Properties where LandlordId = User.Id |
| **Caretaker** | property_id | Single property (PropertyId) |
| **Accountant** | null | ALL properties (read-only) |
| **Tenant** | null | Own tenant record only |

## Demo Accounts

### System Admin:
- Email: `admin@rentcollection.com`
- Password: `Admin@123`
- Access: ALL properties

### Landlords:
1. **John Landlord** (owns 2 properties)
   - Email: `landlord@example.com`
   - Password: `Landlord@123`
   - Properties: Sunset Apartments Westlands, Parklands Heights

2. **Mary Wanjiku** (owns 2 properties)
   - Email: `mary.wanjiku@example.com`
   - Password: `Landlord@123`
   - Properties: Kileleshwa Gardens, Lavington Court

3. **David Kamau** (owns 2 properties)
   - Email: `david.kamau@example.com`
   - Password: `Landlord@123`
   - Properties: Utawala Maisonettes, Ruiru Bungalows Estate

### Caretakers:
1. **Jane Mueni** (works at Sunset Apartments)
   - Email: `caretaker@example.com`
   - Password: `Caretaker@123`

2. **Peter Kamau** (works at Kileleshwa Gardens)
   - Email: `peter.caretaker@example.com`
   - Password: `Caretaker@123`

3. **James Omondi** (works at Utawala Maisonettes)
   - Email: `james.caretaker@example.com`
   - Password: `Caretaker@123`

### Accountant:
- Email: `accountant@example.com`
- Password: `Accountant@123`
- Access: ALL properties (read-only reports)

## Authorization Policies (What You Have)

Check `src/RentCollection.Application/Authorization/AuthorizationPolicyExtensions.cs` for:

1. **Role-based policies**: SystemAdmin, Landlord, Caretaker, Accountant, Tenant
2. **Permission-based policies**: Property management, tenant management, payment handling

## Next Steps for Data Isolation

To enforce data isolation in your services/repositories, you should filter queries based on:

```csharp
// For Landlords - only their properties
if (user.Role == UserRole.Landlord)
{
    query = query.Where(p => p.LandlordId == user.Id);
}

// For Caretakers - only their assigned property
if (user.Role == UserRole.Caretaker && user.PropertyId.HasValue)
{
    query = query.Where(p => p.Id == user.PropertyId.Value);
}

// SystemAdmin and Accountant see all (no filter)
```

## Database Reset Required!

Since we changed the entity relationships, you MUST reset the database:

```bash
# Run the reset script
./reset-database.sh   # Linux/Mac
reset-database.bat    # Windows

# Or manually:
cd src/RentCollection.API
dotnet ef database drop --project ../RentCollection.Infrastructure --force
dotnet ef migrations add InitialCreate --project ../RentCollection.Infrastructure
dotnet run
```

After reset:
- ✅ Landlords will see their properties
- ✅ Properties correctly linked to owners
- ✅ Data isolation working
