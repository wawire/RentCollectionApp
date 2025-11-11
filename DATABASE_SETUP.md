# Database Setup Guide - SQL Server

This guide will help you set up the SQL Server database for the Rent Collection Application using Entity Framework Core migrations.

## Prerequisites

- ✅ .NET 8 SDK installed
- ✅ SQL Server 2019+ or SQL Server Express installed
- ✅ SQL Server Management Studio (SSMS) installed (recommended)
- ✅ Project cloned and packages restored

## Step-by-Step Database Setup

### 1. Verify Prerequisites

```bash
# Check .NET version
dotnet --version
# Should show 8.0.x

# Check EF Core tools
dotnet ef --version
# Should show 8.0.x
```

If EF Core tools are not installed:
```bash
dotnet tool install --global dotnet-ef
```

### 2. Configure SQL Server Connection

Update the connection string in **one** of these files based on your setup:

#### Option A: Windows Authentication (Recommended)
`src/RentCollection.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RentCollection_Dev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

#### Option B: SQL Server Authentication
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RentCollection_Dev;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

#### Option C: SQL Server Express
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=RentCollection_Dev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Create Initial Migration

Navigate to the API project directory:
```bash
cd src/RentCollection.API
```

Create the initial migration:
```bash
dotnet ef migrations add InitialCreate --project ../RentCollection.Infrastructure
```

**Expected Output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

This creates a new `Migrations` folder in `RentCollection.Infrastructure` with:
- `{timestamp}_InitialCreate.cs` - Migration file
- `{timestamp}_InitialCreate.Designer.cs` - Designer file
- `ApplicationDbContextModelSnapshot.cs` - Current model snapshot

### 4. Review Generated Migration

Open `src/RentCollection.Infrastructure/Migrations/{timestamp}_InitialCreate.cs`

You should see:
- ✅ `CreateTable` for Properties
- ✅ `CreateTable` for Units
- ✅ `CreateTable` for Tenants
- ✅ `CreateTable` for Payments
- ✅ `CreateTable` for SmsLogs
- ✅ Foreign key relationships
- ✅ Indexes

### 5. Apply Migration to Database

```bash
dotnet ef database update --project ../RentCollection.Infrastructure
```

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20241111_InitialCreate'.
Done.
```

### 6. Verify Database Creation

#### Using SSMS:
1. Open SQL Server Management Studio
2. Connect to your server (localhost or localhost\SQLEXPRESS)
3. Expand **Databases** → **RentCollection_Dev**
4. Expand **Tables**

You should see:
- `dbo.Properties`
- `dbo.Units`
- `dbo.Tenants`
- `dbo.Payments`
- `dbo.SmsLogs`
- `dbo.__EFMigrationsHistory` (tracks migrations)

#### Using Command Line:
```bash
sqlcmd -S localhost -E -Q "USE RentCollection_Dev; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
```

### 7. (Optional) Seed Sample Data

To populate the database with sample data for development/testing:

#### Option A: Code-based Seeding

Update `src/RentCollection.API/Program.cs` to add seeding:

```csharp
// Add this before app.Run()
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        await ApplicationDbContextSeed.SeedAsync(context, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

app.Run();
```

Then run the application once:
```bash
dotnet run
```

#### Option B: Manual SQL Insert

Run this SQL script in SSMS:

```sql
USE RentCollection_Dev;

-- Insert sample property
INSERT INTO Properties (Name, Location, Description, TotalUnits, IsActive, CreatedAt)
VALUES ('Sunset Apartments', '123 Main St, Nairobi', 'Modern apartments', 10, 1, GETUTCDATE());

-- Get the property ID
DECLARE @PropertyId INT = SCOPE_IDENTITY();

-- Insert sample unit
INSERT INTO Units (UnitNumber, PropertyId, MonthlyRent, Bedrooms, Bathrooms, IsOccupied, IsActive, CreatedAt)
VALUES ('A101', @PropertyId, 25000, 2, 1, 0, 1, GETUTCDATE());

-- Verify
SELECT * FROM Properties;
SELECT * FROM Units;
```

## Common Commands Reference

### Create a New Migration
```bash
cd src/RentCollection.API
dotnet ef migrations add MigrationName --project ../RentCollection.Infrastructure
```

### Apply Migrations
```bash
dotnet ef database update --project ../RentCollection.Infrastructure
```

