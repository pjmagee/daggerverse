using Dagger;

/// <summary>
/// A Dagger module that dynamically generates and serves ASP.NET Blazor applications on-the-fly
/// 
/// This module uses 'dotnet new blazor' to scaffold templates at runtime instead of storing source code.
/// All application code is generated dynamically through Dagger functions, keeping the repository clean
/// with only the Dagger module code.
/// </summary>
[Object]
public class AspnetBlazorTemplateModule
{    
    [Field(Description = "The name of the project to generate")]
    public string ProjectName { get; set; }
 
    [Field(Description = "The .NET SDK version to use")]
    public string Version { get; set; }

    [Field(Description = "The HTTP port to expose")]
    public int Port { get; set; }

    [Field(Description = "The base OS image variant")]
    public string BaseImage { get; set; }

    [Field(Description = "The build configuration (e.g., Debug or Release)")]
    public string Configuration { get; set; }

    public AspnetBlazorTemplateModule(
        string projectName = "AspNetBlazorTemplate",
        string version = "10.0",
        int port = 5000,
        string baseImage = "debian",
        string configuration = "Release")
    {
        this.ProjectName = projectName;
        this.Version = version;
        this.Port = port;
        this.BaseImage = baseImage;
        this.Configuration = configuration;
    }

    /// <summary>
    /// Scaffold a new ASP.NET Blazor application from the default template
    /// </summary>
    /// <param name="projectName">The name of the project to generate. Uses constructor default if not specified.</param>
    /// <param name="version">The .NET SDK version to use. Uses constructor default if not specified.</param>
    /// <returns>Directory containing the scaffolded Blazor application</returns>
    [Function]
    public Directory Scaffold(string? projectName = null, string? version = null)
    {
        var name = projectName ?? this.ProjectName;
        var ver = version ?? this.Version;

        return Dag.Dotnet()
            .Sdk(ver)
            .WithWorkdir("/workspace")
            .WithExec(["dotnet", "new", "blazor", "-n", name])
            .Directory("/workspace");
    }

    /// <summary>
    /// Build the ASP.NET Blazor application
    /// </summary>
    /// <param name="projectName">The name of the project. Uses constructor default if not specified.</param>
    /// <param name="version">The .NET version. Uses constructor default if not specified.</param>
    /// <param name="configuration">Build configuration (Debug or Release). Uses constructor default if not specified.</param>
    [Function]
    public Container Build(
        string? projectName = null,
        string? version = null,
        string? configuration = null)
    {
        var name = projectName ?? this.ProjectName;
        var ver = version ?? this.Version;
        var config = configuration ?? this.Configuration;
        var source = Scaffold(name, ver);
        var projectPath = $"{name}/{name}.csproj";

        return Dag.Dotnet()
            .Build(source, config, ver, projectPath);
    }

    /// <summary>
    /// Publish the ASP.NET Blazor application
    /// </summary>
    /// <param name="projectName">The name of the project. Uses constructor default if not specified.</param>
    /// <param name="version">The .NET version. Uses constructor default if not specified.</param>
    /// <param name="configuration">Build configuration. Uses constructor default if not specified.</param>
    [Function]
    public Container Publish(
        string? projectName = null,
        string? version = null,
        string? configuration = null)
    {
        var name = projectName ?? this.ProjectName;
        var ver = version ?? this.Version;
        var config = configuration ?? this.Configuration;
        var source = Scaffold(name, ver);
        var projectPath = $"{name}/{name}.csproj";

        return Dag.Dotnet()
            .Publish(source, config, ver, projectPath);
    }

    /// <summary>
    /// Create a production-ready ASP.NET container
    /// </summary>
    /// <param name="projectName">The name of the project. Uses constructor default if not specified.</param>
    /// <param name="port">The HTTP port to expose. Uses constructor default if not specified.</param>
    /// <param name="version">The .NET version. Uses constructor default if not specified.</param>
    /// <param name="baseImage">The base OS image variant. Uses constructor default if not specified.</param>
    [Function]
    public Container PublishContainer(
        string? projectName = null,
        int? port = null,
        string? version = null,
        string? baseImage = null)
    {
        var name = projectName ?? this.ProjectName;
        var ver = version ?? this.Version;
        var portValue = port ?? this.Port;
        var image = baseImage ?? this.BaseImage;
        var source = Scaffold(name, ver);
        var projectPath = $"{name}/{name}.csproj";
        var entrypoint = $"{name}.dll";

        return Dag.Dotnet()
            .PublishAspNetContainer(
                source,
                entrypoint,
                ver,
                image,
                "Release",
                projectPath,
                portValue);
    }

    /// <summary>
    /// Serve the ASP.NET Blazor application as a Dagger service
    /// </summary>
    /// <param name="projectName">The name of the project. Uses constructor default if not specified.</param>
    /// <param name="port">The HTTP port to expose. Uses constructor default if not specified.</param>
    /// <param name="version">The .NET version. Uses constructor default if not specified.</param>
    /// <param name="baseImage">The base OS image variant. Uses constructor default if not specified.</param>
    [Function]
    public Service Serve(
        string? projectName = null,
        int? port = null,
        string? version = null,
        string? baseImage = null)
    {
        return PublishContainer(projectName, port, version, baseImage)
            .AsService();
    }

    /// <summary>
    /// Export the scaffolded template source code to inspect or modify
    /// </summary>
    /// <param name="projectName">The name of the project. Uses constructor default if not specified.</param>
    /// <param name="version">The .NET SDK version. Uses constructor default if not specified.</param>
    /// <returns>Directory containing the generated template source</returns>
    [Function]
    public Directory Source(string? projectName = null, string? version = null)
    {
        return Scaffold(projectName, version);
    }
}
