// A dagger module of Heroes Decode by HeroesToolChest that decodes Heroes of the Storm replays.

package main

import (
	"context"
	"fmt"
	"heroes-decode/internal/dagger"
	"strings"
	"time"
)

type HeroesDecode struct {
}

// The heroes decode command
func (m *HeroesDecode) Decode(
	ctx context.Context,
	// +optional
	// The replay file to decode
	file *dagger.File,
	// +optional
	// Additional arguments to pass to the decoder
	args []string,

) (*dagger.Container, error) {

	repo := dag.Git("https://github.com/HeroesToolChest/HeroesDecode.git")
	dir := repo.Tag("v1.4.0").Tree()

	build := dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:8.0").
		WithWorkdir("/app").
		WithDirectory("/app", dir.Directory("HeroesDecode")).
		WithExec([]string{"dotnet", "publish", "-c", "Release"})

	app := dag.Container().
		From("mcr.microsoft.com/dotnet/runtime:8.0").
		WithWorkdir("/app").
		WithDirectory("/app", build.Directory("/app/bin/Release/net8.0/publish")).
		WithEntrypoint([]string{"./HeroesDecode"})

	cmd := []string{}

	if file != nil {

		replayName := fmt.Sprintf("%s.StormReplay", strings.ReplaceAll(time.Now().Format(time.RFC3339Nano), ":", "_"))
		replayPath := fmt.Sprintf("/app/%s", replayName)
		replay := []string{"--replay-path", replayPath}

		app, _ = app.WithFile(replayPath, file).Sync(ctx)

		cmd = append(cmd, replay...)
	}

	if args != nil {
		cmd = append(cmd, args...)
	}

	return app.
		WithExec(cmd, dagger.ContainerWithExecOpts{
			UseEntrypoint: true,
		}).
		Sync(ctx)
}
