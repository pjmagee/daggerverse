using Dagger;

/// <summary>
/// A tour of the Dagger C# SDK. Every function shows one feature, and the
/// summary says which attribute or pattern it uses.
///
/// Try it:
///   dagger functions
///   dagger call greet --name=Dagger
///   dagger check
///   dagger generate
///   dagger up
/// </summary>
[Object]
public class CsharpFeatures
{
    /// <summary>
    /// The greeting used by Greet. Public properties with a setter become fields.
    /// </summary>
    public string Greeting { get; set; }

    /// <summary>
    /// How loudly to greet. Enum-typed fields work like any other field.
    /// </summary>
    public Volume Volume { get; set; }

    /// <summary>
    /// The module's constructor. Its arguments become top-level flags:
    /// `dagger call --greeting=Hi --volume=LOUD greet`.
    /// </summary>
    /// <param name="greeting">The greeting to use</param>
    /// <param name="volume">How loudly to greet</param>
    public CsharpFeatures(string greeting = "Hello", Volume volume = Volume.Normal)
    {
        Greeting = greeting;
        Volume = volume;
    }

    // ----------------------------------------------------------------------
    // Functions, arguments and return types
    // ----------------------------------------------------------------------

    /// <summary>
    /// [Function] exposes a method. Arguments with defaults are optional.
    /// </summary>
    /// <param name="name">Who to greet</param>
    [Function]
    public string Greet(string name = "World")
    {
        var message = $"{Greeting}, {name}!";
        return Volume switch
        {
            Volume.Quiet => message.ToLowerInvariant(),
            Volume.Loud or Volume.Shouting => message.ToUpperInvariant(),
            _ => message,
        };
    }

    /// <summary>
    /// [Function(Name = ...)] renames a function, and [Name] renames an argument.
    /// This one is called as `dagger call add-all --numbers=1,2,3`.
    /// </summary>
    /// <param name="values">The numbers to add up</param>
    [Function(Name = "addAll")]
    public int Sum([Name("numbers")] int[] values)
    {
        return values.Sum();
    }

    /// <summary>
    /// Nullable arguments are optional and arrive as null when omitted.
    /// </summary>
    /// <param name="text">The text to shout</param>
    /// <param name="suffix">Text to append, if any</param>
    [Function]
    public string Shout(string text, string? suffix = null)
    {
        return text.ToUpperInvariant() + (suffix ?? "!");
    }

    /// <summary>
    /// Async functions return Task or Task&lt;T&gt;. Dagger awaits them.
    /// </summary>
    [Function]
    public async Task<string> OsRelease()
    {
        return await Dag.Container()
            .From("alpine:3.20")
            .WithExec(["cat", "/etc/os-release"])
            .Stdout();
    }

    /// <summary>
    /// Secret arguments keep their value out of logs and caches.
    /// Call it with `--token=env:MY_TOKEN`.
    /// </summary>
    /// <param name="token">A secret token</param>
    [Function]
    public async Task<int> TokenLength(Secret token)
    {
        var plaintext = await token.Plaintext();
        return plaintext.Length;
    }

    /// <summary>
    /// Returning another [Object] lets callers chain:
    /// `dagger call counter --start=5 increment increment value`.
    /// </summary>
    /// <param name="start">The starting value</param>
    [Function]
    public Counter Counter(int start = 0)
    {
        return new Counter(start);
    }

    /// <summary>
    /// Interface arguments accept any object, from any module, that has the
    /// interface's functions. See IFormatter.
    /// </summary>
    /// <param name="formatter">An object implementing Formatter</param>
    /// <param name="text">The text to format</param>
    [Function]
    public Task<string> FormatWith(IFormatter formatter, string text)
    {
        return formatter.Format(text);
    }

    // ----------------------------------------------------------------------
    // Directories, defaults and filters
    // ----------------------------------------------------------------------

    /// <summary>
    /// [DefaultPath] makes a Directory argument optional: when omitted, Dagger
    /// loads the path from the caller's project (relative paths start at the
    /// module's directory). [Ignore] filters what gets uploaded.
    /// </summary>
    /// <param name="source">The directory to list</param>
    [Function]
    public Task<string[]> ListSource(
        [DefaultPath(".")] [Ignore("bin", "obj", "sdk")] Directory source
    )
    {
        return source.Entries();
    }

