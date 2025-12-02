#!/bin/bash

echo "🗑️  Dropping database and resetting migrations..."

cd "$(dirname "$0")/src/RentCollection.API"

# Drop the database
echo "Dropping database..."
dotnet ef database drop --project ../RentCollection.Infrastructure --force

# Delete old migrations
echo "Deleting old migrations..."
rm -rf ../RentCollection.Infrastructure/Migrations/*

# Create fresh initial migration
echo "Creating fresh initial migration..."
dotnet ef migrations add InitialCreate --project ../RentCollection.Infrastructure --context ApplicationDbContext

echo "✅ Database reset complete! Run the application to apply migrations and seed data."
