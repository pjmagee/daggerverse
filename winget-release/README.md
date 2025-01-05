# dagger-winget-release

This is a bit painful to automate fully via Github Actions since `winget` is **NOT** officially supported on the Github Windows runners.

Because of these issues, it's better to run the dagger command on a local Windows machine and pipe to execute the command.

## Submit the WinGet release:

**WARNING**: This will auto create a PR and submit the release to the WinGet repository! 

```bash
dagger call create-winget-command --token=cmd:'op read op://Private/github.com/wingetcli' | iex
```

## Create the command and invoke the command:

```powershell
dagger call create-winget-command | iex
```

## Print the command to the console:

```powershell
dagger call create-winget-command
```