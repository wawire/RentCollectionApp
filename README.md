# Rent Collection Application

Modern property management system for landlords, caretakers, and tenants in Kenya. Built with .NET 8 and Next.js 15.

## Features

- Multi-property management with role-based access control
- Payment tracking (M-Pesa, Bank Transfer, Cash)
- Tenant payment portal with M-Pesa STK Push integration
- Payment confirmation workflow for landlords
- SMS notifications and reminders
- PDF receipt generation
- Financial reports and dashboards

## Quick Start

```bash
# Clone and navigate to the project
cd RentCollectionApp

# Start backend API
cd src/RentCollection.API
dotnet run

# Start frontend (in a new terminal)
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
| **Tenant** | peter.mwangi@gmail.com | Tenant@123 |
| **Caretaker** | caretaker@example.com | Caretaker@123 |

## Tech Stack

**Backend**
- .NET 8 with Clean Architecture + DDD
- Entity Framework Core with SQL Server
- JWT Authentication
- M-Pesa Daraja API integration
- SMS via Africa's Talking

**Frontend**
- Next.js 15 with TypeScript
- React 18
- Tailwind CSS
- Client-side auth with JWT

## User Roles

- **SystemAdmin**: Access to all properties across all landlords
- **Landlord**: Manages their own properties, units, tenants, and payments
- **Caretaker**: Manages a single assigned property
- **Accountant**: Read-only access to financial reports
- **Tenant**: View payment instructions and record payments

## Project Structure

```
├── src/
│   ├── RentCollection.Domain/       # Entities, enums, interfaces
│   ├── RentCollection.Application/  # Business logic, DTOs, services
│   ├── RentCollection.Infrastructure/  # Data access, external services
│   ├── RentCollection.API/          # REST API controllers
│   └── RentCollection.WebApp/       # Next.js frontend
└── docs/
    ├── ARCHITECTURE.md              # System design and data models
    ├── API.md                       # Complete API documentation
    └── DEVELOPMENT.md               # Setup, testing, contributing
```

## Documentation

- [Architecture](docs/ARCHITECTURE.md) - System design, payment flows, database schema
- [API Reference](docs/API.md) - Complete API endpoint documentation
- [Development Guide](docs/DEVELOPMENT.md) - Setup instructions, testing, contributing

## License

MIT
