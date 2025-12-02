using Dagger;

namespace AspNetHelloTemplate;

/// <summary>
/// A Dagger module that dynamically generates and serves ASP.NET Blazor applications on-the-fly
/// 
/// This module uses 'dotnet new blazor' to scaffold templates at runtime instead of storing source code.
/// All application code is generated dynamically through Dagger functions, keeping the repository clean
/// with only the Dagger module code.
/// </summary>
[Object]
public class AspnetHelloTemplateModule
{
    /// <summary>
    /// Scaffold a new ASP.NET Blazor application from the default template
    /// </summary>
    /// <param name="projectName">The name of the project to generate. Default: "AspNetHelloTemplate"</param>
    /// <param name="version">The .NET SDK version to use. Default: "10.0"</param>
    /// <returns>Directory containing the scaffolded Blazor application</returns>
    [Function]
    public Directory Scaffold(string projectName = "AspNetHelloTemplate", string version = "10.0")
    {
        return Dag.Dotnet()
            .Sdk(version)
            .WithWorkdir("/workspace")
            .WithExec(["dotnet", "new", "blazor", "-n", projectName])
            .Directory("/workspace");
    }

    /// <summary>
    /// Build the ASP.NET Blazor application (scaffolds template on-the-fly)
    /// </summary>
    /// <param name="projectName">The name of the project. Default: "AspNetHelloTemplate"</param>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    /// <param name="configuration">Build configuration (Debug or Release). Default: "Release"</param>
    [Function]
    public Container Build(
        string projectName = "AspNetHelloTemplate",
        string version = "10.0",
        string configuration = "Release")
    {
        var source = Scaffold(projectName, version);
        var projectPath = $"{projectName}/{projectName}.csproj";

        return Dag.Dotnet()
            .Build(source, configuration, version, projectPath);
    }

    /// <summary>
    /// Publish the ASP.NET Blazor application (scaffolds template on-the-fly)
    /// </summary>
    /// <param name="projectName">The name of the project. Default: "AspNetHelloTemplate"</param>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    /// <param name="configuration">Build configuration. Default: "Release"</param>
    [Function]
    public Container Publish(
        string projectName = "AspNetHelloTemplate",
        string version = "10.0",
        string configuration = "Release")
    {
        var source = Scaffold(projectName, version);
        var projectPath = $"{projectName}/{projectName}.csproj";

        return Dag.Dotnet()
            .Publish(source, configuration, version, projectPath);
    }

    /// <summary>
    /// Create a production-ready ASP.NET container (scaffolds template on-the-fly)
    /// </summary>
    /// <param name="projectName">The name of the project. Default: "AspNetHelloTemplate"</param>
    /// <param name="port">The HTTP port to expose. Default: 5000</param>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant. Default: "debian"</param>
    [Function]
    public Container PublishContainer(
        string projectName = "AspNetHelloTemplate",
        int port = 5000,
        string version = "10.0",
        string baseImage = "debian")
    {
        var source = Scaffold(projectName, version);
        var projectPath = $"{projectName}/{projectName}.csproj";
        var entrypoint = $"{projectName}.dll";

        return Dag.Dotnet()
            .PublishAspNetContainer(
                source,
                entrypoint,
                version,
                baseImage,
                "Release",
                projectPath,
                port);
    }

    /// <summary>
    /// Serve the ASP.NET Blazor application as a Dagger service (scaffolds template on-the-fly)
    /// </summary>
    /// <param name="projectName">The name of the project. Default: "AspNetHelloTemplate"</param>
    /// <param name="port">The HTTP port to expose. Default: 5000</param>
    /// <param name="version">The .NET version. Default: "10.0"</param>
    /// <param name="baseImage">The base OS image variant. Default: "debian"</param>
    [Function]
    public Service Serve(
        string projectName = "AspNetHelloTemplate",
        int port = 5000,
        string version = "10.0",
        string baseImage = "debian")
    {
        return PublishContainer(projectName, port, version, baseImage)
            .AsService();
    }

    /// <summary>
    /// Export the scaffolded template source code to inspect or modify
    /// </summary>
    /// <param name="projectName">The name of the project. Default: "AspNetHelloTemplate"</param>
    /// <param name="version">The .NET SDK version. Default: "10.0"</param>
    /// <returns>Directory containing the generated template source</returns>
    [Function]
    public Directory Source(string projectName = "AspNetHelloTemplate", string version = "10.0")
    {
        return Scaffold(projectName, version);
    }
}
