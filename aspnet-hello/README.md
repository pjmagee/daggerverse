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

### Local Development

Start the Blazor application as a Dagger service on port 5000:

```bash
dagger call serve up --ports=5000:5000
```

### Remote Module Usage

Call the module from GitHub without cloning:

```bash
dagger -m github.com/pjmagee/daggerverse/aspnet-hello call serve up --ports=5000:5000
```

Access the application at **http://localhost:5000**

## Available Dagger Functions

```bash
dagger functions
```

- **build** - Build the ASP.NET Blazor application using the dotnet dependency module
- **publish** - Publish the ASP.NET Blazor application using the dotnet dependency module  
- **publish-container** - Create a production-ready ASP.NET container using the dotnet dependency module
- **serve** - Serve the ASP.NET Blazor application as a Dagger service on the specified port

## Project Structure

- `AspNetHello/` - Blazor Web App (.NET 10)
- `.dagger/` - Dagger C# module
  - `Main.cs` - Module implementation with Dagger functions
  - `AspnetHelloModule.csproj` - Module project file
- `aspnet-hello.slnx` - Solution file (XML format)
- `dagger.json` - Dagger module configuration
