using Dagger;

namespace Dotnet;

/// <summary>
/// A Dagger module for .NET development with support for SDK, Runtime, and ASP.NET Core containers
///
/// This module provides functions to work with official Microsoft .NET container images,
/// supporting various base images (Alpine, Ubuntu, Debian, Mariner), versions, and customizations.
///
/// Key features:
/// - Multiple .NET versions (6, 7, 8, 9, 10)
/// - SDK containers for building
/// - Runtime containers for production
/// - ASP.NET Core containers for web applications
/// - Support for different base OS variants (Alpine, Ubuntu, Debian, Mariner, Chiseled)
/// - Customizable environment variables and configurations
/// </summary>
[Object]
public class Dotnet
{
    /// <summary>
    /// Returns a .NET SDK container for building applications
    /// </summary>
    /// <param name="version">The .NET version (e.g., "8.0", "9.0", "10.0"). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (debian, alpine, ubuntu, mariner). Default: "debian"</param>
    [Function]
    public Container Sdk(string version = "10.0", string baseImage = "debian")
    {
        var tag =
            string.IsNullOrEmpty(baseImage) || baseImage == "debian"
                ? version
                : $"{version}-{baseImage}";

        return Dag.Container().From($"mcr.microsoft.com/dotnet/sdk:{tag}");
    }

    /// <summary>
    /// Returns a .NET Runtime container for running applications
    /// </summary>
    /// <param name="version">The .NET version (e.g., "8.0", "9.0", "10.0"). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (debian, alpine, ubuntu, mariner, ubuntu-chiseled). Default: "debian"</param>
    [Function]
    public Container Runtime(string version = "10.0", string baseImage = "debian")
    {
        var tag =
            string.IsNullOrEmpty(baseImage) || baseImage == "debian"
                ? version
                : $"{version}-{baseImage}";

        return Dag.Container().From($"mcr.microsoft.com/dotnet/runtime:{tag}");
    }

    /// <summary>
    /// Returns an ASP.NET Core container for web applications
    /// </summary>
    /// <param name="version">The .NET version (e.g., "8.0", "9.0", "10.0"). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (debian, alpine, ubuntu, mariner, ubuntu-chiseled). Default: "debian"</param>
    [Function]
    public Container AspNet(string version = "10.0", string baseImage = "debian")
    {
        var tag =
            string.IsNullOrEmpty(baseImage) || baseImage == "debian"
                ? version
                : $"{version}-{baseImage}";

        return Dag.Container().From($"mcr.microsoft.com/dotnet/aspnet:{tag}");
    }

    /// <summary>
    /// Build a .NET application from source
    /// </summary>
    /// <param name="source">Directory containing the .NET source code</param>
    /// <param name="configuration">Build configuration (Debug or Release). Default: "Release"</param>
    /// <param name="version">The .NET SDK version to use. Default: "10.0"</param>
    /// <param name="project">Optional path to specific project file (relative to source)</param>
    [Function]
    public Container Build(
        Directory source,
        string configuration = "Release",
        string version = "10.0",
        string? project = null
    )
    {
        var container = Sdk(version).WithMountedDirectory("/src", source).WithWorkdir("/src");

        var buildArgs = new List<string> { "dotnet", "build" };

        if (!string.IsNullOrEmpty(project))
        {
            buildArgs.Add(project);
        }

        buildArgs.AddRange(["-c", configuration]);

        return container.WithExec([.. buildArgs]);
    }

    /// <summary>
    /// Publish a .NET application ready for deployment
    /// </summary>
    /// <param name="source">Directory containing the .NET source code</param>
    /// <param name="configuration">Build configuration (Debug or Release). Default: "Release"</param>
    /// <param name="version">The .NET SDK version to use. Default: "10.0"</param>
    /// <param name="project">Optional path to specific project file (relative to source)</param>
    /// <param name="outputDir">Output directory for published artifacts. Default: "/publish"</param>
    [Function]
    public Container Publish(
        Directory source,
        string configuration = "Release",
        string version = "10.0",
        string? project = null,
        string outputDir = "/publish"
    )
    {
        var container = Sdk(version).WithMountedDirectory("/src", source).WithWorkdir("/src");

        var publishArgs = new List<string> { "dotnet", "publish" };

        if (!string.IsNullOrEmpty(project))
        {
            publishArgs.Add(project);
        }

        publishArgs.AddRange(["-c", configuration, "-o", outputDir]);

        return container.WithExec([.. publishArgs]);
    }

