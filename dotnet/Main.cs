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
/// - SDK containers for building (including Native AOT SDK)
/// - Runtime containers for production
/// - Runtime-deps containers for Native AOT and self-contained apps
/// - ASP.NET Core containers for web applications (including composite images)
/// - Support for different base OS variants (Alpine, Ubuntu, Debian, Mariner, Chiseled, Distroless)
/// - Native AOT compilation and deployment
/// - Non-root user support for secure deployments
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
    /// Returns a runtime-deps container with only OS dependencies (no .NET runtime)
    /// Ideal for Native AOT and self-contained deployments
    /// </summary>
    /// <param name="version">The .NET version (e.g., "8.0", "9.0", "10.0"). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (debian, alpine, ubuntu, mariner, jammy-chiseled). Default: "debian"</param>
    [Function]
    public Container RuntimeDeps(string version = "10.0", string baseImage = "debian")
    {
        var tag =
            string.IsNullOrEmpty(baseImage) || baseImage == "debian"
                ? version
                : $"{version}-{baseImage}";

        return Dag.Container().From($"mcr.microsoft.com/dotnet/runtime-deps:{tag}");
    }

    /// <summary>
    /// Returns a runtime-deps container optimized for Native AOT (minimal, no globalization)
    /// Available for Alpine, Mariner, and Ubuntu Chiseled
    /// </summary>
    /// <param name="version">The .NET version (e.g., "8.0", "9.0", "10.0"). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (alpine, mariner-distroless, jammy-chiseled). Default: "jammy-chiseled"</param>
    [Function]
    public Container RuntimeDepsAot(string version = "10.0", string baseImage = "jammy-chiseled")
    {
        var tag = $"{version}-{baseImage}-aot";
        return Dag.Container().From($"mcr.microsoft.com/dotnet/runtime-deps:{tag}");
    }

    /// <summary>
    /// Returns a runtime-deps container with globalization support (tzdata, ICU, stdc++)
    /// For Native AOT and Core CLR apps requiring globalization
    /// </summary>
    /// <param name="version">The .NET version (e.g., "8.0", "9.0", "10.0"). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (alpine, mariner-distroless, jammy-chiseled). Default: "jammy-chiseled"</param>
    [Function]
    public Container RuntimeDepsExtra(string version = "10.0", string baseImage = "jammy-chiseled")
    {
        var tag = $"{version}-{baseImage}-extra";
        return Dag.Container().From($"mcr.microsoft.com/dotnet/runtime-deps:{tag}");
    }

    /// <summary>
    /// Returns a .NET SDK container with Native AOT tooling for building Native AOT applications
    /// These images are larger (typically 2x SDK size) but include required AOT compilation tools
    /// </summary>
    /// <param name="version">The .NET version (e.g., "8.0", "9.0", "10.0"). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (alpine, mariner, ubuntu). Default: "ubuntu"</param>
    [Function]
    public Container AotSdk(string version = "10.0", string baseImage = "ubuntu")
    {
        var tag = $"{version}-{baseImage}-aot";
        return Dag.Container().From($"mcr.microsoft.com/dotnet/sdk:{tag}");
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
    /// Publish a .NET application as Native AOT for minimal size and fast startup
    /// </summary>
    /// <param name="source">Directory containing the .NET source code</param>
    /// <param name="rid">Runtime identifier (e.g., linux-x64, linux-arm64, win-x64). Required.</param>
    /// <param name="configuration">Build configuration. Default: "Release"</param>
    /// <param name="version">The .NET SDK version to use. Default: "10.0"</param>
    /// <param name="baseImage">The AOT SDK base image variant (alpine, mariner, ubuntu). Default: "ubuntu"</param>
    /// <param name="project">Optional path to specific project file (relative to source)</param>
    /// <param name="outputDir">Output directory for published artifacts. Default: "/publish"</param>
    [Function]
    public Container PublishNativeAot(
        Directory source,
        string rid,
        string configuration = "Release",
        string version = "10.0",
        string baseImage = "ubuntu",
        string? project = null,
        string outputDir = "/publish"
    )
    {
        var container = AotSdk(version, baseImage)
            .WithMountedDirectory("/src", source)
            .WithWorkdir("/src");

        var publishArgs = new List<string> { "dotnet", "publish" };

        if (!string.IsNullOrEmpty(project))
        {
            publishArgs.Add(project);
        }

        publishArgs.AddRange(["-c", configuration, "-r", rid, "-o", outputDir]);

        return container.WithExec([.. publishArgs]);
    }

    /// <summary>
    /// Create a production-ready Native AOT application container
    /// </summary>
    /// <param name="source">Directory containing the .NET source code</param>
    /// <param name="entrypoint">The name of the executable to run (e.g., "MyApp" or "./MyApp")</param>
    /// <param name="rid">Runtime identifier (e.g., linux-x64, linux-arm64)</param>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    /// <param name="baseImage">The runtime-deps base image variant (alpine, mariner-distroless, jammy-chiseled). Default: "jammy-chiseled"</param>
    /// <param name="useAotVariant">Use the -aot optimized runtime-deps image. Default: true</param>
    /// <param name="configuration">Build configuration. Default: "Release"</param>
    /// <param name="project">Optional path to specific project file</param>
    [Function]
    public Container PublishNativeAotContainer(
        Directory source,
        string entrypoint,
        string rid,
        string version = "10.0",
        string baseImage = "jammy-chiseled",
        bool useAotVariant = true,
        string configuration = "Release",
        string? project = null
    )
    {
        // Determine the SDK base image from RID
        var sdkBaseImage = rid.StartsWith("linux") ? "ubuntu" : "ubuntu";

        // Build stage
        var buildContainer = PublishNativeAot(
            source,
            rid,
            configuration,
            version,
            sdkBaseImage,
            project,
            "/publish"
        );
        var publishDir = buildContainer.Directory("/publish");

        // Runtime stage - choose appropriate runtime-deps
        var runtimeContainer = useAotVariant
            ? RuntimeDepsAot(version, baseImage)
            : RuntimeDeps(version, baseImage);

        runtimeContainer = runtimeContainer
            .WithDirectory("/app", publishDir)
            .WithWorkdir("/app")
            .WithEntrypoint([entrypoint.StartsWith("./") ? entrypoint : $"./{entrypoint}"]);

        return runtimeContainer;
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

    /// <summary>
    /// Get an ASP.NET Core composite container with pre-compiled R2R assemblies
    /// Composite images offer faster startup and smaller size. Available for Alpine, Ubuntu Chiseled, and Mariner Distroless
    /// </summary>
    /// <param name="version">The .NET version (8.0+). Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant (alpine, jammy-chiseled, mariner-distroless). Default: "jammy-chiseled"</param>
    [Function]
    public Container AspNetComposite(string version = "10.0", string baseImage = "jammy-chiseled")
    {
        var tag = $"{version}-{baseImage}-composite";
        return Dag.Container().From($"mcr.microsoft.com/dotnet/aspnet:{tag}");
    }

    /// <summary>
    /// Get a Mariner distroless .NET runtime container (minimal, no shell/package manager)
    /// </summary>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    [Function]
    public Container DistrolessRuntime(string version = "10.0")
    {
        return Runtime(version, "mariner-distroless");
    }

    /// <summary>
    /// Get a Mariner distroless ASP.NET Core container (minimal, no shell/package manager)
    /// </summary>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    [Function]
    public Container DistrolessAspNet(string version = "10.0")
    {
        return AspNet(version, "mariner-distroless");
    }

    /// <summary>
    /// Get a chiseled Ubuntu runtime-deps container with globalization support (extra variant)
    /// Includes tzdata, ICU, and stdc++ for apps requiring globalization
    /// </summary>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    [Function]
    public Container ChiseledExtra(string version = "10.0")
    {
        return RuntimeDepsExtra(version, "jammy-chiseled");
    }

    /// <summary>
    /// Configure a container to run as the non-root 'app' user (UID 1654)
    /// Recommended for security in production deployments. Requires app to run on port 8080 or higher
    /// </summary>
    /// <param name="baseContainer">The container to configure</param>
    [Function]
    public Container WithNonRootUser(Container baseContainer)
    {
        return baseContainer.WithUser("app");
    }

    /// <summary>
    /// Configure a container to run as the non-root 'app' user by UID (1654)
    /// Useful for Kubernetes runAsNonRoot test which requires UID-based user specification
    /// </summary>
    /// <param name="baseContainer">The container to configure</param>
    [Function]
    public Container WithNonRootUserUid(Container baseContainer)
    {
        return baseContainer.WithUser("1654");
    }
}
