@echo off
REM Script to create and apply Identity tables migration
REM Run this script from the RentCollectionApp root directory on your local machine

echo Creating EF Core migration for Identity tables...
cd src\RentCollection.API
dotnet ef migrations add AddIdentityTables --project ..\RentCollection.Infrastructure --context ApplicationDbContext

echo.
echo Applying migration to database...
dotnet ef database update --project ..\RentCollection.Infrastructure --context ApplicationDbContext

echo.
echo Migration applied successfully!
echo.
echo You can now test the authentication flow:
echo 1. Pull the latest changes from the remote repository
echo 2. Run this script to apply migrations
echo 3. Start the API: cd src\RentCollection.API ^&^& dotnet run
echo 4. Start the WebApp: cd src\RentCollection.WebApp ^&^& npm run dev
echo 5. Navigate to http://localhost:3000/login
echo 6. Use demo credentials: admin@rentcollection.com / Admin@123

pause
