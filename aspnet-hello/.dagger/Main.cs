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
    public Container Build(Directory? source = null, string version = "10.0", string configuration = "Release")
    {
        source ??= Dag.CurrentModule().Source().Directory_("..");
        
        return Dag.Dotnet()
            .Build(source, configuration, version, "AspNetHello/AspNetHello.csproj");
    }

    /// <summary>
    /// Publish the ASP.NET Blazor application using the dotnet dependency module
    /// </summary>
    [Function]
    public Container Publish(Directory? source = null, string version = "10.0", string configuration = "Release")
    {
        source ??= Dag.CurrentModule().Source().Directory_("..");
        
        return Dag.Dotnet()
            .Publish(source, configuration, version, "AspNetHello/AspNetHello.csproj");
    }

    /// <summary>
    /// Create a production-ready ASP.NET container using the dotnet dependency module
    /// </summary>
    [Function]
    public Container PublishContainer(Directory? source = null, int port = 5000, string version = "10.0")
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
                port);
    }

    /// <summary>
    /// Serve the ASP.NET Blazor application as a Dagger service on the specified port
    /// </summary>
    [Function]
    public Service Serve(Directory? source = null, int port = 5000, string version = "10.0")
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
                port)
            .AsService();
    }
}
