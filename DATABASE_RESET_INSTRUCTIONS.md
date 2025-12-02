# Database Reset Instructions

The database schema is out of sync with the entity models. Follow these steps to reset:

## Option 1: Use the provided scripts

### Windows:
```cmd
reset-database.bat
```

### Linux/Mac:
```bash
./reset-database.sh
```

## Option 2: Manual commands

Navigate to the API project directory:
```bash
cd src/RentCollection.API
```

Then run these commands:

### 1. Drop the existing database
```bash
dotnet ef database drop --project ../RentCollection.Infrastructure --force
```

### 2. Create a fresh migration
```bash
dotnet ef migrations add InitialCreate --project ../RentCollection.Infrastructure --context ApplicationDbContext
```

### 3. Run the application
The application will automatically:
- Apply the migration (create all tables with correct schema)
- Seed the database with demo data

```bash
dotnet run
```

## What was fixed

- Old migrations deleted (they were missing the `LandlordId` column in Properties table)
- Fresh migration will be created with all current entity properties
- Database will be recreated with correct schema

## Demo Credentials (after seeding)

- **System Admin**: admin@rentcollection.com / Admin@123
- **Landlord 1**: landlord1@example.com / Landlord@123
- **Landlord 2**: landlord2@example.com / Landlord@123
- **Caretaker**: caretaker@example.com / Caretaker@123
- **Accountant**: accountant@example.com / Accountant@123
