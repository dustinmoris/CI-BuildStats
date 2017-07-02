# ----------------------------------------------
# Build script
# ----------------------------------------------

param
(
    [switch] $Release,
    [switch] $Run
)

$ErrorActionPreference = "Stop"

# ----------------------------------------------
# Helper functions
# ----------------------------------------------

function Invoke-Cmd ($cmd)
{
    Write-Host $cmd -ForegroundColor DarkCyan
    if ([environment]::OSVersion.Platform -eq "Unix")
    {
        Invoke-Expression -Command $cmd
    }
    else
    {
        $command = "cmd.exe /C $cmd"
        Invoke-Expression -Command $command
        if ($LastExitCode -ne 0) { Write-Error "An error occured when executing '$cmd'."; return }
    }
}

function Write-DotnetVersion
{
    $dotnetVersion = Invoke-Cmd "dotnet --version"
    Write-Host ".NET Core runtime version: $dotnetVersion" -ForegroundColor Cyan
}

function dotnet-restore ($project, $argv) { Invoke-Cmd "dotnet restore $project $argv" }
function dotnet-build   ($project, $argv) { Invoke-Cmd "dotnet build $project $argv" }
function dotnet-run     ($project, $argv) { Invoke-Cmd "dotnet run --project $project $argv" }
function dotnet-test    ($project, $argv) { Invoke-Cmd "dotnet test $project $argv" }
function dotnet-pack    ($project, $argv) { Invoke-Cmd "dotnet pack $project $argv" }

function Test-Version ($project)
{
    if ($env:APPVEYOR_REPO_TAG -eq $true)
    {
        Write-Host "Matching version against git tag..." -ForegroundColor Magenta

        [xml] $xml = Get-Content $project
        [string] $version = $xml.Project.PropertyGroup.Version
        [string] $gitTag  = $env:APPVEYOR_REPO_TAG_NAME

        Write-Host "Project version: $version" -ForegroundColor Cyan
        Write-Host "Git tag version: $gitTag" -ForegroundColor Cyan

        if (!$gitTag.EndsWith($version))
        {
            Write-Error "Version and Git tag do not match."
        }
    }
}

function Update-AppVeyorBuildVersion ($project)
{
    if ($env:APPVEYOR -eq $true)
    {
        Write-Host "Updating AppVeyor build version..." -ForegroundColor Magenta

        [xml]$xml = Get-Content $project
        $version = $xml.Project.PropertyGroup.Version
        $buildVersion = "$version-$env:APPVEYOR_BUILD_NUMBER"
        Write-Host "Setting AppVeyor build version to $buildVersion."
        Update-AppveyorBuild -Version $buildVersion
    }
}

function Remove-OldBuildArtifacts
{
    Write-Host "Deleting old build artifacts..." -ForegroundColor Magenta

    Get-ChildItem -Include "bin", "obj" -Recurse -Directory `
    | ForEach-Object { 
        Write-Host "Removing folder $_" -ForegroundColor DarkGray
        Remove-Item $_ -Recurse -Force }
}

# ----------------------------------------------
# Main
# ----------------------------------------------

$app   = ".\src\BuildStats\BuildStats.fsproj"
$tests = ".\tests\BuildStats.Tests\BuildStats.Tests.fsproj"

Update-AppVeyorBuildVersion $app
Test-Version $app
Write-DotnetVersion
Remove-OldBuildArtifacts

$configuration = if ($Release.IsPresent) { "Release" } else { "Debug" }

Write-Host "Building BuildStats.Core..." -ForegroundColor Magenta
dotnet-restore $app
dotnet-build   $app "-c $configuration"

Write-Host "Building and running BuildStats.Tests..." -ForegroundColor Magenta
dotnet-restore $tests
dotnet-build   $tests
dotnet-test    $tests

if ($Run.IsPresent)
{
    Write-Host "Launching sample application..." -ForegroundColor Magenta
    dotnet-run $app
}