# D2 Diagramming

This tool uses Dagger to render D2 diagrams within containerized environments. You can render a single D2 file or an entire directory of D2 files, specify the output format, and optionally pass extra arguments to the underlying D2 command.

## Supported Formats

- `svg` (default)
- `png`
- `pdf`
- `pptx`
- `gif`

> **Note:** When rendering in `gif` format, the D2 command requires an animation interval flag (`--animate-interval`) with a value greater than 0. If no such argument is provided using the `with-arg` option, a default value of `100` will be automatically injected.

## Commands

### Render a Directory of D2 Files

```bash
dagger call --dir=./ render export --path=./out
```

This command mounts a directory of D2 files and renders them to the output directory specified by `--path`.

### Render a Single D2 File

```bash
dagger call --file=your-file.d2 render export --path=./out
```

This command renders the specified D2 file to the output location.

### Render with a Different Output Format

Specify the desired output format with the `--format` flag. For example, to render as a PDF:

```bash
dagger call --format='pdf' --file='your-file.d2' render export --path=./out
```


### Render GIF with Animation

When rendering GIFs, you can pass additional arguments using the `with-arg` flag. For example:

```bash
dagger call --format='gif' --file='your-file.d2' with-arg --arg='--animate-interval=100' render export --path=./out
```

If the `--animate-interval` argument is omitted for GIFs, the system will automatically prepend a default value (`--animate-interval 100`).

## API Overview

The underlying API is built around a fluent, builder-style pattern employing the following:

- **Format Selection:**  
  Use the `--format` flag (default is `svg`). The supported formats are defined as:
  - `SVG`
  - `PNG`
  - `PDF`
  - `PPTX`
  - `GIF`
  
- **Input Specification:**  
  Use `--file` for a single D2 file or `--dir` for a directory of D2 files.

- **Extra Arguments:**  
  Use the `with-arg` method/flag to pass additional command-line arguments to the D2 command. This is especially useful for GIF exports, where animated GIFs need the `--animate-interval` flag.

- **Rendering:**  
  The `render export` command creates an output directory (as provided by `--path`) containing the rendered diagram(s).

## Summary

This tool streamlines the process of rendering D2 diagrams in multiple formats, offering flexibility with extra arguments to fine-tune the rendering process—especially when generating animated GIFs. Configure your inputs with `--file` or `--dir`, choose your format via `--format`, and augment the D2 command using `with-arg`.