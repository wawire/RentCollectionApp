# EF Core Migrations Safety

## Guardrails
- Never run `dotnet ef migrations remove` unless you have confirmed the migration is the last one in the chain.
- Prefer forward-only migrations over editing existing migration files.
- If a migration has been applied in any environment, do not delete it. Create a new migration to fix issues.

## Safe Workflow
1) Stop any running API process that may lock binaries.
2) Build the solution.
3) Add a new migration.
4) Apply the migration to your target database.

## Commands (PowerShell)
```powershell
# Stop running API (if any)
.\scripts\stop-api.ps1

# Build
dotnet build .\RentCollectionApp.sln

# Add a migration (example)
dotnet ef migrations add DescribeChange -s .\src\RentCollection.API -p .\src\RentCollection.Infrastructure

# Apply migrations
dotnet ef database update -s .\src\RentCollection.API -p .\src\RentCollection.Infrastructure
```
