#!/bin/bash

# Release script for MinimalApiHelpers
set -e

if [ $# -eq 0 ]; then
    echo "Usage: $0 <version>"
    echo "Example: $0 1.0.1"
    exit 1
fi

VERSION=$1

echo "🚀 Preparing release $VERSION..."

# Update version in project files
echo "📝 Updating version in project files..."
sed -i '' "s/<PackageVersion>.*<\/PackageVersion>/<PackageVersion>$VERSION<\/PackageVersion>/g" MinimalApiHelpers/MinimalApiHelpers.csproj
sed -i '' "s/<PackageVersion>.*<\/PackageVersion>/<PackageVersion>$VERSION<\/PackageVersion>/g" MinimalApiHelpers.FluentValidation/MinimalApiHelpers.FluentValidation.csproj

# Commit version changes
git add MinimalApiHelpers/MinimalApiHelpers.csproj MinimalApiHelpers.FluentValidation/MinimalApiHelpers.FluentValidation.csproj
git commit -m "chore: bump version to $VERSION"

# Create and push tag
git tag -a "v$VERSION" -m "Release v$VERSION"
git push origin main
git push origin "v$VERSION"

echo "✅ Release $VERSION prepared!"
echo "🎯 Tag v$VERSION created and pushed"
echo "📦 GitHub Actions will automatically build and publish the packages"
