#!/bin/bash

# Build and pack script for MinimalApiHelpers
set -e

echo "🔧 Building MinimalApiHelpers packages..."

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean --configuration Release

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build solution
echo "🏗️  Building solution..."
dotnet build --configuration Release --no-restore

# Pack NuGet packages
echo "📦 Packing NuGet packages..."
rm -rf ./artifacts
mkdir -p ./artifacts
dotnet pack --configuration Release --no-build --output ./artifacts

echo "✅ Packages created successfully!"
echo "📁 Packages location: ./artifacts/"
ls -la ./artifacts/

echo ""
echo "To test locally:"
echo "  dotnet nuget add source $(pwd)/artifacts --name local"
echo "  dotnet add package MinimalApiHelpers --source local"
echo ""
echo "To publish to NuGet.org (requires API key):"
echo "  dotnet nuget push ./artifacts/*.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY"
