using Dagger;

/// <summary>
/// A Dagger module for building and serving ASP.NET Blazor applications
/// </summary>
[Object]
public class AspnetHelloModule
{
    /// <summary>
    /// Build the ASP.NET Blazor application using the dotnet dependency module
    /// </summary>
    [Function]
    public Container Build(
        Directory? source = null,
        string version = "10.0",
        string configuration = "Release"
    )
    {
        source ??= Dag.CurrentModule().Source().Directory_("..");

        return Dag.Dotnet().Build(source, configuration, version, "AspNetHello/AspNetHello.csproj");
    }

    /// <summary>
    /// Publish the ASP.NET Blazor application using the dotnet dependency module
    /// </summary>
    [Function]
    public Container Publish(
        Directory? source = null,
        string version = "10.0",
        string configuration = "Release"
    )
    {
        source ??= Dag.CurrentModule().Source().Directory_("..");

        return Dag.Dotnet()
            .Publish(source, configuration, version, "AspNetHello/AspNetHello.csproj");
    }

    /// <summary>
    /// Create a production-ready ASP.NET container using the dotnet dependency module
    /// </summary>
    [Function]
    public Container PublishContainer(
        Directory? source = null,
        int port = 5000,
        string version = "10.0"
    )
    {
        source ??= Dag.CurrentModule().Source().Directory_("..");

        return Dag.Dotnet()
            .PublishAspNetContainer(
                source,
                "AspNetHello.dll",
                version,
                "debian",
                "Release",
                "AspNetHello/AspNetHello.csproj",
                port
            );
    }

    /// <summary>
    /// Serve the ASP.NET Blazor application as a Dagger service with HTTP and HTTPS support
    /// </summary>
    [Function]
    public Service Serve(
        Directory? source = null,
        int httpPort = 5000,
        int httpsPort = 443,
        string version = "10.0"
    )
    {
        source ??= Dag.CurrentModule().Source().Directory_("..");

        // Get published artifacts from dotnet module
        var published = Dag.Dotnet()
            .Publish(source, "Release", version, "AspNetHello/AspNetHello.csproj")
            .Directory("/publish");

        // Use SDK image to have dev-certs tool available for HTTPS
        var container = Dag.Dotnet()
            .Sdk(version)
            .WithDirectory("/app", published)
            .WithWorkdir("/app")
            .WithEnvVariable("ASPNETCORE_URLS", $"http://+:{httpPort};https://+:{httpsPort}")
            .WithEnvVariable("ASPNETCORE_HTTPS_PORT", httpsPort.ToString())
            .WithEnvVariable("ASPNETCORE_ENVIRONMENT", "Development")
            .WithExec(["dotnet", "dev-certs", "https", "--trust"])
            .WithExposedPort(httpPort)
            .WithExposedPort(httpsPort)
            .WithEntrypoint(["dotnet", "AspNetHello.dll"]);

        return container.AsService();
    }
}
