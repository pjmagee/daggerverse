/*
D2 Diagramming Tool

This tool uses Dagger to render D2 diagrams within containerized environments.
You can render a single D2 file or an entire directory of D2 files, specify the output
format (svg, png, pdf, pptx, gif), and pass extra arguments to the D2 command.

Example Usages:

	dagger call --file=your-file.d2 render export --path=./out
	dagger call --format='pdf' --file='your-file.d2' render export --path=./out
	dagger call --format='gif' --file='your-file.d2' with-arg --arg='--animate-interval=100' render export --path=./out

For more details, refer to the README.md.
*/
package main

import (
	"context"
	"dagger/d-2/internal/dagger"
	"errors"
	"fmt"
	"strings"
)

type D2 struct {
	File   *dagger.File      // +private
	Dir    *dagger.Directory // +private
	Format Format            // +private
	Args   []string          // +private, extra arguments for d2 command
}

// format https://d2lang.com/tour/formats
type Format string

const (
	SVG  Format = "svg"
	PNG  Format = "png"
	PDF  Format = "pdf"
	PPTX Format = "pptx"
	GIF  Format = "gif"
)

func New(
	// +optional
	// +default="svg"
	format Format,
	// +optional
	// +ignore=["*", "!**/*.d2"]
	dir *dagger.Directory,
	// +optional
	file *dagger.File) *D2 {
	return &D2{
		Dir:    dir,
		File:   file,
		Format: format,
	}
}

func (m *D2) WithArg(arg string) *D2 {
	m.Args = append(m.Args, arg)
	return m
}

func (m *D2) WithFrmat(format Format) *D2 {
	m.Format = format
	return m
}

// This doesn't work, because the file is uploaded in the session and changes from the host machine are not sent
// +private
func (m *D2) Serve(
	file *dagger.File,
	// +optional
	// +default=9000
	port int,
	// +optional
	// +default="0.0.0.0"
	host string) *dagger.Service {

	return container(PNG).
		WithWorkdir("/d2").
		WithMountedFile("./in/in.d2", file).
		WithWorkdir("./in").
		WithEnvVariable("PORT", fmt.Sprintf("%d", port)).
		WithEnvVariable("HOST", host).
		WithExposedPort(port, dagger.ContainerWithExposedPortOpts{
			Protocol:                    "TCP",
			Description:                 "The port D2 listens on",
			ExperimentalSkipHealthcheck: true,
		}).
		AsService(dagger.ContainerAsServiceOpts{
			Args:                          []string{"sh", "-c", "d2 --watch --port $PORT --host $HOST in.d2 out.svg"},
			UseEntrypoint:                 false,
			ExperimentalPrivilegedNesting: false,
			InsecureRootCapabilities:      false,
			Expand:                        true,
			NoInit:                        false,
		})
}

// renders the D2 file or directory to the given format format.
func (m *D2) Render() (*dagger.Directory, error) {

	if m.File != nil {
		return renderFile(m.File, m.Format, m.Args), nil
	}

	if m.Dir != nil {
		return renderDir(m.Dir, m.Format, m.Args), nil
	}

	return nil, errors.New("no file or directory provided")
}

func container(format Format) *dagger.Container {

	if format == SVG {
		return dag.Container().
			From("alpine").
			WithExec([]string{"apk", "add", "go"}).
			WithExec([]string{"go", "install", "oss.terrastruct.com/d2@latest"}).
			WithEnvVariable("PATH", "$PATH:/root/go/bin", dagger.ContainerWithEnvVariableOpts{Expand: true})
	}

	return dag.Container().
		From("mcr.microsoft.com/playwright:v1.50.1-noble").
		WithExec([]string{"bash", "-c", "apt-get update && apt-get -y install build-essential && curl -fsSL https://d2lang.com/install.sh | sh -s --"})
}

func renderFile(file *dagger.File, format Format, extraArgs []string) *dagger.Directory {
	// Assuming dagger.File provides a Name() method that returns the original file name, e.g. "HLD.d2".
	originalName, _ := file.Name(context.Background())
	// Create the input file path using the original file name.
	inputPath := fmt.Sprintf("/d2/in/%s", originalName)
	// Generate the output file name by appending the format extension: "HLD.d2.svg" for example.
	outputName := fmt.Sprintf("%s.%s", originalName, format)

	args := []string{"d2"}
	// Append any extra args after normalization (handles gif-specific flag defaults)
	args = append(args, normalizeArgs(format, extraArgs)...)
	// Append the input path and computed output path to the command arguments.
	args = append(args, inputPath, fmt.Sprintf("/d2/out/%s", outputName))

	return container(format).
		WithWorkdir("/d2").
		WithMountedFile(inputPath, file).
		WithWorkdir("./out").
		WithExec(args).
		Directory(".")
}

func renderDir(dir *dagger.Directory, format Format, extraArgs []string) *dagger.Directory {
	normalized := normalizeArgs(format, extraArgs)
	extraStr := ""
	if len(normalized) > 0 {
		extraStr = strings.Join(normalized, " ") + " "
	}
	// Build the shell command with extra arguments included (if any)
	cmd := fmt.Sprintf("for f in /d2/in/*; do d2 %s $f $(basename $f).%s; done", extraStr, format)
	return container(format).
		WithWorkdir("/d2").
		WithMountedDirectory("./in", dir).
		WithWorkdir("./out").
		WithExec([]string{"sh", "-c", cmd}).
		Directory(".")
}

func normalizeArgs(format Format, extraArgs []string) []string {

	normalized := extraArgs

	if format == GIF {
		found := false
		for _, arg := range normalized {
			if strings.HasPrefix(arg, "--animate-interval") {
				found = true
				break
			}
		}
		if !found {
			normalized = append([]string{"--animate-interval", "100"}, normalized...)
		}
	}
	return normalized
}
