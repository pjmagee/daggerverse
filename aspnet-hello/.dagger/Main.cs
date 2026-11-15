using Dagger;

/// <summary>
/// A Dagger module for building, checking and serving the AspNetHello Blazor app.
///
/// It doubles as a tour of the C# SDK's workspace features:
/// [DefaultPath] + [Ignore] for the source, [Check] for `dagger check`,
/// [Generate] for `dagger generate`, [Up] for `dagger up` and
/// [DefaultAddress] for a container argument with a default image.
/// </summary>
[Object]
public class AspnetHelloModule
{
    private const string Project = "AspNetHello/AspNetHello.csproj";

    /// <summary>
    /// The app's source. Defaults to this module's directory, minus build output.
    /// </summary>
    public Directory Source { get; set; }

    /// <summary>
    /// The .NET version used for every build.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Creates the module. Both arguments are optional: `source` falls back to
    /// the module's directory through [DefaultPath], so `dagger call build` works
    /// without any flags.
    /// </summary>
    /// <param name="source">The app's source directory</param>
    /// <param name="version">The .NET version, such as 10.0</param>
    public AspnetHelloModule(
        [DefaultPath(".")] [Ignore("**/bin", "**/obj", ".dagger/sdk", ".git")] Directory source,
        string version = "10.0"
    )
    {
        Source = source;
        Version = version;
    }

    /// <summary>
    /// Build the app with the dotnet dependency module.
    /// </summary>
    /// <param name="configuration">Build configuration (Debug or Release)</param>
    [Function]
    public Container Build(string configuration = "Release")
    {
        return Dag.Dotnet().Build(Source, configuration, Version, Project);
    }

    /// <summary>
    /// Publish the app's artifacts to /publish.
    /// </summary>
    /// <param name="configuration">Build configuration (Debug or Release)</param>
    [Function]
    public Container Publish(string configuration = "Release")
    {
        return Dag.Dotnet().Publish(Source, configuration, Version, Project);
    }

    /// <summary>
    /// Create a production ASP.NET runtime container for the app.
    /// </summary>
    /// <param name="port">The port the app listens on</param>
    [Function]
    public Container PublishContainer(int port = 5000)
    {
        return Dag.Dotnet()
            .PublishAspNetContainer(
                Source,
                "AspNetHello.dll",
                Version,
                "debian",
                "Release",
                Project,
                port
            );
    }

    /// <summary>
    /// Serve the app over HTTP and HTTPS.
    ///
    /// [Up] registers it with `dagger up`, so the app runs with no flags.
    /// It still works as a regular function: `dagger call serve up --ports=5000:5000`.
    /// </summary>
    /// <param name="httpPort">HTTP port</param>
    /// <param name="httpsPort">HTTPS port</param>
    [Function]
    [Up]
    public Service Serve(int httpPort = 5000, int httpsPort = 5001)
    {
        var published = Publish().Directory("/publish");

        // The SDK image ships the dev-certs tool needed for HTTPS.
        return Dag.Dotnet()
            .Sdk(Version)
            .WithDirectory("/app", published)
            .WithWorkdir("/app")
            .WithEnvVariable("ASPNETCORE_URLS", $"http://+:{httpPort};https://+:{httpsPort}")
            .WithEnvVariable("ASPNETCORE_HTTPS_PORT", httpsPort.ToString())
            .WithEnvVariable("ASPNETCORE_ENVIRONMENT", "Development")
            .WithExec(["dotnet", "dev-certs", "https"])
            .WithExposedPort(httpPort)
            .WithExposedPort(httpsPort)
            .AsService(args: ["dotnet", "AspNetHello.dll"]);
    }

    /// <summary>
    /// Check that the app compiles. Run it with `dagger check`.
    /// A check passes when the returned container's last command exits with 0.
    /// </summary>
    [Function]
    [Check]
    public Container BuildCheck()
    {
        return Build();
    }

    /// <summary>
    /// Check that the code is formatted, without changing it.
    /// </summary>
    [Function]
    [Check]
    public Container FormatCheck()
    {
        return DotnetFormat(verifyOnly: true);
    }

    /// <summary>
    /// Start the app and request its home page.
    ///
    /// The curl container is an argument with [DefaultAddress], so it is optional:
    /// it defaults to the image below, and a caller can pass another one.
    /// </summary>
    /// <param name="curl">A container with curl installed</param>
    [Function]
    [Check]
    public async Task SmokeTest([DefaultAddress("alpine/curl")] Container curl)
    {
        var body = await curl.WithServiceBinding("app", Serve())
            .WithExec(["curl", "--fail", "--silent", "--retry", "10", "--retry-connrefused", "http://app:5000/"])
            .Stdout();

        if (!body.Contains("Hello, world!"))
        {
            throw new InvalidOperationException("The home page did not render the expected greeting");
        }
    }

    /// <summary>
    /// Format the code with `dotnet format` and return the changes.
    ///
    /// [Generate] registers it with `dagger generate`, which shows the
    /// changeset and offers to apply it to your working copy.
    /// </summary>
    [Function]
    [Generate]
    public Changeset Format()
    {
        var formatted = DotnetFormat(verifyOnly: false).Directory("/src");
        return formatted.Changes(Source);
    }

    private Container DotnetFormat(bool verifyOnly)
    {
        List<string> args = ["dotnet", "format", Project];
        if (verifyOnly)
        {
            args.Add("--verify-no-changes");
        }

        return Dag.Dotnet()
            .Sdk(Version)
            .WithDirectory("/src", Source)
            .WithWorkdir("/src")
            .WithExec([.. args]);
    }
}
