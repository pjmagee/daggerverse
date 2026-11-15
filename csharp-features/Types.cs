using Dagger;

/// <summary>
/// A second object type, returned by CsharpFeatures.Counter. Objects can be
/// split across files; the SDK finds every [Object] in the module.
/// </summary>
[Object]
public class Counter
{
    /// <summary>
    /// The current value. [Function] on a property exposes it as a field.
    /// </summary>
    [Function]
    public int Value { get; set; }

    /// <summary>
    /// Creates a counter.
    /// </summary>
    /// <param name="value">The starting value</param>
    public Counter(int value = 0)
    {
        Value = value;
    }

    /// <summary>
    /// Returns a new counter, one higher. Returning the same type makes the
    /// function chainable.
    /// </summary>
    /// <param name="by">How much to add</param>
    [Function]
    public Counter Increment(int by = 1)
    {
        return new Counter(Value + by);
    }
}

/// <summary>
/// [Enum] registers a C# enum. Values show up in SCREAMING_CASE on the CLI.
/// </summary>
[Enum(Description = "How loudly to greet")]
public enum Volume
{
    /// <summary>All lowercase</summary>
    [EnumValue(Description = "All lowercase")]
    Quiet,

    /// <summary>Unchanged</summary>
    [EnumValue(Description = "Unchanged")]
    Normal,

    /// <summary>All uppercase</summary>
    [EnumValue(Description = "All uppercase")]
    Loud,

    /// <summary>Old spelling of Loud</summary>
    [EnumValue(Description = "Old spelling of Loud", Deprecated = "Use LOUD instead")]
    Shouting,
}

/// <summary>
/// [Interface] declares a Dagger interface. Any object from any module with a
/// matching format function can be passed where a Formatter is expected.
/// </summary>
[Interface(Name = "Formatter")]
public interface IFormatter
{
    /// <summary>
    /// Formats the text.
    /// </summary>
    /// <param name="text">The text to format</param>
    [Function]
    Task<string> Format(string text);
}
