# NuGet Package Configuration Guide

This document describes how the MinimalApiHelpers solution is configured to generate and publish NuGet packages.

## Package Structure

- **MinimalApiHelpers** - Core package with timing filters, exception handling, and endpoint extensions
- **MinimalApiHelpers.FluentValidation** - FluentValidation integration (depends on MinimalApiHelpers)

## Configuration Files

### Directory.Build.props
Contains common MSBuild properties for all projects including:
- Common package metadata (author, license, repository URL)
- NuGet package generation settings
- Symbol package configuration
- Deterministic build settings

### Individual Project Files
Each project (`.csproj`) contains:
- Package-specific metadata (PackageId, Title, Description, Tags)
- Package version
- Dependencies and references

## Local Development

### Building Packages Locally
```bash
# Use the build script
./scripts/build-packages.sh

# Or manually
dotnet pack --configuration Release --output ./artifacts
```

### Testing Packages Locally
```bash
# Add local package source
dotnet nuget add source $(pwd)/artifacts --name local

# Use in a test project
dotnet add package MinimalApiHelpers --source local
dotnet add package MinimalApiHelpers.FluentValidation --source local
```

## Versioning

Currently using manual versioning:
- Version is specified in each project file as `<PackageVersion>`
- For releases, update version in both project files and create a git tag

### Future: Automatic Semantic Versioning with GitVersion

The solution is prepared for GitVersion integration:
1. Uncomment GitVersion package references in project files
2. Configure GitVersion.yml for your branching strategy
3. Update GitHub Actions workflow to use GitVersion

## Automated Publishing

### GitHub Actions Workflow
The CI/CD pipeline (`.github/workflows/ci-cd.yml`) handles:

1. **Build and Test** (on every push/PR):
   - Restores dependencies
   - Builds solution
   - Runs tests
   - Creates NuGet packages
   - Uploads artifacts

2. **Publish to NuGet.org** (on GitHub releases):
   - Downloads build artifacts
   - Pushes packages to NuGet.org using `NUGET_API_KEY` secret

3. **Publish to GitHub Packages** (on main branch and releases):
   - Pushes packages to GitHub Packages using `GITHUB_TOKEN`

### Required Secrets

Set these in your GitHub repository settings:

- `NUGET_API_KEY` - Your NuGet.org API key for publishing packages

### Publishing a Release

1. **Manual Method**:
   ```bash
   # Update versions and create release
   ./scripts/release.sh 1.0.1
   
   # Then create a GitHub release from the tag
   ```

2. **GitHub UI Method**:
   - Create a new release in GitHub
   - Choose or create a tag (e.g., `v1.0.1`)
   - This will trigger the publish workflow

## Package Metadata

### Update These Fields in Directory.Build.props:
- `<Authors>` - Your name
- `<Company>` - Your company name
- `<PackageProjectUrl>` - Your GitHub repository URL
- `<RepositoryUrl>` - Your GitHub repository URL

### Optional Enhancements:
- Add package icon: Place `icon.png` in `assets/` folder and reference it
- Add license file: Reference `LICENSE` file in package
- Add release notes: Link to GitHub releases

## Best Practices

1. **Version Management**:
   - Use semantic versioning (Major.Minor.Patch)
   - Tag releases consistently (v1.0.0)
   - Update version in both project files

2. **Documentation**:
   - Keep README.md updated with usage examples
   - Document breaking changes in release notes
   - Include XML documentation comments in code

3. **Testing**:
   - Test packages locally before releasing
   - Validate package contents using `dotnet nuget verify`
   - Check package size and dependencies

4. **Security**:
   - Regularly update dependencies
   - Enable NuGet package auditing (already configured)
   - Review security advisories

## Troubleshooting

### Common Issues:

1. **GitVersion Errors**: 
   - Ensure git repository has at least one commit
   - Check GitVersion.yml syntax
   - Try running `dotnet-gitversion --diag` for details

2. **Package Not Found**:
   - Check package source configuration
   - Verify package was uploaded successfully
   - Wait for NuGet.org indexing (can take 10-15 minutes)

3. **Build Failures**:
   - Check target framework compatibility
   - Verify all dependencies are available
   - Review MSBuild output for specific errors

## References

- [NuGet Package Creation Guide](https://docs.microsoft.com/en-us/nuget/create-packages/creating-a-package-msbuild)
- [GitVersion Documentation](https://gitversion.net/)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net)
