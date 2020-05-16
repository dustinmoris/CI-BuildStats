# ----------------------------------------------
# Build script
# ----------------------------------------------

param
(
    [switch] $Docker,
    [switch] $Deploy,
    [string] $DockerUsername,
    [string] $DockerPassword
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

$configuration = "Release"

Write-Host "Building application..." -ForegroundColor Magenta
dotnet-build   $app "-c $configuration"

Write-Host "Building and running tests..." -ForegroundColor Magenta
dotnet-build   $tests
dotnet-test    $tests

if ($Docker.IsPresent -or $Deploy.IsPresent -or $env:APPVEYOR_REPO_TAG -eq $true)
{
    $appDir = [System.IO.Path]::GetDirectoryName($app)
    Write-Host "Building Docker image..." -ForegroundColor Magenta
    Invoke-Cmd "docker build -t dustinmoris/ci-buildstats:$version $appDir"
}

if ($Deploy.IsPresent -or $env:APPVEYOR_REPO_TAG -eq $true)
{
    if ([string]::IsNullOrEmpty($DockerUsername) -or [string]::IsNullOrEmpty($DockerPassword))
    {
        if ($env:DOCKER_USERNAME -and $env:DOCKER_PASSWORD)
        {
            $DockerUsername = $env:DOCKER_USERNAME
            $DockerPassword = $env:DOCKER_PASSWORD
        }
        else
        {
            Write-Error "Cannot deploy because Docker Hub credentials are missing."
            return
        }
    }

    Write-Host "Deploying Docker image..." -ForegroundColor Magenta
    Invoke-Cmd "docker tag dustinmoris/ci-buildstats:$version dustinmoris/ci-buildstats:latest"
    Invoke-Cmd "docker login -u='$DockerUsername' -p='$DockerPassword'"
    Invoke-Cmd "docker push dustinmoris/ci-buildstats:$version"
    Invoke-Cmd "docker push dustinmoris/ci-buildstats:latest"
}

Write-SuccessFooter "Build script completed successfully!"