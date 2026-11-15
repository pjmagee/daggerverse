# C# SDK feature tour

A small Dagger module that shows every feature of the [Dagger C# SDK](https://github.com/pjmagee/dagger/tree/csharp/experimental/sdk/csharp), one function per feature.

```bash
dagger -m github.com/pjmagee/daggerverse/csharp-features functions
```

| Feature | Where | Try it |
| --- | --- | --- |
| Constructor arguments | `CsharpFeatures(...)` | `dagger call --greeting=Hi --volume=LOUD greet` |
| `[Function]`, optional arguments | `Greet` | `dagger call greet --name=Dagger` |
| `[Function(Name)]`, `[Name]`, arrays | `Sum` | `dagger call add-all --numbers=1,2,3` |
| Nullable arguments | `Shout` | `dagger call shout --text=hey` |
| Async functions | `OsRelease` | `dagger call os-release` |
| Secrets | `TokenLength` | `dagger call token-length --token=env:HOME` |
| Chained objects, `[Function]` fields | `Counter` (Types.cs) | `dagger call counter --start=5 increment increment value` |
| `[Interface]` | `IFormatter` (Types.cs), `FormatWith` | pass any object with a `format` function |
| `[Enum]`, `[EnumValue]` | `Volume` (Types.cs) | `dagger call --volume=QUIET greet` |
| `[DefaultPath]`, `[Ignore]` | `ListSource` | `dagger call list-source` |
| `[DefaultAddress]` | `AlpineVersion` | `dagger call alpine-version --ctr=alpine:3.19` |
| Cache policies | `Now`, `SessionId`, `RandomNumber` | `dagger call now` |
| `[Deprecated]` | `SayHello`, `Welcome`, `Volume.Shouting` | `dagger call say-hello` |
| `[Check]` | `GreetingIsNotEmpty`, `HasReadme` | `dagger check` |
| `[Generate]` | `GreetingFile` | `dagger generate` |
| `[Up]` | `Web` | `dagger up`, then open http://localhost:8080 |

The module uses the C# SDK from `github.com/pjmagee/dagger/sdk/csharp@csharp/experimental` until it lands upstream.
