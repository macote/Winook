[CmdletBinding()]
param(
    [string] $Version,

    [string] $Configuration = "Release",

    [string] $Source = "https://api.nuget.org/v3/index.json",

    [string] $ApiKey = $env:NUGET_API_KEY,

    [string] $OutputDirectory = "artifacts\packages",

    [switch] $SkipNativeBuild,

    [switch] $SkipManagedBuild,

    [switch] $NoPush,

    [switch] $AllowDirty
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\Winook\Winook.csproj"
$solutionPath = Join-Path $repoRoot "Winook.slnx"
$outputPath = Join-Path $repoRoot $OutputDirectory

function Invoke-Checked {
    param(
        [Parameter(Mandatory)][string] $FilePath,
        [Parameter(Mandatory)][string[]] $Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
    }
}

function Get-ProjectPackageVersion {
    [xml] $project = Get-Content -LiteralPath $projectPath
    return $project.Project.PropertyGroup.PackageVersion
}

function Test-PackageReadme {
    param([Parameter(Mandatory)][string] $PackagePath)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -eq "Winook.nuspec" } | Select-Object -First 1
        if (-not $nuspecEntry) {
            throw "Package does not contain Winook.nuspec."
        }

        $reader = [System.IO.StreamReader]::new($nuspecEntry.Open())
        try {
            [xml] $nuspec = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        $readmeNode = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='readme']")
        if (-not $readmeNode -or [string]::IsNullOrWhiteSpace($readmeNode.InnerText)) {
            throw "Package nuspec does not declare an embedded readme."
        }

        $readmePath = $readmeNode.InnerText.Trim()
        $readmeEntry = $zip.Entries | Where-Object { $_.FullName -eq $readmePath } | Select-Object -First 1
        if (-not $readmeEntry) {
            throw "Package declares readme '$readmePath' but the file is missing from the package."
        }

        Write-Host "Verified embedded package readme: $readmePath"
    }
    finally {
        $zip.Dispose()
    }
}

if (-not $AllowDirty) {
    $status = git -C $repoRoot status --porcelain
    if ($status) {
        throw "Working tree has uncommitted changes. Commit or stash them, or pass -AllowDirty."
    }
}

if (-not (Test-Path -LiteralPath $projectPath)) {
    throw "Project not found: $projectPath"
}

$packageVersion = if ($Version) { $Version } else { Get-ProjectPackageVersion }
if ([string]::IsNullOrWhiteSpace($packageVersion)) {
    throw "Package version was not provided and could not be read from $projectPath."
}

if (-not $SkipNativeBuild) {
    Invoke-Checked "powershell" @(
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        (Join-Path $repoRoot "scripts\build-msys.ps1"),
        "-Config",
        "release",
        "-Clean"
    )
}

if (-not $SkipManagedBuild) {
    Invoke-Checked "dotnet" @("build", $solutionPath, "-c", $Configuration)
}

New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

$packArguments = @("pack", $projectPath, "-c", $Configuration, "-o", $outputPath)
if ($SkipManagedBuild) {
    $packArguments += "--no-build"
}
if ($Version) {
    $packArguments += "/p:PackageVersion=$Version"
    $packArguments += "/p:Version=$Version"
}

Invoke-Checked "dotnet" $packArguments

$packagePath = Join-Path $outputPath "Winook.$packageVersion.nupkg"
if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Expected package was not created: $packagePath"
}

Test-PackageReadme -PackagePath $packagePath

if ($NoPush) {
    Write-Host "Created package without pushing: $packagePath"
    exit 0
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "NuGet API key was not provided. Set NUGET_API_KEY or pass -ApiKey."
}

Invoke-Checked "dotnet" @(
    "nuget",
    "push",
    $packagePath,
    "--api-key",
    $ApiKey,
    "--source",
    $Source
)

Write-Host "Published package: $packagePath"