    /// <summary>
    /// Create a production-ready .NET application container
    /// </summary>
    /// <param name="source">Directory containing the .NET source code</param>
    /// <param name="entrypoint">The name of the DLL to run (e.g., "MyApp.dll")</param>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant. Default: "debian"</param>
    /// <param name="configuration">Build configuration. Default: "Release"</param>
    /// <param name="project">Optional path to specific project file</param>
    [Function]
    public Container PublishContainer(
        Directory source,
        string entrypoint,
        string version = "10.0",
        string baseImage = "debian",
        string configuration = "Release",
        string? project = null
    )
    {
        // Build stage
        var buildContainer = Publish(source, configuration, version, project, "/publish");
        var publishDir = buildContainer.Directory("/publish");

        // Runtime stage
        var runtimeContainer = Runtime(version, baseImage)
            .WithDirectory("/app", publishDir)
            .WithWorkdir("/app")
            .WithEntrypoint(["dotnet", entrypoint]);

        return runtimeContainer;
    }

    /// <summary>
    /// Create a production-ready ASP.NET Core application container
    /// </summary>
    /// <param name="source">Directory containing the ASP.NET Core source code</param>
    /// <param name="entrypoint">The name of the DLL to run (e.g., "MyWebApp.dll")</param>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant. Default: "debian"</param>
    /// <param name="configuration">Build configuration. Default: "Release"</param>
    /// <param name="project">Optional path to specific project file</param>
    /// <param name="port">The HTTP port to expose. Default: 8080 (standard for .NET 8+)</param>
    [Function]
    public Container PublishAspNetContainer(
        Directory source,
        string entrypoint,
        string version = "10.0",
        string baseImage = "debian",
        string configuration = "Release",
        string? project = null,
        int port = 8080
    )
    {
        // Build stage
        var buildContainer = Publish(source, configuration, version, project, "/publish");
        var publishDir = buildContainer.Directory("/publish");

        // Runtime stage
        var runtimeContainer = AspNet(version, baseImage)
            .WithDirectory("/app", publishDir)
            .WithWorkdir("/app")
            .WithEnvVariable("ASPNETCORE_HTTP_PORTS", port.ToString())
            .WithExposedPort(port)
            .WithEntrypoint(["dotnet", entrypoint]);

        return runtimeContainer;
    }

    /// <summary>
    /// Run .NET tests in a container
    /// </summary>
    /// <param name="source">Directory containing the .NET source code</param>
    /// <param name="version">The .NET SDK version to use. Default: "10.0"</param>
    /// <param name="project">Optional path to specific test project file</param>
    [Function]
    public async Task<string> Test(
        Directory source,
        string version = "10.0",
        string? project = null
    )
    {
        var container = Sdk(version).WithMountedDirectory("/src", source).WithWorkdir("/src");

        var testArgs = new List<string> { "dotnet", "test" };

        if (!string.IsNullOrEmpty(project))
        {
            testArgs.Add(project);
        }

        return await container.WithExec(testArgs.ToArray()).StdoutAsync();
    }

    /// <summary>
    /// Restore NuGet packages for a .NET project
    /// </summary>
    /// <param name="source">Directory containing the .NET source code</param>
    /// <param name="version">The .NET SDK version to use. Default: "10.0"</param>
    /// <param name="project">Optional path to specific project file</param>
    [Function]
    public Container Restore(Directory source, string version = "10.0", string? project = null)
    {
        var container = Sdk(version).WithMountedDirectory("/src", source).WithWorkdir("/src");

        var restoreArgs = new List<string> { "dotnet", "restore" };

        if (!string.IsNullOrEmpty(project))
        {
            restoreArgs.Add(project);
        }

        return container.WithExec([.. restoreArgs]);
    }

    /// <summary>
    /// Create a custom .NET container with environment variables
    /// </summary>
    /// <param name="baseContainer">The base container to customize</param>
    /// <param name="envVars">Environment variables as key=value pairs</param>
    [Function]
    public Container WithEnvironment(Container baseContainer, string[] envVars)
    {
        var container = baseContainer;

        foreach (var envVar in envVars)
        {
            var parts = envVar.Split('=', 2);
            if (parts.Length == 2)
            {
                container = container.WithEnvVariable(parts[0], parts[1]);
            }
        }

        return container;
    }

    /// <summary>
    /// Get a chiseled Ubuntu .NET runtime container (minimal, secure, non-root)
    /// </summary>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    [Function]
    public Container ChiseledRuntime(string version = "10.0")
    {
        return Runtime(version, "ubuntu-chiseled");
    }

    /// <summary>
    /// Get a chiseled Ubuntu ASP.NET Core container (minimal, secure, non-root)
    /// </summary>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    [Function]
    public Container ChiseledAspNet(string version = "10.0")
    {
        return AspNet(version, "ubuntu-chiseled");
    }
}
