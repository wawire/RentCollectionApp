# Property-User Relationship Fixes

## Issues Found

### 1. **Type Mismatch** ✅ FIXED
- **Problem**: `Property.LandlordId` was `string?` but `User.Id` is `int`
- **Cause**: Leftover from old ASP.NET Identity system (which used string IDs)
- **Fix**: Changed `Property.LandlordId` to `int?` and added `User? Landlord` navigation property

### 2. **Missing Property Ownership**
- **Problem**: Properties were seeded WITHOUT `LandlordId` values
- **Result**: Landlords appear with no properties!
- **Fix Needed**: Update `ApplicationDbContextSeed.cs` to assign `LandlordId` to properties

### 3. **Incorrect Seeding Order** ✅ FIXED
- **Problem**: Properties were seeded before Users
- **Issue**: Can't assign `LandlordId` if users don't exist yet
- **Fix**: Reversed order in `Program.cs` - now seeds Users first, then Properties

### 4. **Incorrect Landlord PropertyId Assignment**
- **Problem**: Landlords had `PropertyId = 1, 3, 5` (pointing to ONE property)
- **Issue**: Landlords can own MULTIPLE properties, not just one
- **Fix Needed**: Set `PropertyId = null` for landlords (they see all via `OwnedProperties`)

## Changes Made

### Entity Changes:
1. **Property.cs** ✅
   ```csharp
   public int? LandlordId { get; set; }  // Changed from string? to int?
   public User? Landlord { get; set; }    // Added navigation property
   ```

2. **User.cs** ✅
   ```csharp
   public ICollection<Property> OwnedProperties { get; set; } = new List<Property>();
   ```

3. **Program.cs** ✅
   - Reversed seeding order: Users first, then Properties

## Still Need To Fix (In New Migration)

### Update `DefaultUsers.cs`:

Change Landlord 1:
```csharp
PropertyId = null, // Landlords see ALL their properties (via OwnedProperties)
```

Change Landlord 2:
```csharp
PropertyId = null, // Landlords see ALL their properties (via OwnedProperties)
```

Change Landlord 3:
```csharp
PropertyId = null, // Landlords see ALL their properties (via OwnedProperties)
```

### Update `ApplicationDbContextSeed.cs`:

Add `LandlordId` to properties (assuming UserIds: Admin=1, John=2, Mary=3, David=4):

```csharp
// LANDLORD 1 (John - User Id 2) owns properties 1 & 2
new Property
{
    Name = "Sunset Apartments Westlands",
    Location = "Muthangari Road, Westlands, Nairobi",
    LandlordId = 2,  // ← ADD THIS (John)
    // ...
},
new Property
{
    Name = "Parklands Heights",
    Location = "5th Avenue, Parklands, Nairobi",
    LandlordId = 2,  // ← ADD THIS (John)
    // ...
},

// LANDLORD 2 (Mary - User Id 3) owns properties 3 & 4
new Property
{
    Name = "Kileleshwa Gardens",
    Location = "Mandera Road, Kileleshwa, Nairobi",
    LandlordId = 3,  // ← ADD THIS (Mary)
    // ...
},
new Property
{
    Name = "Lavington Court",
    Location = "James Gichuru Road, Lavington, Nairobi",
    LandlordId = 3,  // ← ADD THIS (Mary)
    // ...
},

// LANDLORD 3 (David - User Id 4) owns properties 5 & 6
new Property
{
    Name = "Utawala Maisonettes",
    Location = "Eastern Bypass, Utawala, Nairobi",
    LandlordId = 4,  // ← ADD THIS (David)
    // ...
},
new Property
{
    Name = "Ruiru Bungalows Estate",
    Location = "Ruiru-Kiambu Road, Ruiru",
    LandlordId = 4,  // ← ADD THIS (David)
    // ...
}
```

Also update Caretaker PropertyIds to use Property IDs (after properties are created):
- Caretaker 1 (Jane): PropertyId will be set after property seeding
- Caretaker 2 (Peter): PropertyId will be set after property seeding
- Caretaker 3 (James): PropertyId will be set after property seeding

**OR** Update caretakers after properties are seeded.

## Summary of Relationship Model

### For Landlords:
- **User.Id** = 2, 3, 4 (John, Mary, David)
- **User.PropertyId** = `null` (they see all their properties)
- **User.OwnedProperties** = Collection of all properties they own
- **Property.LandlordId** = Points back to the landlord's User.Id

### For Caretakers:
- **User.PropertyId** = Points to ONE property they manage
- **Property.LandlordId** = Points to the landlord who owns it

### For SystemAdmin/Accountant:
- **User.PropertyId** = `null` (they see ALL properties across all landlords)

## Next Steps

1. ✅ Run `reset-database.bat` or `reset-database.sh` to drop DB and delete old migrations
2. ✅ Run `dotnet ef migrations add InitialCreate` to create fresh migration
3. ⚠️ **Before running the app**, manually edit the seed files as shown above
4. ✅ Run the application - it will apply migration and seed data
5. ✅ Test: Landlords should now see their properties!

## Authorization Notes

The authorization policies should check:
- **Landlords**: Can only access properties where `Property.LandlordId == User.Id`
- **Caretakers**: Can only access properties where `Property.Id == User.PropertyId`
- **SystemAdmin/Accountant**: Can access ALL properties
