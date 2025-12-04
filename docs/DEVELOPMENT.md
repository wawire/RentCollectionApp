# Development Guide

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server (LocalDB or full instance)
- Git

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd RentCollectionApp
```

### 2. Database Setup

**Option A: Quick Reset (Recommended)**

```bash
./reset-database.sh   # Linux/Mac
reset-database.bat    # Windows
```

This will:
- Drop existing database
- Delete old migrations
- Create fresh migration
- Seed demo data

**Option B: Manual Setup**

```bash
cd src/RentCollection.API

# Drop database
dotnet ef database drop --project ../RentCollection.Infrastructure --force

# Create migration
dotnet ef migrations add InitialCreate --project ../RentCollection.Infrastructure

# Apply migration (or just run the app - it auto-migrates)
dotnet ef database update --project ../RentCollection.Infrastructure
```

### 3. Backend Configuration

Create `src/RentCollection.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RentCollectionDB;Trusted_Connection=true;"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-here-change-in-production",
    "Issuer": "RentCollectionAPI",
    "Audience": "RentCollectionApp",
    "ExpirationHours": 24
  }
}
```

### 4. Run Backend

```bash
cd src/RentCollection.API
dotnet run
```

API will be available at:
- http://localhost:5000
- Swagger: http://localhost:5000/swagger

### 5. Frontend Configuration

Create `src/RentCollection.WebApp/.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

### 6. Run Frontend

```bash
cd src/RentCollection.WebApp
npm install
npm run dev
```

Frontend will be available at: http://localhost:3000

---

## Test Credentials

### System Admin
```
Email: admin@rentcollection.com
Password: Admin@123
```

### Landlords
```
Email: landlord@example.com
Password: Landlord@123
Properties: Sunset Apartments Westlands, Parklands Heights

Email: mary.wanjiku@example.com
Password: Landlord@123
Properties: Kileleshwa Gardens, Lavington Court
```

### Tenants
```
Email: peter.mwangi@gmail.com
Password: Tenant@123
Unit: B1 - Sunset Apartments (Rent: KSh 12,000)

Email: grace.akinyi@yahoo.com
Password: Tenant@123
Unit: 1A - Sunset Apartments (Rent: KSh 18,000)

Email: alice.wambui@gmail.com
Password: Tenant@123
Unit: K-2A - Kileleshwa Gardens (Rent: KSh 35,000)
```

### Caretakers
```
Email: caretaker@example.com
Password: Caretaker@123
Property: Sunset Apartments
```

---

## Testing

### Backend Testing

**Run all tests:**
```bash
dotnet test
```

**Run with coverage:**
```bash
dotnet test /p:CollectCoverage=true
```

### Frontend Testing

**Run tests:**
```bash
cd src/RentCollection.WebApp
npm test
```

**Run E2E tests:**
```bash
npm run test:e2e
```

### Manual Testing Workflow

#### 1. Tenant Payment Flow

**Login as Tenant:**
```bash
POST http://localhost:5000/api/Auth/login
{
  "email": "peter.mwangi@gmail.com",
  "password": "Tenant@123"
}
```

**View Payment Instructions:**
```bash
GET http://localhost:5000/api/TenantPayments/instructions
Authorization: Bearer {token}
```

**Record Payment:**
```bash
POST http://localhost:5000/api/TenantPayments/record
Authorization: Bearer {token}
{
  "amount": 12000,
  "paymentDate": "2025-12-04T10:00:00Z",
  "paymentMethod": "MPesa",
  "transactionReference": "TEST123",
  "mPesaPhoneNumber": "0723870917",
  "periodStart": "2025-12-01T00:00:00Z",
  "periodEnd": "2025-12-31T23:59:59Z"
}
```

#### 2. Landlord Confirmation Flow

**Login as Landlord:**
```bash
POST http://localhost:5000/api/Auth/login
{
  "email": "landlord@example.com",
  "password": "Landlord@123"
}
```

**View Pending Payments:**
```bash
GET http://localhost:5000/api/Payments/pending
Authorization: Bearer {landlord_token}
```

**Confirm Payment:**
```bash
PUT http://localhost:5000/api/Payments/{id}/confirm
Authorization: Bearer {landlord_token}
```

---

## Development Workflow

### Backend Changes

1. **Create/modify entities** in `Domain/Entities/`
2. **Create migration:**
   ```bash
   dotnet ef migrations add MigrationName --project ../RentCollection.Infrastructure
   ```
3. **Apply migration:**
   ```bash
   dotnet ef database update --project ../RentCollection.Infrastructure
   ```
4. **Build and test:**
   ```bash
   dotnet build
   dotnet test
   ```

