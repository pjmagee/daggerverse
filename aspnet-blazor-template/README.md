# ASP.NET Hello Template - On-the-Fly Scaffolding with Dagger

A Dagger module that dynamically generates ASP.NET Blazor applications using `dotnet new blazor` at runtime. **No source code is stored in the repository** - everything is scaffolded on-the-fly through Dagger functions.

## How It Works

The `Scaffold()` function uses the .NET SDK container to run `dotnet new blazor` and returns the generated directory. All other functions (`Build`, `Publish`, `PublishContainer`, `Serve`) first scaffold the template, then pass it to the `Dag.Dotnet()` module for compilation and deployment.

## Running the Application

### Serve Locally

Start the dynamically-scaffolded Blazor application as a Dagger service on port 5000:

```bash
dagger call serve up --ports=5000:5000
```

Access the application at **http://localhost:5000**

### Remote Module Usage

Call the module from GitHub without cloning:

```bash
dagger -m github.com/pjmagee/daggerverse/aspnet-hello-template call serve up --ports=5000:5000
```

## Available Dagger Functions

```bash
dagger functions
```

### Core Functions

- **scaffold** - Generate a fresh ASP.NET Blazor template using `dotnet new blazor`
- **build** - Scaffold and build the Blazor application
- **publish** - Scaffold and publish the application artifacts
- **publish-container** - Scaffold and create a production-ready container
- **serve** - Scaffold and serve the application as a Dagger service
- **source** - Export the scaffolded source code for inspection

## Usage Examples

### Custom Project Name

```bash
# Scaffold a project with a custom name
dagger call scaffold --project-name="MyBlazorApp"

# Build with custom name
dagger call build --project-name="MyBlazorApp"

# Serve with custom name and port
dagger call serve --project-name="MyBlazorApp" --port=8080 up --ports=8080:8080
```

### Different .NET Versions

```bash
# Use .NET 8.0
dagger call serve --version="8.0" up --ports=5000:5000

# Use .NET 9.0
dagger call serve --version="9.0" up --ports=5000:5000
```

### Export Scaffolded Source

```bash
# Export the generated template to local directory
dagger call source export --path=./generated-app

# Inspect what gets generated
dagger call scaffold directory --path=.
```

### Build Container Image

```bash
# Create a production container
dagger call publish-container

# With custom settings
dagger call publish-container \
  --project-name="MyApp" \
  --port=8080 \
  --version="10.0" \
  --base-image="alpine"
```

## Module Dependencies

This module depends on:

- **dotnet** - [`github.com/pjmagee/daggerverse/dotnet`](../dotnet) for SDK/runtime containers and build operations

## Key Differences from aspnet-hello

| Aspect | aspnet-hello | aspnet-hello-template |
|--------|-------------|---------------------|
| Source Code | Checked into git | Generated on-the-fly |
| Repository Size | Includes full Blazor project | Only Dagger module code |
| Flexibility | Fixed template | Dynamic, can customize project name |
| Use Case | Production application | Template demonstration/testing |
| Maintenance | Manual template updates | Always latest .NET template |

## Project Structure

```
aspnet-hello-template/
├── dagger.json          # Dagger module configuration
├── Main.cs              # Module implementation with Scaffold() function
├── LICENSE              # Apache 2.0 license
├── README.md            # This file
└── .gitignore           # Excludes everything except module files
```

**No AspNetHello/ directory** - everything is generated at runtime! 🚀

## Implementation Details

The module leverages:

1. **`Dag.Dotnet().Sdk()`** - Provides .NET SDK container with `dotnet new` command
2. **`WithExec(["dotnet", "new", "blazor"])`** - Scaffolds the Blazor template
3. **`Directory()` extraction** - Captures the generated project directory
4. **`Dag.Dotnet()` chaining** - Passes scaffolded code to build/publish functions

This approach demonstrates how Dagger can create ephemeral, reproducible build environments without storing intermediate artifacts in version control.

## Why This Approach?

- **Always Fresh** - Uses the latest template from the .NET SDK version
- **Minimal Repository** - No bloat from boilerplate code
- **Portable** - Works identically across all environments
- **Educational** - Shows Dagger's ability to compose dynamic workflows
- **Testable** - Can easily test different .NET versions and template variations

## License

Apache License 2.0 - See [LICENSE](LICENSE) file for details.
