# Rent Collection Application

A comprehensive full-stack application for managing rental properties, tenants, and payments built with .NET 8 and Next.js 15.

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

- **Domain Layer**: Core business entities and enums
- **Application Layer**: Business logic, DTOs, services, and validators
- **Infrastructure Layer**: Data access, external services (EF Core, SMS, PDF)
- **API Layer**: ASP.NET Core Web API with RESTful endpoints
- **Frontend**: Next.js 15 with TypeScript and Tailwind CSS

## Project Structure

```
RentCollectionApp/
├── src/
│   ├── RentCollection.Domain/              # Core entities
│   ├── RentCollection.Application/         # Business logic
│   ├── RentCollection.Infrastructure/      # Data & external services
│   ├── RentCollection.API/                 # Web API
│   └── RentCollection.WebApp/              # Next.js 15 frontend
├── tests/
│   ├── RentCollection.UnitTests/
│   └── RentCollection.IntegrationTests/
├── .gitignore
├── README.md
└── RentCollectionApp.sln
```

## Technologies Used

### Backend
- **.NET 8**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 8**: ORM for database operations
- **PostgreSQL**: Primary database
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **QuestPDF**: PDF generation
- **RestSharp**: HTTP client for Africa's Talking SMS API

### Frontend
- **Next.js 15**: React framework with App Router
- **React 18**: UI library
- **TypeScript**: Type-safe JavaScript
- **Tailwind CSS**: Utility-first CSS framework
- **Axios**: HTTP client
- **React Icons**: Icon library

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (for frontend)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- IDE: Visual Studio 2022, VS Code, or JetBrains Rider

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd RentCollectionApp
```

### 2. Database Setup

1. Install PostgreSQL
2. Create a new database:
   ```sql
   CREATE DATABASE rentcollection_dev;
   ```
3. Update the connection string in `src/RentCollection.API/appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=rentcollection_dev;Username=postgres;Password=yourpassword"
   }
   ```

### 3. Backend Setup

1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Run database migrations:
   ```bash
   cd src/RentCollection.API
   dotnet ef database update --project ../RentCollection.Infrastructure
   ```

3. Run the API:
   ```bash
   dotnet run
   ```

   The API will be available at:
   - HTTPS: `https://localhost:7000`
   - HTTP: `http://localhost:5000`
   - Swagger UI: `https://localhost:7000/swagger`

### 4. Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd src/RentCollection.WebApp
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Create environment file:
   ```bash
   cp .env.example .env.local
   ```

4. Update `.env.local` with your API URL:
   ```
   NEXT_PUBLIC_API_URL=https://localhost:7000/api
   ```

5. Run the development server:
   ```bash
   npm run dev
   ```

   The frontend will be available at `http://localhost:3000`

## Features

### Core Functionality
- ✅ Property Management (CRUD operations)
- ✅ Unit Management per property
- ✅ Tenant Management with lease tracking
- ✅ Payment Recording and tracking
- ✅ Dashboard with statistics
- ✅ Monthly reports
- ✅ SMS notifications (Africa's Talking integration)
- ✅ PDF receipt generation

### Technical Features
- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ Dependency Injection
- ✅ AutoMapper for DTO mapping
- ✅ FluentValidation for input validation
- ✅ Global exception handling
- ✅ Structured logging with Serilog
- ✅ API documentation with Swagger
- ✅ CORS configuration
- ✅ TypeScript for type safety
- ✅ Responsive design with Tailwind CSS

## API Endpoints

### Properties
- `GET /api/properties` - Get all properties
- `GET /api/properties/{id}` - Get property by ID
- `POST /api/properties` - Create new property
- `PUT /api/properties/{id}` - Update property
- `DELETE /api/properties/{id}` - Delete property

### Dashboard
- `GET /api/dashboard/stats` - Get dashboard statistics
- `GET /api/dashboard/monthly-report/{year}` - Get monthly report

*Full API documentation available at `/swagger` when running the API*

## Database Migrations

### Create a new migration
```bash
cd src/RentCollection.API
dotnet ef migrations add MigrationName --project ../RentCollection.Infrastructure
```

### Apply migrations
```bash
dotnet ef database update --project ../RentCollection.Infrastructure
```

### Remove last migration
```bash
dotnet ef migrations remove --project ../RentCollection.Infrastructure
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run unit tests only
```bash
dotnet test tests/RentCollection.UnitTests
```

### Run integration tests only
```bash
dotnet test tests/RentCollection.IntegrationTests
```

## Configuration

### Backend Configuration
Configuration is managed through `appsettings.json` files:

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides (not in source control)

### Frontend Configuration
Environment variables in `.env.local`:

```env
NEXT_PUBLIC_API_URL=https://localhost:7000/api
```

## Building for Production

### Backend
```bash
dotnet publish src/RentCollection.API -c Release -o ./publish
```

### Frontend
```bash
cd src/RentCollection.WebApp
npm run build
npm run start
```

## Project Dependencies

### Backend NuGet Packages
- Microsoft.EntityFrameworkCore (8.0.0)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.0)
- AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1)
- FluentValidation.DependencyInjectionExtensions (11.9.0)
- Swashbuckle.AspNetCore (6.5.0)
- Serilog.AspNetCore (8.0.0)
- QuestPDF (2024.1.0)
- RestSharp (110.2.0)

### Frontend NPM Packages
- next (^15.0.0)
- react (^18.3.0)
- typescript (^5)
- tailwindcss (^3.4.0)
- axios (^1.6.2)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please create an issue in the GitHub repository.
