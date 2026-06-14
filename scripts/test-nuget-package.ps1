[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Version,

    [string] $PackageSource = "artifacts\packages",

    [string] $Configuration = "Release",

    [string] $WorkDirectory = "artifacts\nuget-package-tests",

    [switch] $KeepArtifacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$packageSourcePath = if ([System.IO.Path]::IsPathRooted($PackageSource)) {
    $PackageSource
}
else {
    Join-Path $repoRoot $PackageSource
}
$workRoot = if ([System.IO.Path]::IsPathRooted($WorkDirectory)) {
    $WorkDirectory
}
else {
    Join-Path $repoRoot $WorkDirectory
}
$testRoot = Join-Path $workRoot $Version

$expectedSupportFiles = @(
    "Winook.Lib.Host.x86.exe",
    "Winook.Lib.Host.x64.exe",
    "Winook.Lib.Keyboard.x86.dll",
    "Winook.Lib.Keyboard.x64.dll",
    "Winook.Lib.Mouse.x86.dll",
    "Winook.Lib.Mouse.x64.dll"
)

function Invoke-Checked {
    param(
        [Parameter(Mandatory)][string] $FilePath,
        [Parameter(Mandatory)][string[]] $Arguments,
        [string] $WorkingDirectory = $repoRoot
    )

    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}

function New-TestProject {
    param(
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][string] $TargetFramework,
        [Parameter(Mandatory)][ValidateSet("Exe", "Library")][string] $OutputType
    )

    $projectDirectory = Join-Path $testRoot $Name
    New-Item -ItemType Directory -Path $projectDirectory -Force | Out-Null

    $outputTypeProperty = if ($OutputType -eq "Exe") {
        "    <OutputType>Exe</OutputType>`r`n"
    }
    else {
        ""
    }

    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$TargetFramework</TargetFramework>
$outputTypeProperty    <NoWarn>`$(NoWarn);NETSDK1138</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Winook" Version="$Version" />
  </ItemGroup>
</Project>
"@

    Set-Content -Path (Join-Path $projectDirectory "$Name.csproj") -Value $projectContent -Encoding UTF8

    if ($OutputType -eq "Exe") {
        Set-Content -Path (Join-Path $projectDirectory "Program.cs") -Value @"
using System;
using System.IO;
using Winook;

internal static class Program
{
    private static void Main()
    {
        var supportDirectory = Path.Combine(AppContext.BaseDirectory, "winook.support");
        Console.WriteLine("Winook package smoke test");
        Console.WriteLine("Hook type: " + typeof(MouseHook).FullName);
        Console.WriteLine("Base directory: " + AppContext.BaseDirectory);
        Console.WriteLine("Support directory: " + supportDirectory);
        Console.WriteLine("Support directory exists: " + Directory.Exists(supportDirectory));
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
    }
}
"@ -Encoding UTF8
    }
    else {
        Set-Content -Path (Join-Path $projectDirectory "PackageSmoke.cs") -Value @"
using Winook;

public static class PackageSmoke
{
    public static string HookTypeName => typeof(MouseHook).FullName;
}
"@ -Encoding UTF8
    }

    return $projectDirectory
}

function Assert-SupportFiles {
    param(
        [Parameter(Mandatory)][string] $ProjectDirectory,
        [Parameter(Mandatory)][string] $TargetFramework
    )

    $outputDirectory = Join-Path $ProjectDirectory "bin\$Configuration\$TargetFramework"
    $supportDirectory = Join-Path $outputDirectory "winook.support"

    if (-not (Test-Path -LiteralPath $supportDirectory)) {
        throw "Support directory was not copied: $supportDirectory"
    }

    foreach ($supportFile in $expectedSupportFiles) {
        $path = Join-Path $supportDirectory $supportFile
        if (-not (Test-Path -LiteralPath $path)) {
            throw "Expected support file was not copied: $path"
        }
    }

    Write-Host "Verified $TargetFramework output support files: $supportDirectory"
}

function Assert-PackageLayout {
    param([Parameter(Mandatory)][string] $PackagePath)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        $entries = @($zip.Entries | ForEach-Object FullName)
        if ($entries | Where-Object { $_ -like "contentFiles/*" }) {
            throw "Package still contains contentFiles entries."
        }

        foreach ($supportFile in $expectedSupportFiles) {
            $rid = if ($supportFile -like "*.x86.*") { "win-x86" } else { "win-x64" }
            $expectedEntry = "runtimes/$rid/native/$supportFile"
            if ($entries -notcontains $expectedEntry) {
                throw "Package is missing runtime native asset: $expectedEntry"
            }
        }

        if ($entries -notcontains "buildTransitive/Winook.targets") {
            throw "Package is missing buildTransitive/Winook.targets."
        }

        if ($entries -notcontains "build/net45/Winook.props") {
            throw "Package is missing build/net45/Winook.props."
        }

        Write-Host "Verified package layout: no contentFiles; runtime native assets and build targets are present."
    }
    finally {
        $zip.Dispose()
    }
}

if (-not (Test-Path -LiteralPath $packageSourcePath)) {
    throw "Package source does not exist: $packageSourcePath"
}

$packagePath = Join-Path $packageSourcePath "Winook.$Version.nupkg"
if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Package was not found: $packagePath"
}

if (Test-Path -LiteralPath $testRoot) {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null

try {
    Assert-PackageLayout -PackagePath $packagePath

    $nugetConfig = Join-Path $testRoot "NuGet.config"
    Set-Content -Path $nugetConfig -Value @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="globalPackagesFolder" value="$(Join-Path $testRoot "packages")" />
  </config>
  <packageSources>
    <clear />
    <add key="local" value="$packageSourcePath" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ -Encoding UTF8

    $scenarios = @(
        @{ Name = "Net48TestApp"; TargetFramework = "net48"; OutputType = "Exe" },
        @{ Name = "Net10WindowsTestApp"; TargetFramework = "net10.0-windows"; OutputType = "Exe" },
        @{ Name = "NetStandard20TestLibrary"; TargetFramework = "netstandard2.0"; OutputType = "Library" },
        @{ Name = "NetStandard21TestLibrary"; TargetFramework = "netstandard2.1"; OutputType = "Library" }
    )

    foreach ($scenario in $scenarios) {
        $projectDirectory = New-TestProject `
            -Name $scenario.Name `
            -TargetFramework $scenario.TargetFramework `
            -OutputType $scenario.OutputType

        Invoke-Checked "dotnet" @(
            "build",
            (Join-Path $projectDirectory "$($scenario.Name).csproj"),
            "-c",
            $Configuration,
            "--configfile",
            $nugetConfig
        ) -WorkingDirectory $projectDirectory

        Assert-SupportFiles -ProjectDirectory $projectDirectory -TargetFramework $scenario.TargetFramework

    }

    Write-Host "NuGet package smoke tests passed for Winook $Version."
}
finally {
    if (-not $KeepArtifacts -and (Test-Path -LiteralPath $testRoot)) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
