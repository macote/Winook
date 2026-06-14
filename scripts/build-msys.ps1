[CmdletBinding()]
param(
    [ValidateSet("release", "debug")]
    [string] $Config = "release",

    [string] $MsysRoot = $(
        if (Test-Path "C:\msys64") {
            "C:\msys64"
        }
        elseif ($env:MSYS2_ROOT) {
            $env:MSYS2_ROOT
        }
        else {
            ""
        }
    ),

    [switch] $Clean,

    [switch] $SkipCopy
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $MsysRoot) {
    throw "MSYS2 was not found at 'C:\msys64' and MSYS2_ROOT is not set. Pass -MsysRoot or set MSYS2_ROOT."
}

$msysShell = Join-Path $MsysRoot "msys2_shell.cmd"

if (-not (Test-Path $msysShell)) {
    throw "MSYS2 shell launcher was not found at '$msysShell'. Pass -MsysRoot or set MSYS2_ROOT."
}

function ConvertTo-MsysPath {
    param([Parameter(Mandatory)][string] $Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath -match "^([A-Za-z]):\\(.*)$") {
        $drive = $Matches[1].ToLowerInvariant()
        $rest = $Matches[2] -replace "\\", "/"
        return "/$drive/$rest"
    }

    return $fullPath -replace "\\", "/"
}

function Quote-BashArgument {
    param([Parameter(Mandatory)][string] $Value)

    return "'" + ($Value -replace "'", "'\''") + "'"
}

function Invoke-MsysMake {
    param(
        [Parameter(Mandatory)][string] $ProjectPath,
        [Parameter(Mandatory)][ValidateSet("x86", "x64")][string] $Architecture,
        [switch] $RunClean
    )

    if ($Architecture -eq "x86") {
        $shellType = "-mingw32"
        $toolchain = "/mingw32"
    }
    else {
        $shellType = "-ucrt64"
        $toolchain = "/ucrt64"
    }

    $projectMsysPath = ConvertTo-MsysPath $ProjectPath
    $toolchainWinPath = Join-Path $MsysRoot ($toolchain.TrimStart("/") -replace "/", "\")
    $preprocessorScript = [System.IO.Path]::GetTempFileName() + ".cmd"
    $preprocessorForWindres = $preprocessorScript -replace "\\", "/"
    $makeCommand = "make config=$Config CC=$toolchain/bin/g++ WINDRES='windres --preprocessor=$preprocessorForWindres --preprocessor-arg=-E --preprocessor-arg=-xc-header --preprocessor-arg=-DRC_INVOKED'"
    $commandLines = @(
        "set -e",
        "cd $(Quote-BashArgument $projectMsysPath)"
    )
    if ($RunClean) {
        $commandLines += "make clean"
    }
    $commandLines += $makeCommand
    $command = $commandLines -join "; "
    $escapedCommand = $command.Replace('"', '\"')

    Write-Host "Building $Architecture $(Split-Path -Leaf $ProjectPath)"
    try {
        Set-Content -Path $preprocessorScript -Encoding ASCII -Value @(
            "@echo off",
            "`"$toolchainWinPath\bin\gcc.exe`" %*"
        )

        $startInfo = New-Object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = $msysShell
        $startInfo.Arguments = "-no-start -defterm $shellType -shell bash.exe -c `"$escapedCommand`""
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true

        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $startInfo
        [void] $process.Start()
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()

        if ($stdout) {
            Write-Host $stdout.TrimEnd()
        }
        if ($stderr) {
            [Console]::Error.WriteLine($stderr.TrimEnd())
        }
        if ($process.ExitCode -ne 0) {
            throw "MSYS build failed for $Architecture $(Split-Path -Leaf $ProjectPath)."
        }
    }
    finally {
        Remove-Item -Path $preprocessorScript -Force -ErrorAction SilentlyContinue
    }
}

$libPath = Join-Path $repoRoot "src\Winook.Lib"
$hostPath = Join-Path $repoRoot "src\Winook.Lib.Host"
$supportPath = Join-Path $repoRoot "src\Winook\winook.support"

Invoke-MsysMake -ProjectPath $libPath -Architecture x86 -RunClean:$Clean
Invoke-MsysMake -ProjectPath $hostPath -Architecture x86 -RunClean:$Clean
Invoke-MsysMake -ProjectPath $libPath -Architecture x64
Invoke-MsysMake -ProjectPath $hostPath -Architecture x64

if (-not $SkipCopy) {
    New-Item -ItemType Directory -Path $supportPath -Force | Out-Null

    $outputs = @(
        @{ Source = Join-Path $libPath "bin\Winook.Lib.Keyboard.x86.dll"; Destination = $supportPath },
        @{ Source = Join-Path $libPath "bin\Winook.Lib.Mouse.x86.dll"; Destination = $supportPath },
        @{ Source = Join-Path $hostPath "bin\Winook.Lib.Host.x86.exe"; Destination = $supportPath },
        @{ Source = Join-Path $libPath "bin\Winook.Lib.Keyboard.x64.dll"; Destination = $supportPath },
        @{ Source = Join-Path $libPath "bin\Winook.Lib.Mouse.x64.dll"; Destination = $supportPath },
        @{ Source = Join-Path $hostPath "bin\Winook.Lib.Host.x64.exe"; Destination = $supportPath }
    )

    foreach ($output in $outputs) {
        if (-not (Test-Path $output.Source)) {
            throw "Expected build output was not found: $($output.Source)"
        }

        Copy-Item -Path $output.Source -Destination $output.Destination -Force
    }

    Write-Host "Copied native binaries to $supportPath"
}

Write-Host "MSYS native build completed."
