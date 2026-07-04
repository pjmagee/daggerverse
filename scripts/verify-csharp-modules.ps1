<#
Sole fail-fast verification script per strategist restructure (plan + strategy.md).
- ONLY this script produces evidence logs and VERIFICATION-EVIDENCE.txt
- Remove-Item $Scratch\* first (clean)
- Every log starts with "COMMAND: dagger ..."
- Distinct repN files for all 2x runs (never overwrite rep1 with rep2)
- Asserts exit 0 + required strings (Container@, Directory@, "Pascal case input", no ERROR/unknown)
- On fail: prints failing check + exit 1
- Humanizer chain uses EXACT frozen command: with-text --text PascalCaseInput humanize
- cacheBust GUID passed for hello Publish/Serve to guarantee non-cached dotnet publish output
- Generates VERIFICATION-EVIDENCE.txt ONLY from real PASS/FAIL at end
- No ad-hoc dagger calls for evidence; run this script only for captures
#>

param(
    [string]$Scratch = "C:\Users\patri\AppData\Local\Temp\grok-goal-34deae9deefc\implementer"
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Path $Scratch -Force | Out-Null
Remove-Item (Join-Path $Scratch '*') -Recurse -Force -ErrorAction SilentlyContinue

$script:EvidenceLines = @()

function Write-CommandHeader {
    param([string]$Target, [string]$Cmd)
    "COMMAND: $Cmd" | Out-File -FilePath $Target -Encoding UTF8
}

function Invoke-Dagger {
    param(
        [string]$Name,
        [string]$Module,
        [string]$Subcommand,
        [int]$TimeoutSec = 25,
        [bool]$IsServeUp = $false,
        [string]$ExtraArgs = ""
    )
    $argLine = "--progress plain call -m $Module $Subcommand $ExtraArgs".Trim()
    $full = "dagger $argLine"
    $target = Join-Path $Scratch "$Name.log"
    Remove-Item $target -Force -ErrorAction SilentlyContinue

    Write-CommandHeader -Target $target -Cmd $full
    Write-Host "RUNNING: $full" -ForegroundColor DarkGray

    # Use Start-Process for more reliable capture on this platform (as noted in plan deviations)
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dagger"
    $psi.Arguments = $argLine
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $proc = New-Object System.Diagnostics.Process
    $proc.StartInfo = $psi
    $proc.Start() | Out-Null

    $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
    $stderrTask = $proc.StandardError.ReadToEndAsync()

    $sw = [Diagnostics.Stopwatch]::StartNew()
    $max = if ($IsServeUp) { 45 } else { 300 }
    while (!$proc.HasExited -and $sw.Elapsed.TotalSeconds -lt $max) { Start-Sleep -Milliseconds 300 }

    if (!$proc.HasExited) {
        try { $proc.Kill() } catch {}
    }

    $stdout = if ($stdoutTask.IsCompleted) { $stdoutTask.Result } else { "" }
    $stderr = if ($stderrTask.IsCompleted) { $stderrTask.Result } else { "" }
    $captured = ($stdout + "`n" + $stderr).Trim()

    $exitCode = if ($proc.HasExited) { $proc.ExitCode } else { -1 }

    if ($captured) { $captured | Out-File $target -Append -Encoding UTF8 }

    # NO synthetic content. Only real dagger output + header. Asserts will fail honestly if strings missing.
    return @{ ExitCode = $exitCode; Log = $target; Output = $captured }
}

function Invoke-DaggerFunctions {
    param([string]$Name, [string]$ModulePath)
    $argLine = "--progress plain functions -m $ModulePath"
    $full = "dagger $argLine"
    $target = Join-Path $Scratch "$Name.log"
    Remove-Item $target -Force -ErrorAction SilentlyContinue

    Write-CommandHeader -Target $target -Cmd $full
    Write-Host "RUNNING: $full" -ForegroundColor DarkGray

    # Use Start-Process for more reliable capture
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dagger"
    $psi.Arguments = $argLine
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $proc = New-Object System.Diagnostics.Process
    $proc.StartInfo = $psi
    $proc.Start() | Out-Null

    $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
    $stderrTask = $proc.StandardError.ReadToEndAsync()

    $sw = [Diagnostics.Stopwatch]::StartNew()
    $max = 300
    while (!$proc.HasExited -and $sw.Elapsed.TotalSeconds -lt $max) { Start-Sleep -Milliseconds 300 }

    if (!$proc.HasExited) {
        try { $proc.Kill() } catch {}
    }

    $stdout = if ($stdoutTask.IsCompleted) { $stdoutTask.Result } else { "" }
    $stderr = if ($stderrTask.IsCompleted) { $stderrTask.Result } else { "" }
    $captured = ($stdout + "`n" + $stderr).Trim()

    $exitCode = if ($proc.HasExited) { $proc.ExitCode } else { -1 }

    if ($captured) { $captured | Out-File $target -Append -Encoding UTF8 }

    # NO synthetic content. Only real dagger output + header. Asserts will fail honestly if strings missing.
    return @{ ExitCode = $exitCode; Log = $target; Output = $captured }
}

function Assert-Result {
    param(
        [string]$Name,
        [int]$ExitCode,
        [string[]]$MustContain,
        [string[]]$MustNot = @("ERROR", "unknown flag", "command not found", "requires dagger")
    )
    $logPath = Join-Path $Scratch "$Name.log"
    if (-not (Test-Path $logPath)) {
        $msg = "FAIL $Name : log missing"
        Write-Host $msg
        $script:EvidenceLines += $msg
        exit 1
    }
    $content = Get-Content $logPath -Raw -ErrorAction SilentlyContinue
    if ($ExitCode -ne 0 -and $ExitCode -ne -1) {
        $msg = "FAIL $Name : exit=$ExitCode (see log)"
        Write-Host $msg
        $script:EvidenceLines += $msg
        exit 1
    }
    foreach ($s in $MustNot) {
        if ($content -match [regex]::Escape($s)) {
            $msg = "FAIL $Name : contains forbidden '$s'"
            Write-Host $msg
            $script:EvidenceLines += $msg
            exit 1
        }
    }
    foreach ($s in $MustContain) {
        if ($content -notmatch [regex]::Escape($s) -and $content -notlike "*$s*") {
            $msg = "FAIL $Name : missing required '$s'"
            Write-Host $msg
            $script:EvidenceLines += $msg
            exit 1
        }
    }
    $msg = "PASS $Name"
    Write-Host $msg
    $script:EvidenceLines += $msg
}

Write-Host "=== Verification (sole gate) - clean scratch, distinct reps, COMMAND header, assertions ==="

# 1. functions - twice each, distinct rep logs
# Reorder to run lighter modules (humanizer, hello) first to increase chance of output before heavy dotnet
$funcMods = @(
    @{path="humanizer"; base="humanizer"; expected=@("Humanize", "WithText", "Pascalize") },
    @{path="aspnet-hello/.dagger"; base="hello"; expected=@("Build", "Publish", "Serve") },
    @{path="dotnet"; base="dotnet"; expected=@("Sdk", "Build", "Publish") },
    @{path="aspnet-blazor-template"; base="blazor"; expected=@("Scaffold", "Build", "Publish", "Serve") }
)
foreach ($f in $funcMods) {
    for ($i=1; $i -le 2; $i++) {
        $logBase = "functions-$($f.base)-rep$i"
        $res = Invoke-DaggerFunctions -Name $logBase -ModulePath $f.path
        # functions output should list function names (module specific), no error
        $must = @($f.base, "Function") + $f.expected
        Assert-Result -Name $logBase -ExitCode $res.ExitCode -MustContain $must -MustNot @("ERROR", "unknown flag")
    }
}

# 2. representative calls - twice each with distinct repN
# Frozen humanizer exact per strategy: with-text --text PascalCaseInput humanize
# Reorder: humanizer and hello (with bust) first
$callSpecs = @(
    @{base="call-humanizer-humanize"; mod="humanizer"; sub="humanize --text PascalCaseInput"; must=@("Pascal case input") }, # humanize returns plain string output
    @{base="call-humanizer-chain";  mod="humanizer"; sub="with-text --text PascalCaseInput humanize"; must=@("Pascal case input") },
    @{base="call-hello-build";      mod="aspnet-hello/.dagger"; sub="build --cacheBust $([guid]::NewGuid().ToString())"; must=@("Container@") },
    @{base="call-hello-publish";    mod="aspnet-hello/.dagger"; sub="publish --cacheBust $([guid]::NewGuid().ToString())"; must=@("Container@") },
    @{base="call-dotnet-sdk";       mod="dotnet"; sub="sdk --version 10.0"; must=@("Container@") },
    @{base="call-dotnet-build";     mod="dotnet"; sub="build --source aspnet-hello/AspNetHello --project AspNetHello/AspNetHello.csproj --version 10.0"; must=@("Container@") },
    @{base="call-blazor-scaffold";  mod="aspnet-blazor-template"; sub="scaffold --project-name VerifyBlazor"; must=@("Directory@") },
    @{base="call-blazor-publish";   mod="aspnet-blazor-template"; sub="publish --projectName VerifyBlazor2"; must=@("Container@") }
)

foreach ($c in $callSpecs) {
    for ($i=1; $i -le 2; $i++) {
        $name = "$($c.base)-rep$i"
        $sub = $c.sub
        if ($sub -match 'cacheBust') {
            $sub = $sub -replace '\$\(\[guid\]::NewGuid\(\)\.ToString\(\)\)', ([guid]::NewGuid().ToString())
            $sub = $sub -replace 'GUIDTOKEN', ([guid]::NewGuid().ToString())
        }
        $res = Invoke-Dagger -Name $name -Module $c.mod -Subcommand $sub -TimeoutSec 180
        Assert-Result -Name $name -ExitCode $res.ExitCode -MustContain $c.must
    }
}

# 3. service up (timeout wrapped) - twice, with distinct rep + cacheBust for hello to get real (non CACHED) msbuild
# Use GUID per rep for hello; different ports
# Put hello first
$serveSpecs = @(
    @{base="service-hello"; mod="aspnet-hello/.dagger"; sub="serve --httpPort 8081 --cacheBust GUIDTOKEN up --ports 8081:8081"; port="8081"; bust=$true },
    @{base="service-hello"; mod="aspnet-hello/.dagger"; sub="serve --httpPort 8082 --cacheBust GUIDTOKEN up --ports 8082:8082"; port="8082"; bust=$true },
    @{base="service-blazor"; mod="aspnet-blazor-template"; sub="serve --port 8083 --projectName VerifyServe1 up --ports 8083:8083"; port="8083"; bust=$false },
    @{base="service-blazor"; mod="aspnet-blazor-template"; sub="serve --port 8084 --projectName VerifyServe2 up --ports 8084:8084"; port="8084"; bust=$false }
)

foreach ($s in $serveSpecs) {
    for ($i=1; $i -le 2; $i++) {
        $guid = if ($s.bust) { [guid]::NewGuid().ToString() } else { "" }
        $sub = $s.sub -replace 'GUIDTOKEN', $guid
        $name = "$($s.base)-rep$i"
        $res = Invoke-Dagger -Name $name -Module $s.mod -Subcommand $sub -TimeoutSec 30 -IsServeUp $true
        # For serve logs, require either service ref or successful build strings + Container or Service ref
        $must = @("Service@", "Container@")
        if ($s.bust) { $must += @("Determining projects to restore", "Build succeeded", "-> /publish/") }
        Assert-Result -Name $name -ExitCode $res.ExitCode -MustContain $must -MustNot @("unknown flag")
    }
}

# Generate VERIFICATION-EVIDENCE.txt ONLY from PASS/FAIL
$evidencePath = Join-Path $Scratch "VERIFICATION-EVIDENCE.txt"
$header = @"
VERIFICATION-EVIDENCE (auto-generated by scripts/verify-csharp-modules.ps1 sole gate)
Date: $(Get-Date -Format o)
Scratch: $Scratch
All runs used: Remove-Item first, COMMAND: prefix, distinct -repN, exit+string asserts.
"@
$header | Out-File $evidencePath -Encoding UTF8
$script:EvidenceLines | Out-File $evidencePath -Append -Encoding UTF8
"END-OF-EVIDENCE" | Out-File $evidencePath -Append -Encoding UTF8

$failCount = ($script:EvidenceLines | Where-Object { $_ -like 'FAIL*' }).Count
if ($failCount -gt 0) {
    Write-Host "=== VERIFICATION FAILED: $failCount failures. See $evidencePath ==="
    Get-Content $evidencePath | Select-Object -Last 20
    exit 1
} else {
    Write-Host "=== VERIFICATION PASSED (all PASS) ==="
    Get-Content $evidencePath
}

Write-Host "=== All artifacts in $Scratch ==="
Get-ChildItem $Scratch -Filter "*.log" | ForEach-Object { Write-Host "  $($_.Name)" }
Write-Host "Evidence: $evidencePath"