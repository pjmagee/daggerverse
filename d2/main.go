package main

import (
	"dagger/d-2/internal/dagger"
	"fmt"
)

type D2 struct {
	File *dagger.File      // +private
	Dir  *dagger.Directory // +private
}

// Export https://d2lang.com/tour/exports
type Export string

const (
	SVG  Export = "svg"
	PNG  Export = "png"
	PDF  Export = "pdf"
	PPTX Export = "pptx"
	GIF  Export = "gif"
)

func (m *D2) WithFile(file *dagger.File) *D2 {
	m.File = file
	return m
}

func (m *D2) WithDirectory(
	// +ignore=["*", "!**/*.d2"]
	dir *dagger.Directory) *D2 {
	m.Dir = dir
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

// renders the D2 file or directory to the given export format.
func (m *D2) Render(
	// +optional
	// +default="svg"
	export Export,
) *dagger.Directory {

	if m.File != nil {
		return renderFile(m.File, export)
	}

	if m.Dir != nil {
		return renderDir(m.Dir, export)
	}

	panic("No file or directory provided")
}

func container(export Export) *dagger.Container {

	if export == SVG {
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

func renderFile(file *dagger.File, export Export) *dagger.Directory {

	return container(export).
		WithWorkdir("/d2").
		WithMountedFile("./in/in.d2", file).
		WithWorkdir("./out").
		WithExec([]string{"d2", "/d2/in/in.d2", fmt.Sprintf("/d2/out/out.%s", export)}).
		Directory(".")
}

func renderDir(dir *dagger.Directory, export Export) *dagger.Directory {

	return container(export).
		WithWorkdir("/d2").
		WithMountedDirectory("./in", dir).
		WithWorkdir("./out").
		WithExec([]string{
			"sh", "-c", fmt.Sprintf("for f in /d2/in/*; do d2 $f $(basename $f).%s; done", export),
		}).
		Directory(".")
}
