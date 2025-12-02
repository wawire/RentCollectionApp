# Setup Guide

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server (LocalDB or full instance)

## Database Setup

### Option 1: Quick Reset (Recommended for Dev)

```bash
./reset-database.sh   # Linux/Mac
reset-database.bat    # Windows
```

This will:
1. Drop existing database
2. Delete old migrations
3. Create fresh migration
4. Seed demo data on first run

### Option 2: Manual Setup

```bash
cd src/RentCollection.API

# Drop database (if exists)
dotnet ef database drop --project ../RentCollection.Infrastructure --force

# Create migration
dotnet ef migrations add InitialCreate --project ../RentCollection.Infrastructure

# Apply migration (or just run the app - it auto-migrates)
dotnet ef database update --project ../RentCollection.Infrastructure
```

## Running the Application

### Backend API

```bash
cd src/RentCollection.API
dotnet run
```

API runs on: http://localhost:5000
Swagger UI: http://localhost:5000/swagger

### Frontend

```bash
cd src/RentCollection.WebApp
npm install
npm run dev
```

Frontend runs on: http://localhost:3000

## User Roles & Access

### SystemAdmin
- **Email**: admin@rentcollection.com
- **Password**: Admin@123
- **Access**: All properties across all landlords

### Landlords (3 landlords, each owns 2 properties)

**John Landlord**
- **Email**: landlord@example.com
- **Password**: Landlord@123
- **Properties**: Sunset Apartments Westlands, Parklands Heights

**Mary Wanjiku**
- **Email**: mary.wanjiku@example.com
- **Password**: Landlord@123
- **Properties**: Kileleshwa Gardens, Lavington Court

**David Kamau**
- **Email**: david.kamau@example.com
- **Password**: Landlord@123
- **Properties**: Utawala Maisonettes, Ruiru Bungalows

### Caretakers (assigned to specific properties)

**Jane Mueni** - Sunset Apartments
- **Email**: caretaker@example.com
- **Password**: Caretaker@123

**Peter Kamau** - Kileleshwa Gardens
- **Email**: peter.caretaker@example.com
- **Password**: Caretaker@123

**James Omondi** - Utawala Maisonettes
- **Email**: james.caretaker@example.com
- **Password**: Caretaker@123

### Accountant
- **Email**: accountant@example.com
- **Password**: Accountant@123
- **Access**: All properties (read-only financial reports)

## Data Isolation

| Role | Access Scope |
|------|--------------|
| **SystemAdmin** | ALL properties (all landlords) |
| **Landlord** | Only properties they own (`Property.LandlordId = User.Id`) |
| **Caretaker** | Single property (`Property.Id = User.PropertyId`) |
| **Accountant** | ALL properties (read-only) |
| **Tenant** | Own tenant record only |

## Configuration

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RentCollectionDB;Trusted_Connection=true;"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key",
    "Issuer": "RentCollectionAPI",
    "Audience": "RentCollectionApp"
  }
}
```

### Frontend (.env.local)

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

## Troubleshooting

### Migration Issues
Run the reset script to drop and recreate the database with a fresh migration.

### Port Already in Use
Backend uses port 5000, frontend uses 3000. Change in:
- Backend: `launchSettings.json`
- Frontend: `npm run dev -- -p 3001`

### Authentication Issues
Check JWT secret is consistent in `appsettings.json` and tokens haven't expired (24h default).
