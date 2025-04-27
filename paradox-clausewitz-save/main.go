// A module for working with Paradox Clausewitz save files
// This module provides functions to work with Paradox Clausewitz save files
// using the mageesoft-pdx-ce-sav tool from https://github.com/pjmagee/paradox-clausewitz-save

package main

import (
	"context"
	"dagger/paradox-clausewitz-save/internal/dagger"
	"fmt"
	"runtime"
	"strings"
)

type ParadoxClausewitzSave struct {
}

// downloads and returns the latest release binary for the current platform
func (m *ParadoxClausewitzSave) GetBinary() (*dagger.File, error) {

	platform := runtime.GOOS
	arch := runtime.GOARCH

	archMap := map[string]string{"amd64": "x64", "arm64": "arm64"}
	platformMap := map[string]string{"linux": "linux", "darwin": "macos", "windows": "windows"}

	mappedArch, ok := archMap[arch]
	if !ok {
		return nil, fmt.Errorf("unsupported architecture: %s", arch)
	}

	mappedPlatform, ok := platformMap[platform]
	if !ok {
		return nil, fmt.Errorf("unsupported platform: %s", platform)
	}

	extension := "tar.gz"
	if mappedPlatform == "windows" {
		extension = "zip"
	}

	latestReleaseURL := "https://github.com/pjmagee/paradox-clausewitz-save/releases/latest"

	container := dag.Container().
		From("ubuntu:latest").
		WithExec([]string{"apt-get", "update"}).
		WithExec([]string{"apt-get", "install", "-y", "curl"})

	cmd := []string{
		"sh", "-c",
		"curl -s -I -L " + latestReleaseURL + " | grep -i 'location:' | tail -n 1 | sed 's/.*\\/v\\([^/]*\\).*/\\1/' | tr -d '\\r\\n'",
	}

	version, err := container.WithExec(cmd).Stdout(context.Background())
	if err != nil {
		return nil, fmt.Errorf("failed to get latest version: %w", err)
	}

	version = strings.TrimSpace(version)
	if version == "" {
		version = "1.0.0"
	}

	filename := fmt.Sprintf("mageesoft-pdx-ce-sav_%s_%s_%s.%s", version, mappedPlatform, mappedArch, extension)
	url := fmt.Sprintf("https://github.com/pjmagee/paradox-clausewitz-save/releases/download/v%s/%s", version, filename)

	return dag.HTTP(url), nil
}

// processes a Paradox Clausewitz save file with the specified arguments
func (m *ParadoxClausewitzSave) Process(
	ctx context.Context,
	// +optional
	saveFile *dagger.File,
	// +optional
	args []string) (string, error) {

	binary, err := m.GetBinary()

	if err != nil {
		return "", err
	}

	container := dag.Container().
		From("ubuntu:latest").
		WithExec([]string{"apt-get", "update"}).
		WithExec([]string{"apt-get", "install", "-y", "tar", "unzip", "file"})

	container = container.WithExec([]string{"mkdir", "-p", "/app"})
	container = container.WithMountedFile("/tmp/binary", binary)

	// Get the binary name to determine extraction method
	binaryName, err := binary.Name(ctx)
	if err != nil {
		return "", fmt.Errorf("failed to get binary name: %w", err)
	}

	// Extract the archive based on its type
	if strings.HasSuffix(binaryName, ".zip") {
		container = container.WithExec([]string{"unzip", "/tmp/binary", "-d", "/app"})
	} else {
		container = container.WithExec([]string{"tar", "-xzf", "/tmp/binary", "-C", "/app"})
	}

	container = container.
		WithExec([]string{"ls", "-la", "/app"}).
		WithExec([]string{"chmod", "+x", "/app/mageesoft-pdx-ce-sav"}).
		WithWorkdir("/app").
		WithExec([]string{"sh", "-c", "echo 'File type:' && file ./mageesoft-pdx-ce-sav"}).
		WithExec([]string{"apt-get", "install", "-y", "libc6", "libstdc++6", "libicu-dev"}).
		WithExec([]string{"apt-get", "install", "-y", "wget", "apt-transport-https"}).
		WithEnvVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1").
		WithExec([]string{"chmod", "+x", "/app/mageesoft-pdx-ce-sav"})

	// Prepare command arguments
	cmdArgs := []string{"./mageesoft-pdx-ce-sav"}

	// Add save file if provided
	if saveFile != nil {
		container = container.WithMountedFile("/tmp/save", saveFile)
		cmdArgs = append(cmdArgs, "-s", "/tmp/save")
	}

	// Add any additional arguments
	if args != nil {
		cmdArgs = append(cmdArgs, args...)
	}

	// Execute the binary
	result := container.WithExec(cmdArgs)

	// Return the output
	return result.Stdout(ctx)
}
