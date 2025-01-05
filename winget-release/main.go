package main

import (
	"context"
	"encoding/json"
	"fmt"
	"strings"
	"winget-release/internal/dagger"
)

type WingetRelease struct {
}

type Release struct {
	TagName string `json:"tag_name"`
	HtmlUrl string `json:"html_url"`
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
	token *dagger.Secret,
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

	pat := ""

	if token != nil {
		pat, _ = token.Plaintext(context.Background())
	}

	cmd := fmt.Sprintf("wingetcreate update")

	if submit {
		cmd += " --submit "
	}

	if pat != "" {
		cmd += fmt.Sprintf(" --token \"%s\" ", strings.ReplaceAll(pat, "\n", ""))
	}

	cmd += fmt.Sprintf(" --urls "+
		"\"https://dl.dagger.io/dagger/releases/%[1]s/dagger_v%[1]s_windows_amd64.zip\""+
		" \"https://dl.dagger.io/dagger/releases/%[1]s/dagger_v%[1]s_windows_arm64.zip\""+
		" \"https://dl.dagger.io/dagger/releases/%[1]s/dagger_v%[1]s_windows_armv7.zip\""+
		" --version %[1]s", version)

	cmd += fmt.Sprintf(" Dagger.Cli")

	return cmd, nil
}
