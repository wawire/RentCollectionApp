@echo off
echo Dropping database and resetting migrations...

cd /d "%~dp0src\RentCollection.API"

REM Drop the database
echo Dropping database...
dotnet ef database drop --project ..\RentCollection.Infrastructure --force

REM Delete old migrations
echo Deleting old migrations...
del /q ..\RentCollection.Infrastructure\Migrations\*.*

REM Create fresh initial migration
echo Creating fresh initial migration...
dotnet ef migrations add InitialCreate --project ..\RentCollection.Infrastructure --context ApplicationDbContext

echo Database reset complete! Run the application to apply migrations and seed data.
pause
