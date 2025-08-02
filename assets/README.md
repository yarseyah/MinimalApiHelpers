# Package Assets

If you want to add a package icon, place a 128x128 PNG file named `icon.png` in this directory and update the project files to include:

```xml
<PackageIcon>icon.png</PackageIcon>
```

And add this to the ItemGroup:

```xml
<None Include="../assets/icon.png" Pack="true" PackagePath="\" />
```
