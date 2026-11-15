# ASP.NET Hello - Blazor with Dagger

A Blazor Web App demonstrating containerized development using Dagger and the Dagger C# SDK.

## Setup Steps

From an empty repository to a running Dagger service:

- Created Blazor Web App using `dotnet new blazor -n AspNetHello`
- Initialized Dagger: `dagger init --sdk=github.com/pjmagee/dagger/sdk/csharp@csharp/experimental --name=aspnet-hello-module`
- Installed dotnet module: `dagger install github.com/pjmagee/daggerverse/dotnet`
- Implemented Dagger functions in `Main.cs`:
  - `Build()` - Build the ASP.NET Blazor application
  - `Publish()` - Publish the application artifacts
  - `PublishContainer()` - Create production-ready container
  - `Serve()` - Run as a service with exposed port
- leverage `Dag.Dotnet()` dependency module for cleaner implementation
- Added Dagger module to solution: `dotnet sln add .dagger/AspnetHelloModule.csproj`
- Migrated to `.slnx` format: `dotnet sln migrate`
- Added comprehensive .NET gitignore: `dotnet new gitignore`

## Running the Application

Start the Blazor application with `dagger up`. `Serve` is marked `[Up]`, so no flags are needed:

```bash
dagger up
```

Or call it as a regular function and pick the ports yourself:

```bash
dagger call serve up --ports=5000:5000
```

Call the module from GitHub without cloning:

```bash
dagger -m github.com/pjmagee/daggerverse/aspnet-hello call serve up --ports=5000:5000
```

Access the application at **http://localhost:5000**

## Checks and generators

```bash
dagger check      # build-check, format-check and smoke-test
dagger generate   # format: runs dotnet format and offers to apply the changes
```

`smoke-test` starts the app and requests its home page with curl. Its curl container is a `[DefaultAddress("alpine/curl")]` argument, so it defaults to that image and can be overridden with `--curl=<image>`.

## Available Dagger Functions

```bash
dagger functions
```

The source directory defaults to this module's directory through `[DefaultPath(".")]`, with `bin`, `obj` and the vendored SDK filtered out by `[Ignore]`. Pass `--source=<dir>` to build something else, and `--version=9.0` to change the .NET version.

- **build** - Build the application with the dotnet dependency module
- **publish** - Publish the application artifacts to `/publish`
- **publish-container** - Create a production ASP.NET runtime container
- **serve** (`[Up]`) - Serve the application over HTTP (5000) and HTTPS (5001)
- **build-check** (`[Check]`) - Check that the application compiles
- **format-check** (`[Check]`) - Check that the code is formatted
- **smoke-test** (`[Check]`) - Start the application and request its home page
- **format** (`[Generate]`) - Format the code and return the changes

## Project Structure

- `AspNetHello/` - Blazor Web App (.NET 10)
- `.dagger/` - Dagger C# module
  - `Main.cs` - Module implementation with Dagger functions
  - `AspnetHelloModule.csproj` - Module project file
- `aspnet-hello.slnx` - Solution file (XML format)
- `dagger.json` - Dagger module configuration
