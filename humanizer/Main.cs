using System.Globalization;
using Dagger;
using Humanizer;

namespace TextHumanizer;

/// <summary>
/// A Dagger module for humanizing and transforming text using the Humanizer library.
/// Supports locale-aware string transformations with fluent builder-style chaining.
/// </summary>
[Object]
public class TextHumanizer
{
    /// <summary>
    /// The input text to transform
    /// </summary>
    [Field(Description = "The input text to transform")]
    public string Text { get; set; }

    /// <summary>
    /// Culture for localization (e.g., 'en-US', 'fr-FR', 'de-DE')
    /// </summary>
    [Field(Description = "Culture code for localization (e.g., 'en-US', 'fr-FR')")]
    public string Culture { get; set; }

    /// <summary>
    /// Creates a new Humanizer module instance
    /// </summary>
    public TextHumanizer(string text = "", string culture = "en-US")
    {
        Text = text;
        Culture = culture;
    }

    private CultureInfo GetCulture() => new CultureInfo(Culture);

    // ========== Builder Methods (Chainable) ==========

    /// <summary>
    /// Set the text to transform
    /// </summary>
    [Function]
    public TextHumanizer WithText(string text)
    {
        return new TextHumanizer(text, Culture);
    }

    /// <summary>
    /// Set the culture for locale-aware transformations
    /// </summary>
    [Function]
    public TextHumanizer WithCulture(string culture)
    {
        return new TextHumanizer(Text, culture);
    }

    // ========== Core Humanization Methods ==========

    /// <summary>
    /// Humanizes the input string (e.g., "PascalCaseInput" → "Pascal case input")
    /// </summary>
    [Function]
    public string Humanize(string? text = null)
    {
        var input = text ?? Text;
        return input.Humanize();
    }

    /// <summary>
    /// Dehumanizes the input string (e.g., "some text" → "SomeText")
    /// </summary>
    [Function]
    public string Dehumanize(string? text = null)
    {
        var input = text ?? Text;
        return input.Dehumanize();
    }

    // ========== Case Transformation Methods ==========

    /// <summary>
    /// Transforms to title case (e.g., "some text" → "Some Text")
    /// </summary>
    [Function]
    public string ToTitleCase(string? text = null, string? culture = null)
    {
        var input = text ?? Text;
        var cultureInfo = culture != null ? new CultureInfo(culture) : GetCulture();
        return input.Transform(cultureInfo, To.TitleCase);
    }

    /// <summary>
    /// Transforms to sentence case (e.g., "some text" → "Some text")
    /// </summary>
    [Function]
    public string ToSentenceCase(string? text = null, string? culture = null)
    {
        var input = text ?? Text;
        var cultureInfo = culture != null ? new CultureInfo(culture) : GetCulture();
        return input.Transform(cultureInfo, To.SentenceCase);
    }

    /// <summary>
    /// Transforms to lowercase (e.g., "Some Text" → "some text")
    /// </summary>
    [Function]
    public string ToLowerCase(string? text = null, string? culture = null)
    {
        var input = text ?? Text;
        var cultureInfo = culture != null ? new CultureInfo(culture) : GetCulture();
        return input.Transform(cultureInfo, To.LowerCase);
    }

    /// <summary>
    /// Transforms to uppercase (e.g., "some text" → "SOME TEXT")
    /// </summary>
    [Function]
    public string ToUpperCase(string? text = null, string? culture = null)
    {
        var input = text ?? Text;
        var cultureInfo = culture != null ? new CultureInfo(culture) : GetCulture();
        return input.Transform(cultureInfo, To.UpperCase);
    }

    // ========== Inflection Methods ==========

    /// <summary>
    /// Pluralizes the input word (e.g., "person" → "people")
    /// </summary>
    [Function]
    public string Pluralize(string? text = null, bool inputIsKnownToBeSingular = true)
    {
        var input = text ?? Text;
        return input.Pluralize(inputIsKnownToBeSingular);
    }

    /// <summary>
    /// Singularizes the input word (e.g., "people" → "person")
    /// </summary>
    [Function]
    public string Singularize(string? text = null, bool inputIsKnownToBePlural = true)
    {
        var input = text ?? Text;
        return input.Singularize(inputIsKnownToBePlural);
    }

    /// <summary>
    /// Converts to PascalCase (e.g., "some_property_name" → "SomePropertyName")
    /// </summary>
    [Function]
    public string Pascalize(string? text = null)
    {
        var input = text ?? Text;
        return input.Pascalize();
    }

    /// <summary>
    /// Converts to camelCase (e.g., "SomePropertyName" → "somePropertyName")
    /// </summary>
    [Function]
    public string Camelize(string? text = null)
    {
        var input = text ?? Text;
        return input.Camelize();
    }

    /// <summary>
    /// Converts to underscore_case (e.g., "SomePropertyName" → "some_property_name")
    /// </summary>
    [Function]
    public string Underscore(string? text = null)
    {
        var input = text ?? Text;
        return input.Underscore();
    }

    /// <summary>
    /// Converts to dasherized-case (e.g., "some_property_name" → "some-property-name")
    /// </summary>
    [Function]
    public string Dasherize(string? text = null)
    {
        var input = text ?? Text;
        return input.Dasherize();
    }

    /// <summary>
    /// Converts to kebab-case (e.g., "SomePropertyName" → "some-property-name")
    /// </summary>
    [Function]
    public string Kebaberize(string? text = null)
    {
        var input = text ?? Text;
        return input.Kebaberize();
    }

    /// <summary>
    /// Converts to Title Case and replaces underscores with spaces (e.g., "some_title" → "Some Title")
    /// </summary>
    [Function]
    public string Titleize(string? text = null)
    {
        var input = text ?? Text;
        return input.Titleize();
    }

    // ========== Utility Methods ==========

    /// <summary>
    /// Truncates text to specified length (e.g., "Long text" with length 5 → "Long…")
    /// </summary>
    [Function]
    public string Truncate(string? text = null, int length = 10, string truncationString = "…")
    {
        var input = text ?? Text;
        return input.Truncate(length, truncationString);
    }

    /// <summary>
    /// Combines quantity with pluralized word (e.g., "person" with quantity 5 → "5 people")
    /// </summary>
    [Function]
    public string ToQuantity(string? text = null, int quantity = 1)
    {
        var input = text ?? Text;
        return input.ToQuantity(quantity);
    }

    /// <summary>
    /// Hyphenates text (e.g., "some text here" → "some-text-here")
    /// </summary>
    [Function]
    public string Hyphenate(string? text = null)
    {
        var input = text ?? Text;
        return input.Hyphenate();
    }
}
