# .NET Dagger Module

Reusable Dagger module for .NET development with official Microsoft container images. Supports .NET 6-10 across multiple base images (Debian, Alpine, Ubuntu, Mariner, Chiseled).

## Quick Start

```bash
# Build and publish a web app to production container
dagger call publish-asp-net-container --source=./myapp --entrypoint="MyApp.dll"

# Run tests
dagger call test --source=./myapp

# Get SDK for custom workflows
dagger call sdk --version="10.0" --base-image="alpine"
```

## Usage Examples

### Get a .NET SDK Container

```bash
# Get the latest .NET 10 SDK (Debian-based)
dagger call sdk --version="10.0"

# Get .NET 8 SDK with Alpine
dagger call sdk --version="8.0" --base-image="alpine"

# Get .NET 9 SDK with Ubuntu
dagger call sdk --version="9.0" --base-image="ubuntu"
```

### Get Runtime Containers

```bash
# Get .NET 10 runtime (Debian-based)
dagger call runtime --version="10.0"

# Get .NET 8 runtime with Alpine
dagger call runtime --version="8.0" --base-image="alpine"

# Get chiseled Ubuntu runtime (minimal, secure)
dagger call chiseled-runtime --version="10.0"
```

### Get ASP.NET Core Containers

```bash
# Get ASP.NET Core container
dagger call asp-net --version="10.0"

# Get chiseled ASP.NET Core (minimal, secure)
dagger call chiseled-asp-net --version="10.0"
```

### Build a .NET Application

```bash
# Build in Release mode (default)
dagger call build --source=./myapp

# Build specific project in Debug mode
dagger call build --source=./myapp --configuration="Debug" --project="MyApp.csproj"

# Use specific .NET version
dagger call build --source=./myapp --version="8.0"
```

### Publish a .NET Application

```bash
# Publish application
dagger call publish --source=./myapp

# Publish specific project
dagger call publish --source=./myapp --project="MyApp.csproj" --output-dir="/out"
```

### Create Production Containers

```bash
# Create production container for console app
dagger call publish-container \
  --source=./myapp \
  --entrypoint="MyApp.dll" \
  --version="10.0"

# Create production ASP.NET container
dagger call publish-asp-net-container \
  --source=./mywebapp \
  --entrypoint="MyWebApp.dll" \
  --version="10.0" \
  --port=8080

# Use chiseled Ubuntu for better security
dagger call publish-asp-net-container \
  --source=./mywebapp \
  --entrypoint="MyWebApp.dll" \
  --base-image="ubuntu-chiseled" \
  --version="10.0"
```

### Run Tests

```bash
# Run all tests in solution
dagger call test --source=./myapp

# Run specific test project
dagger call test --source=./myapp --project="MyApp.Tests/MyApp.Tests.csproj"
```

### Restore NuGet Packages

```bash
# Restore all packages
dagger call restore --source=./myapp

# Restore for specific project
dagger call restore --source=./myapp --project="MyApp.csproj"
```

### Custom Environment Variables

```bash
# Add environment variables to a container
dagger call with-environment \
  --base-container=$(dagger call sdk --version="10.0") \
  --env-vars="DOTNET_ENVIRONMENT=Production,CUSTOM_VAR=value"
```

## Function Reference

### Core Container Functions

- **`sdk(version, baseImage)`** - Get .NET SDK container
- **`runtime(version, baseImage)`** - Get .NET Runtime container
- **`aspNet(version, baseImage)`** - Get ASP.NET Core container

### Build Functions

- **`build(source, configuration, version, project)`** - Build a .NET application
- **`publish(source, configuration, version, project, outputDir)`** - Publish application artifacts
- **`restore(source, version, project)`** - Restore NuGet packages

### Production Functions

- **`publishContainer(source, entrypoint, version, baseImage, configuration, project)`** - Create production console app container
- **`publishAspNetContainer(source, entrypoint, version, baseImage, configuration, project, port)`** - Create production web app container

### Testing

- **`test(source, version, project)`** - Run unit tests

### Utility Functions

- **`withEnvironment(baseContainer, envVars)`** - Add environment variables
- **`chiseledRuntime(version)`** - Get chiseled Ubuntu runtime
- **`chiseledAspNet(version)`** - Get chiseled ASP.NET Core

## Best Practices

### Use Chiseled Images for Production

Chiseled Ubuntu images are ~100MB smaller and have fewer CVEs:

```bash
dagger call publish-asp-net-container \
  --source=./app \
  --entrypoint="App.dll" \
  --base-image="ubuntu-chiseled"
```

### Multi-Stage Builds

The `publishContainer` and `publishAspNetContainer` functions automatically implement multi-stage builds:
- Build stage uses SDK image
- Final stage uses minimal runtime/aspnet image

### Port Configuration

For .NET 8+, the default port is 8080 (not 80). Configure with:
- `--port` parameter in `publishAspNetContainer`
- `ASPNETCORE_HTTP_PORTS` environment variable
- `ASPNETCORE_HTTPS_PORTS` for HTTPS
- `ASPNETCORE_URLS` for complex scenarios

### Version Support

- .NET 6, 7, 8: LTS and Current versions
- .NET 9: Current version
- .NET 10: Preview/RC versions

## Examples

### Complete ASP.NET Core Deployment

```bash
# Build and create production container
dagger call publish-asp-net-container \
  --source=./MyWebApi \
  --entrypoint="MyWebApi.dll" \
  --version="10.0" \
  --base-image="ubuntu-chiseled" \
  --configuration="Release" \
  --port=8080

# With environment variables
dagger call publish-asp-net-container \
  --source=./MyWebApi \
  --entrypoint="MyWebApi.dll" | \
  dagger call with-environment \
    --env-vars="ASPNETCORE_ENVIRONMENT=Production,ConnectionStrings__Default=..."
```

### Console Application

```bash
# Build and create production container
dagger call publish-container \
  --source=./MyConsoleApp \
  --entrypoint="MyConsoleApp.dll" \
  --version="10.0" \
  --base-image="alpine"
```

## Integration with Other Dagger Modules

This module can be used with other Dagger modules:

```bash
# Use with a database module
dagger call publish-asp-net-container --source=./api | \
  dagger call -m github.com/other/postgres with-database
```

## References

- [.NET Docker Documentation](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [.NET Container Images](https://learn.microsoft.com/en-us/dotnet/core/docker/container-images)
- [Chiseled Ubuntu Containers](https://devblogs.microsoft.com/dotnet/announcing-dotnet-chiseled-containers/)
- [Microsoft Container Registry](https://mcr.microsoft.com/)

## License

See LICENSE file in the repository.
