# Update Roles - Database Migration Required

## Changes Made

I've updated the authentication system to use proper Kenyan property management roles:

### New Roles:
- **SystemAdmin** (was: Admin) - Full system access
- **Landlord** (new) - Property owner with full access to their properties
- **Caretaker** (was: PropertyManager) - Day-to-day operations for a landlord
- **Accountant** (new) - Financial records access only
- **Tenant** (new) - For future self-service portal

### Database Changes:
1. Added `LandlordId` column to `ApplicationUser` table
2. Added `LandlordId` column to `Properties` table
3. Updated role names in the database

---

## Migration Steps (Run Locally)

### Step 1: Pull Latest Changes
```bash
git pull origin claude/review-work-plan-01Eizmxv3Nzd4bRG5xZoiuZW
```

### Step 2: Drop and Recreate Database (Recommended)
Since this is a development environment, it's easiest to drop the database and recreate it:

```bash
# Navigate to API project
cd src/RentCollection.API

# Drop the database
dotnet ef database drop --project ../RentCollection.Infrastructure --force

# Create new migration
dotnet ef migrations add UpdateRolesToKenyanContext --project ../RentCollection.Infrastructure

# Apply migration
dotnet ef database update --project ../RentCollection.Infrastructure
```

### Step 3: Verify Seeded Users

After migration, the following users will be created:

| Email | Password | Role | Notes |
|-------|----------|------|-------|
| admin@rentcollection.com | Admin@123 | SystemAdmin | Full system access |
| landlord1@example.com | Landlord@123 | Landlord | John Kariuki - owns properties |
| landlord2@example.com | Landlord@123 | Landlord | Mary Wanjiku - owns different properties |
| caretaker@example.com | Caretaker@123 | Caretaker | James Omondi - works for Landlord 1 |
| accountant@example.com | Accountant@123 | Accountant | Grace Mutua - accountant for Landlord 1 |

### Step 4: Test Authentication

1. Start the API: `dotnet run` (from RentCollection.API folder)
2. Start the WebApp: `npm run dev` (from RentCollection.WebApp folder)
3. Navigate to http://localhost:3000/login
4. Try logging in with different users

---

## Expected Behavior (After Implementing Authorization Logic)

Once we implement the authorization logic:

### SystemAdmin
- ✅ Can see ALL properties from all landlords
- ✅ Can manage all users
- ✅ Full system access

### Landlord 1 (John)
- ✅ Can see ONLY their own properties
- ✅ Can manage their own tenants, units, payments
- ✅ Can view financial analytics for their properties
- ❌ Cannot see Landlord 2's properties

### Landlord 2 (Mary)
- ✅ Can see ONLY their own properties
- ❌ Cannot see Landlord 1's properties

### Caretaker (James - works for Landlord 1)
- ✅ Can see properties owned by Landlord 1 only
- ✅ Can add/edit tenants for Landlord 1's properties
- ✅ Can record payments
- ❌ Cannot see Landlord 2's data
- ❌ Cannot view financial analytics
- ❌ Cannot delete properties

### Accountant (Grace - works for Landlord 1)
- ✅ Can view payment records for Landlord 1's properties
- ✅ Can generate financial reports
- ❌ Cannot add/edit tenants
- ❌ Cannot record payments (view only)
- ❌ Cannot send SMS

---

## Next Steps

To make the roles actually work (data isolation), we need to:

1. ✅ **Update Property Entity** - Added LandlordId field (DONE)
2. ⏳ **Update Property Service** - Filter by LandlordId based on current user
3. ⏳ **Update Property Controller** - Set LandlordId when creating properties
4. ⏳ **Add Authorization Attributes** - Role-based permissions on controllers
5. ⏳ **Update Frontend** - Hide/show features based on user role

Would you like me to continue and implement the authorization logic (steps 2-5) so that:
- Landlord 1 only sees their 3 properties
- Landlord 2 only sees their 3 properties
- Caretaker only sees Landlord 1's properties
- Accountant has read-only access?

---

## Current Status

✅ Roles defined and seeded
✅ Database schema updated
✅ Frontend showing new role names
⏳ **Data filtering NOT yet implemented** - all users still see all data

That's why you're currently seeing the same 6 properties for everyone. Let me know if you want me to implement the authorization logic next!