### Frontend Changes

1. **Update types** in `lib/types/`
2. **Update services** in `lib/services/`
3. **Update components** in `app/` or `components/`
4. **Test locally:**
   ```bash
   npm run dev
   ```

### Code Style

**Backend:**
- Follow C# coding conventions
- Use meaningful variable names
- Add XML comments for public APIs
- Keep controllers thin, logic in services

**Frontend:**
- TypeScript strict mode enabled
- Functional components with hooks
- Use Tailwind for styling
- Component files use PascalCase

---

## Common Tasks

### Add New User Role

1. Add to `Domain/Enums/UserRole.cs`
2. Update authorization policies in `API/Program.cs`
3. Update frontend role checks in `lib/auth/`
4. Create migration if needed

### Add Payment Method

1. Add to `Domain/Enums/PaymentMethod.cs`
2. Update `Application/Services/PaymentService.cs`
3. Update frontend payment form
4. Update API documentation

### Add New API Endpoint

1. Create DTO in `Application/DTOs/`
2. Add service method in `Application/Services/`
3. Implement in `Infrastructure/Services/`
4. Create controller in `API/Controllers/`
5. Test with Swagger
6. Add to `docs/API.md`

---

## Troubleshooting

### Database Issues

**"Database does not exist"**
```bash
cd src/RentCollection.API
dotnet ef database update --project ../RentCollection.Infrastructure
```

**"Migration already exists"**
```bash
# Delete Migrations folder and start fresh
rm -rf ../RentCollection.Infrastructure/Migrations
dotnet ef migrations add InitialCreate --project ../RentCollection.Infrastructure
```

### Backend Issues

**Port 5000 already in use:**
- Change port in `launchSettings.json`
- Or kill the process using port 5000

**JWT errors:**
- Ensure JWT secret is consistent in `appsettings.json`
- Check token hasn't expired (default 24h)

### Frontend Issues

**API calls fail:**
- Check `NEXT_PUBLIC_API_URL` in `.env.local`
- Ensure backend is running
- Check CORS settings in backend

**Auth not working:**
- Clear localStorage
- Check token is being sent in headers
- Verify role-based routing in `lib/auth/`

---

## Data Seeding

The application seeds demo data on first run:
- 1 System Admin
- 3 Landlords with 6 properties
- 3 Caretakers
- 1 Accountant
- 9 Tenants (all active in occupied units)
- 5 Payment accounts
- Sample payment records

To re-seed, drop and recreate the database.

---

## Environment Variables

### Backend (.NET)

`appsettings.json`:
- `ConnectionStrings:DefaultConnection` - Database connection
- `Jwt:Secret` - JWT signing key
- `Jwt:Issuer` - Token issuer
- `Jwt:Audience` - Token audience
- `MPesa:ConsumerKey` - M-Pesa API key (optional)
- `MPesa:ConsumerSecret` - M-Pesa secret (optional)

### Frontend (Next.js)

`.env.local`:
- `NEXT_PUBLIC_API_URL` - Backend API base URL

---

## Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make changes and test thoroughly
3. Commit with clear messages: `git commit -m "Add feature X"`
4. Push to branch: `git push origin feature/your-feature`
5. Create pull request

### Commit Message Format

```
Add feature X
Update component Y
Fix bug in Z
Refactor service A
```

### Pull Request Checklist

- [ ] Code builds without errors
- [ ] All tests pass
- [ ] New features have tests
- [ ] Documentation updated
- [ ] No console errors/warnings
- [ ] API changes documented in `docs/API.md`

---

## Deployment

### Backend Deployment

1. Update `appsettings.Production.json`
2. Set production connection string
3. Use strong JWT secret
4. Build release:
   ```bash
   dotnet publish -c Release
   ```
5. Deploy to Azure/AWS/your server

### Frontend Deployment

1. Set production API URL in environment
2. Build:
   ```bash
   npm run build
   ```
3. Deploy to Vercel/Netlify/your server

---

## Useful Commands

```bash
# Backend
dotnet build                    # Build solution
dotnet test                     # Run tests
dotnet run                      # Start API
dotnet ef migrations add Name   # Create migration
dotnet ef database update       # Apply migrations

# Frontend
npm install                     # Install dependencies
npm run dev                     # Start dev server
npm run build                   # Build for production
npm run lint                    # Run linter
npm test                        # Run tests

# Database
./reset-database.sh             # Reset database (Linux/Mac)
reset-database.bat              # Reset database (Windows)
```

---

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet)
- [Next.js Documentation](https://nextjs.org/docs)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [M-Pesa Daraja API](https://developer.safaricom.co.ke)
