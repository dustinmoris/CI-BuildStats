# ----------------------------------------------
# Build script
# ----------------------------------------------

param
(
    [switch] $Release,
    [switch] $Run,
    [switch] $ExcludeTests,
    [switch] $Docker,
    [switch] $Deploy,
    [string] $DockerUsername,
    [SecureString] $DockerPassword
)

$ErrorActionPreference = "Stop"

Import-module "$PSScriptRoot/.psscripts/build-functions.ps1" -Force

Write-BuildHeader "Starting CI-BuildStats build script"

$app   = "./src/BuildStats/BuildStats.fsproj"
$tests = "./tests/BuildStats.Tests/BuildStats.Tests.fsproj"

$version = Get-ProjectVersion $app
Update-AppVeyorBuildVersion $version

if (Test-IsAppVeyorBuildTriggeredByGitTag)
{
    $gitTag = Get-AppVeyorGitTag
    Test-CompareVersions $version $gitTag
}

Write-DotnetCoreVersions
Remove-OldBuildArtifacts

$configuration = if ($Release.IsPresent -or $Docker.IsPresent -or $Deploy.IsPresent -or $env:APPVEYOR -eq $true) { "Release" } else { "Debug" }

Write-Host "Building application..." -ForegroundColor Magenta
dotnet-build   $app "-c $configuration"

if (!$ExcludeTests.IsPresent)
{
    Write-Host "Building and running tests..." -ForegroundColor Magenta
    dotnet-build   $tests
    dotnet-test    $tests
}

if ($Docker.IsPresent -or $Deploy.IsPresent -or $env:APPVEYOR_REPO_TAG -eq $true)
{
    Write-Host "Publishing web application..." -ForegroundColor Magenta
    dotnet-publish $app "-c $configuration"

    Write-Host "Building Docker image..." -ForegroundColor Magenta
    $publishFolder = "./src/BuildStats/bin/Release/netcoreapp2.0/publish"
    Invoke-Cmd "docker build -t dustinmoris/ci-buildstats:$version $publishFolder"
}

if ($Run.IsPresent)
{
    Write-Host "Launching application..." -ForegroundColor Magenta

    if ($Docker.IsPresent)
    {
        Invoke-Cmd "docker run -p 8080:8080 ci-buildstats:$version"
    }
    else
    {
        dotnet-run $app
    }
}
elseif ($Deploy.IsPresent) # -or $env:APPVEYOR_REPO_TAG -eq $true) AppVeyor doesn't support Linux containers yet
{
    if ([string]::IsNullOrEmpty($DockerUsername) -or [string]::IsNullOrEmpty($DockerPassword))
    {
        Write-Error "Cannot deploy because Docker Hub credentials are missing."
        return
    }

    Write-Host "Deploying Docker image..." -ForegroundColor Magenta
    Invoke-Cmd "docker tag dustinmoris/ci-buildstats:$version dustinmoris/ci-buildstats:latest"
    Invoke-Cmd "docker login -u='$DockerUsername' -p='$DockerPassword'"
    Invoke-Cmd "docker push dustinmoris/ci-buildstats:$version"
    Invoke-Cmd "docker push dustinmoris/ci-buildstats:latest"

    Write-Host "Updating Kubernetes deployment..." -ForegroundColor Magenta
    Invoke-Cmd "kubectl set image deployment/ci-buildstats ci-buildstats=docker.io/dustinmoris/ci-buildstats:$version"
}

Write-SuccessFooter "Build script completed successfully!"