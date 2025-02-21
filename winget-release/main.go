package main

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"strings"
)

type WingetRelease struct {
}

type Release struct {
	TagName string `json:"tag_name"`
	HtmlUrl string `json:"html_url"`
}

// New method to check if release is already published
func (m *WingetRelease) IsReleasePublished(version string) (bool, error) {
	url := fmt.Sprintf("https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/Dagger/Cli/%s", version)
	resp, err := http.Get(url)
	if err != nil {
		return false, fmt.Errorf("failed to check winget repository: %w", err)
	}
	defer resp.Body.Close()

	// If status is 200, the release exists
	return resp.StatusCode == http.StatusOK, nil
}

func (m *WingetRelease) GetLatestRelease() (string, error) {
	resp := dag.HTTP("https://api.github.com/repos/dagger/dagger/releases/latest")

	contents, err := resp.Contents(context.Background())
	if err != nil {
		return contents, err
	}

	release := Release{}
	err = json.Unmarshal([]byte(contents), &release)
	if err != nil {
		return "", err
	}

	return release.TagName, nil
}

func (m *WingetRelease) CreateWingetCommand(
	// The version of the release
	// +optional
	tag string,
	// The token to use for the wingetcreate command
	// +optional
	tokenPlaceholder bool,
	// Whether to add the --submit
	// +optional
	// +default=false
	submit bool,
) (string, error) {

	if tag == "" {
		latest, err := m.GetLatestRelease()
		if err != nil {
			return "", err
		}
		tag = latest
	}

	version := strings.TrimPrefix(tag, "v")

	// Check if release is already published
	published, err := m.IsReleasePublished(version)

	if err != nil {
		return "", fmt.Errorf("failed to check if release is published: %w", err)
	}
	if published {
		return "", fmt.Errorf("release %s is already published to winget", version)
	}

	cmd := fmt.Sprintf(`.\wingetcreate.exe update`)

	if tokenPlaceholder {
		cmd += " --token $env:WINGETCREATE_TOKEN "
	}

	if submit {
		cmd += " --submit "
	}

	cmd += fmt.Sprintf(" --urls "+
		"\"https://dl.dagger.io/dagger/releases/%[1]s/dagger_v%[1]s_windows_amd64.zip\""+
		" \"https://dl.dagger.io/dagger/releases/%[1]s/dagger_v%[1]s_windows_arm64.zip\""+
		" \"https://dl.dagger.io/dagger/releases/%[1]s/dagger_v%[1]s_windows_armv7.zip\""+
		" --version %[1]s", version)

	cmd += fmt.Sprintf(" Dagger.Cli")
	cmd = strings.ReplaceAll(cmd, "  ", " ")

	return cmd, nil
}