    /// <summary>
    /// [DefaultAddress] makes a Container argument optional: when omitted,
    /// Dagger pulls the given image. Pass `--ctr=alpine:3.19` to override it.
    /// </summary>
    /// <param name="ctr">The container to inspect</param>
    [Function]
    public Task<string> AlpineVersion([DefaultAddress("alpine:3.20")] Container ctr)
    {
        return ctr.WithExec(["cat", "/etc/alpine-release"]).Stdout();
    }

    // ----------------------------------------------------------------------
    // Caching and deprecation
    // ----------------------------------------------------------------------

    /// <summary>
    /// Cache = "never" re-runs the function on every call.
    /// </summary>
    [Function(Cache = "never")]
    public string Now()
    {
        return DateTimeOffset.UtcNow.ToString("O");
    }

    /// <summary>
    /// Cache = "session" caches the result for one Dagger session.
    /// </summary>
    [Function(Cache = "session")]
    public string SessionId()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// A duration such as "10m" caches the result for that long.
    /// </summary>
    [Function(Cache = "10m")]
    public int RandomNumber()
    {
        return Random.Shared.Next(1, 100);
    }

    /// <summary>
    /// [Deprecated] marks a function or an argument as deprecated. Dagger
    /// shows the message in `dagger functions` and when it is called.
    /// </summary>
    /// <param name="name">Who to greet</param>
    [Function]
    [Deprecated("Use greet instead")]
    public string SayHello(string name = "World")
    {
        return Greet(name);
    }

    /// <summary>
    /// Only the old argument is deprecated here.
    /// </summary>
    /// <param name="name">Who to greet</param>
    /// <param name="who">Old name of the name argument</param>
    [Function]
    public string Welcome(string name = "World", [Deprecated("Use name instead")] string? who = null)
    {
        return Greet(who ?? name);
    }

    // ----------------------------------------------------------------------
    // Workspace features: dagger check, dagger generate, dagger up
    // ----------------------------------------------------------------------

    /// <summary>
    /// [Check] adds a function to `dagger check`. It fails by throwing.
    /// Checks cannot have required arguments.
    /// </summary>
    [Function]
    [Check]
    public void GreetingIsNotEmpty()
    {
        if (string.IsNullOrWhiteSpace(Greeting))
        {
            throw new InvalidOperationException("greeting must not be empty");
        }
    }

    /// <summary>
    /// A check can run a container: awaiting Sync() throws when a command
    /// exits non-zero, which fails the check. Here the source must contain a README.
    /// </summary>
    /// <param name="source">The module's source</param>
    [Function]
    [Check]
    public async Task HasReadme([DefaultPath(".")] Directory source)
    {
        await Dag.Container()
            .From("alpine:3.20")
            .WithDirectory("/src", source)
            .WithExec(["test", "-f", "/src/README.md"])
            .Sync();
    }

    /// <summary>
    /// [Generate] adds a function to `dagger generate`. It returns a Changeset,
    /// which Dagger shows and can apply to the caller's files. This one keeps
    /// a generated GREETING.txt up to date.
    /// </summary>
    /// <param name="source">The module's source</param>
    [Function]
    [Generate]
    public Changeset GreetingFile([DefaultPath(".")] [Ignore("bin", "obj", "sdk")] Directory source)
    {
        return source.WithNewFile("GREETING.txt", Greet() + "\n").Changes(source);
    }

    /// <summary>
    /// [Up] adds a function to `dagger up`, which starts the returned Service
    /// and forwards its ports to the host.
    /// </summary>
    /// <param name="port">The port to serve on</param>
    [Function]
    [Up]
    public Service Web(int port = 8080)
    {
        return Dag.Container()
            .From("nginx:1.27-alpine")
            .WithNewFile("/usr/share/nginx/html/index.html", $"<h1>{Greet()}</h1>\n")
            .WithNewFile(
                "/etc/nginx/conf.d/default.conf",
                $"server {{ listen {port}; root /usr/share/nginx/html; }}\n"
            )
            .WithExposedPort(port)
            .AsService(useEntrypoint: true);
    }
}
