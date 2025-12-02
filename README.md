# Rent Collection Application

Modern property management system for landlords, caretakers, and tenants in Kenya. Built with .NET 8 and Next.js 15.

## Quick Start

```bash
# 1. Reset database and create migration
./reset-database.sh   # Linux/Mac
reset-database.bat    # Windows

# 2. Run backend API
cd src/RentCollection.API
dotnet run

# 3. Run frontend (separate terminal)
cd src/RentCollection.WebApp
npm install
npm run dev
```

Access the application:
- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| **System Admin** | admin@rentcollection.com | Admin@123 |
| **Landlord** | landlord@example.com | Landlord@123 |
| **Caretaker** | caretaker@example.com | Caretaker@123 |
| **Accountant** | accountant@example.com | Accountant@123 |

## Architecture

Clean Architecture with DDD principles:

```
├── Domain/          # Entities, enums, interfaces
├── Application/     # Business logic, DTOs, services
├── Infrastructure/  # Data access, external services
├── API/             # REST API endpoints
└── WebApp/          # Next.js frontend
```

## Tech Stack

**Backend**: .NET 8, EF Core, SQL Server, JWT Auth
**Frontend**: Next.js 15, React 18, TypeScript, Tailwind CSS
**Services**: M-Pesa integration, SMS (Africa's Talking), PDF generation

## Features

- 🏢 Multi-property management
- 👥 User roles: SystemAdmin, Landlord, Caretaker, Accountant, Tenant
- 💰 Payment tracking (M-Pesa, Bank Transfer, Cash)
- 📱 SMS notifications and reminders
- 📊 Financial reports and dashboards
- 📄 PDF receipt generation
- 🔐 JWT authentication with role-based access control

## Development

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Apply migrations
cd src/RentCollection.API
dotnet ef database update --project ../RentCollection.Infrastructure
```

See [SETUP.md](SETUP.md) for detailed setup instructions and user role documentation.

## License

MIT License