### Rollback Last Migration
```bash
dotnet ef database update PreviousMigrationName --project ../RentCollection.Infrastructure
```

### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove --project ../RentCollection.Infrastructure
```

### View Migration List
```bash
dotnet ef migrations list --project ../RentCollection.Infrastructure
```

### Generate SQL Script (without applying)
```bash
dotnet ef migrations script --project ../RentCollection.Infrastructure --output migration.sql
```

### Drop Database (Caution!)
```bash
dotnet ef database drop --project ../RentCollection.Infrastructure
```

## Troubleshooting

### Error: "Build failed"
**Solution:** Make sure all projects compile successfully
```bash
cd ../..  # Go to solution root
dotnet build
```

### Error: "Unable to create migrations"
**Solution:** Ensure you're in the correct directory
```bash
cd src/RentCollection.API
# Then run migration command
```

### Error: "Login failed for user"
**Solution:** Check your connection string credentials or use Windows Authentication

### Error: "Cannot open database"
**Solution:** Ensure SQL Server service is running
```bash
# Check service status (Windows)
Get-Service MSSQLSERVER
# or for SQL Express
Get-Service MSSQL$SQLEXPRESS
```

### Error: "The term 'dotnet-ef' is not recognized"
**Solution:** Install EF Core tools globally
```bash
dotnet tool install --global dotnet-ef
```

## Database Schema Overview

### Tables Created

1. **Properties**
   - Id (PK)
   - Name, Location, Description
   - TotalUnits, IsActive
   - CreatedAt, UpdatedAt, CreatedBy, UpdatedBy

2. **Units**
   - Id (PK)
   - UnitNumber, PropertyId (FK)
   - MonthlyRent, Bedrooms, Bathrooms, SquareFeet
   - IsOccupied, IsActive
   - CreatedAt, UpdatedAt

3. **Tenants**
   - Id (PK)
   - FirstName, LastName, Email, PhoneNumber, IdNumber
   - UnitId (FK)
   - LeaseStartDate, LeaseEndDate
   - MonthlyRent, SecurityDeposit
   - IsActive, Notes
   - CreatedAt, UpdatedAt

4. **Payments**
   - Id (PK)
   - TenantId (FK)
   - Amount, PaymentDate
   - PaymentMethod, Status
   - TransactionReference, Notes
   - PeriodStart, PeriodEnd
   - CreatedAt, UpdatedAt

5. **SmsLogs**
   - Id (PK)
   - PhoneNumber, Message
   - Status, SentAt
   - ExternalId, ErrorMessage
   - TenantId (nullable)
   - CreatedAt

### Relationships

- Properties → Units (1:Many, Cascade Delete)
- Units → Tenants (1:Many, Restrict Delete)
- Tenants → Payments (1:Many, Restrict Delete)

### Indexes

- Properties: Name
- Units: PropertyId + UnitNumber (Unique)
- Tenants: Email, PhoneNumber
- Payments: PaymentDate, Status, TransactionReference
- SmsLogs: PhoneNumber, Status, SentAt

## Next Steps

After successful database setup:

1. ✅ Run the API to verify connection
   ```bash
   cd src/RentCollection.API
   dotnet run
   ```

2. ✅ Access Swagger UI
   - Navigate to: `https://localhost:7xxx/swagger`
   - Test the API endpoints

3. ✅ Proceed to Phase 2, Step 2.4: Complete API Controllers

## Production Deployment

For production deployment:

1. Create production database
2. Update `appsettings.Production.json` with production connection string
3. Run migrations:
   ```bash
   dotnet ef database update --project src/RentCollection.Infrastructure --startup-project src/RentCollection.API --configuration Release
   ```
4. **Do not** seed sample data in production

## Backup and Restore

### Backup Database
```sql
BACKUP DATABASE RentCollection_Dev
TO DISK = 'C:\Backup\RentCollection_Dev.bak'
WITH FORMAT, INIT, NAME = 'Full Backup of RentCollection_Dev';
```

### Restore Database
```sql
RESTORE DATABASE RentCollection_Dev
FROM DISK = 'C:\Backup\RentCollection_Dev.bak'
WITH REPLACE;
```

---

**Need Help?**
- Check the main README.md for general setup instructions
- Review WORK_PLAN.md for the complete implementation roadmap
- Consult SQL Server documentation: https://docs.microsoft.com/en-us/sql/
- EF Core Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
