# MinVer Setup Guide

This guide explains how the MinimalApiHelpers solution uses MinVer for automatic git-based versioning.

## What is MinVer?

MinVer is a minimalist .NET package that provides automatic versioning using Git tags. Unlike GitVersion, MinVer is:

- **Simple**: No configuration files needed - works purely with git tags
- **Reliable**: No complex branch-based logic that can fail
- **Fast**: Minimal overhead during builds
- **Tag-based**: Version determined purely by semantic version tags

## How It Works

### Basic Workflow

1. **Development**: Make commits, MinVer calculates pre-release versions automatically
2. **Release**: Create a semantic version tag (e.g., `git tag 1.0.0`)
3. **Build**: MinVer uses the tag for the exact version
4. **Continue**: Future commits increment patch with pre-release suffix

### Version Examples

| Scenario | Git State | Generated Version |
|----------|-----------|-------------------|
| No tags | Any commit | `0.0.0-alpha.0.5` (with height) |
| Tagged commit | `git tag 1.0.0` | `1.0.0` |
| After tag | 3 commits after 1.0.0 | `1.0.1-alpha.0.3` |
| Pre-release tag | `git tag 2.0.0-beta.1` | `2.0.0-beta.1` |
| After pre-release | 2 commits after beta | `2.0.0-beta.1.2` |

## Configuration

### Package References

Both projects include MinVer:

```xml
<ItemGroup>
  <PackageReference Include="MinVer" PrivateAssets="All" />
</ItemGroup>
```

The `PrivateAssets="All"` ensures MinVer is only used during build and not included as a dependency.

### Central Package Management

MinVer version is managed in `Directory.Packages.props`:

```xml
<ItemGroup>
  <PackageVersion Include="MinVer" Version="6.0.0" />
</ItemGroup>
```

### No Configuration Files

Unlike GitVersion, MinVer requires **no configuration files**:
- No `GitVersion.yml` needed
- No complex branch strategies
- No YAML syntax to debug

## Release Process

### 1. Development Workflow

During development, MinVer automatically generates pre-release versions:

```bash
# Make changes and commit
git add .
git commit -m "Add new feature"

# Build - gets version like 1.0.1-alpha.0.5
dotnet pack --configuration Release --output ./artifacts
```

### 2. Creating a Release

When ready for a release, simply tag the commit:

```bash
# Tag the current commit
git tag 1.1.0
git push --tags

# Build - gets exactly 1.1.0
dotnet pack --configuration Release --output ./artifacts
```

### 3. Pre-releases

For pre-releases, use semantic versioning pre-release syntax:

```bash
# Create pre-release tags
git tag 2.0.0-alpha.1
git tag 2.0.0-beta.1
git tag 2.0.0-rc.1

# Each will use the exact tag version
```

## GitHub Actions Integration

The workflow is simplified with MinVer - no manual version extraction needed:

```yaml
steps:
- name: Checkout code
  uses: actions/checkout@v4
  with:
    fetch-depth: 0 # Required for MinVer to access git history

- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.0.x'

- name: Build
  run: dotnet build --configuration Release

- name: Pack
  run: dotnet pack --configuration Release --output ./artifacts
```

MinVer automatically:
- Calculates the correct version from git history
- Sets all MSBuild version properties
- Works with any git branching strategy

## Advanced Configuration

### Custom Pre-release Identifiers

Change default pre-release suffix from `alpha.0`:

```xml
<PropertyGroup>
  <MinVerDefaultPreReleaseIdentifiers>preview.0</MinVerDefaultPreReleaseIdentifiers>
</PropertyGroup>
```

### Auto-increment Behavior

Control which version part gets incremented after RTM tags:

```xml
<PropertyGroup>
  <MinVerAutoIncrement>minor</MinVerAutoIncrement> <!-- or "major" -->
</PropertyGroup>
```

### Minimum Version

Set a minimum version for the current branch:

```xml
<PropertyGroup>
  <MinVerMinimumMajorMinor>1.0</MinVerMinimumMajorMinor>
</PropertyGroup>
```

### Tag Prefixes

Use prefixes for multiple projects in one repo:

```xml
<PropertyGroup>
  <MinVerTagPrefix>helpers-</MinVerTagPrefix>
</PropertyGroup>
```

Then use tags like: `helpers-1.0.0`

### Ignore Height

For multiple projects, ignore commit height:

```xml
<PropertyGroup>
  <MinVerIgnoreHeight>true</MinVerIgnoreHeight>
</PropertyGroup>
```

## Troubleshooting

### Common Issues

**Issue**: Getting `0.0.0-alpha.0` versions
- **Cause**: No git tags found in history
- **Solution**: Create your first tag: `git tag 0.1.0`

**Issue**: Version not updating in CI
- **Cause**: Shallow git clone
- **Solution**: Use `fetch-depth: 0` in GitHub Actions

**Issue**: Wrong version calculated
- **Cause**: Multiple tags on same commit or unexpected tags
- **Solution**: Check tags with `git tag -l` and remove unwanted ones

### Debugging

Enable verbose MinVer logging:

```bash
dotnet build -p:MinVerVerbosity=diagnostic
```

This shows exactly how MinVer walks the git history and calculates versions.

### Version Properties

MinVer sets these MSBuild properties automatically:

- `MinVerVersion` - Full calculated version
- `MinVerMajor`, `MinVerMinor`, `MinVerPatch` - Version components  
- `MinVerPreRelease` - Pre-release suffix
- `Version` - Used by MSBuild for assembly version
- `PackageVersion` - Used for NuGet package version

## Comparison with GitVersion

| Feature | MinVer | GitVersion |
|---------|--------|------------|
| Configuration | None required | Requires GitVersion.yml |
| Complexity | Simple tag-based | Complex branch strategies |
| Build failures | Rare | Common with config issues |
| Performance | Fast | Slower due to complexity |
| Branching | Any strategy works | Specific strategies required |

## Migration from GitVersion

If migrating from GitVersion:

1. Remove `GitVersion.MsBuild` package references
2. Remove `GitVersion.yml` configuration file
3. Add `MinVer` package references with `PrivateAssets="All"`
4. Remove manual version extraction from CI workflows
5. Create semantic version tags for releases

The solution is now using MinVer and provides much more reliable versioning!
