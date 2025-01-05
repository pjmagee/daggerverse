// A module for youtube-dl, a tool to download videos from youtube
package main

import (
	"dagger/youtube-dl/internal/dagger"
	"fmt"
)

func New(
	// The nightly version of youtube-dl to use
	// +optional
	// +default="2025.01.01"
	version string,
) *YoutubeDl {
	return &YoutubeDl{
		Cli:     Cli(version),
		Options: make([]string, 0),
		Urls:    make([]string, 0),
	}
}

var (
	WithExecOpts = dagger.ContainerWithExecOpts{UseEntrypoint: true}
)

type YoutubeDl struct {
	// The youtube-dl cli container
	Cli *dagger.Container
	// youtubne-dl [OPTIONS]
	Options []string
	// youtube-dl [OPTIONS] URL [URL ...]
	Urls []string
	// The version of youtube-dl to use
	Version string
}

func Ubuntu() *dagger.Container {
	return dag.
		Container().
		From("ubuntu")
}

func Cli(version string) *dagger.Container {

	cache := dag.CacheVolume("youtube-dl")

	return Ubuntu().
		WithWorkdir("/app").
		WithExec([]string{"sh", "-c", "apt-get update && apt-get install -y wget python3.12"}).
		WithExec([]string{"sh", "-c", "ln -s /usr/bin/python3.12 /usr/bin/python"}).
		WithExec([]string{"sh", "-c", fmt.Sprintf("wget https://github.com/ytdl-org/ytdl-nightly/releases/download/%s/youtube-dl -O /app/youtube-dl", version)}).
		WithExec([]string{"sh", "-c", "chmod a+rx /app/youtube-dl"}).
		WithMountedCache("~/.cache/youtube-dl", cache).
		WithEntrypoint([]string{"./youtube-dl"})
}

// The options to pass for youtube-dl [OPTIONS] ...
func (m *YoutubeDl) WithOptions(
	options []string,
) *YoutubeDl {
	m.Options = append(m.Options, options...)
	return m
}

// The URL [URLS ...] to pass to youtube-dl
func (m *YoutubeDl) WithUrls(
	// +optional
	urls []string,
) *YoutubeDl {
	m.Urls = append(m.Urls, urls...)
	return m
}

// Download the video from the given URL
func (m *YoutubeDl) File(
	// The file to save the video to
	path string,
) *dagger.File {
	return m.Cli.WithExec(append(m.Options, m.Urls...), WithExecOpts).File(path)
}

func (m *YoutubeDl) Directory(path string) *dagger.Directory {
	return m.Cli.WithExec(append(m.Options, m.Urls...), WithExecOpts).Directory(path)
}
