param (
    [string]$Configuration = "Release"
)

. ./version-config.ps1

$ErrorActionPreference = 'Stop'

$RootPath = Split-Path -Path "$PSScriptRoot" -Parent
$ProjectRoot = "$PSScriptRoot/project"
$LibraryDirectory = "$ProjectRoot/Axe.SimpleHttpMock"
$TestDirectory = "$ProjectRoot/Axe.SimpleHttpMock.Test"

function compileSolution() {
    Write-Host "Start building solution with following informations" -ForegroundColor Green
    Write-Host "==================================================="
    Write-Host "Configuration       $Configuration"
    Write-Host "ProjectRoot         $ProjectRoot"
    Write-Host "LibraryDirectory    $LibraryDirectory"
    Write-Host "TestDirectory       $TestDirectory"
    Write-Host "==================================================="

    Write-Host "Restoring package dependencies..." -ForegroundColor Green
    Invoke-Expression "dotnet restore '$ProjectRoot'"
    Write-Host "Restoring complete" -ForegroundColor Green

    Write-Host "Build projects..." -ForegroundColor Green
    Invoke-Expression "dotnet build --configuration $Configuration '$LibraryDirectory'"
    Invoke-Expression "dotnet build --configuration $Configuration '$TestDirectory'"
    Write-Host "Build complete" -ForegroundColor Green

    Write-Host "Running test for all targeting frameworks..." -ForegroundColor Green
    Invoke-Expression "dotnet test --configuration $Configuration '$TestDirectory'"
    Write-Host "Build succeeded"
}

function createPackage() {
    Write-Host "Start packaging..." -ForegroundColor Green
    $expression = "dotnet pack --configuration $Configuration '$LibraryDirectory'"
    $versionConfig = getVersionConfiguration;
    if ($versionConfig.preRelease -eq $true) {
        $buildNumber = $versionConfig.buildNumber
        $expression = $expression + " --version-suffix 'beta-build$buildNumber'"
    }
    Invoke-Expression $expression
    Write-Host "Package complete" -ForegroundColor Green
}

function updateAssemblyInfoVersion() {
    Write-Host "Update assembly version to" -ForegroundColor Green
    $assemblyInfoContent = [System.IO.File]::ReadAllText("$PSScriptRoot/template/AssemblyInfo.cs");
    $assemblyVersionStr = getVersionString('ASSEMBLY_VERSION');
    $assemblyFileVersionStr = getVersionString('ASSEMBLY_FILE_VERSION')
    Write-Host "Assembly Version       $assemblyVersionStr"
    Write-Host "Assembly File Version  $assemblyFileVersionStr"

    $assemblyInfoWithVersion = $assemblyInfoContent.Replace('{ASSEMBLY_VERSION}', $assemblyVersionStr).Replace('{ASSEMBLY_FILE_VERSION}', $assemblyFileVersionStr)
    [System.IO.File]::WriteAllText("$RootPath/src/Axe.SimpleHttpMock/Properties/AssemblyInfo.cs", $assemblyInfoWithVersion)
    Write-Host "Update complete" -ForegroundColor Green
}

function updatePackageVersion() {
    Write-Host "Update package version to" -ForegroundColor Green
    $packageVersionStr = getVersionString('PACKAGE_VERSION');
    Write-Host "Package Version        $packageVersionStr"

    $projectPath = "$LibraryDirectory/project.json";
    $projectString = [System.IO.File]::ReadAllText($projectPath);
    $projectJson = $projectString | ConvertFrom-Json
    $projectJson.version = $packageVersionStr
    $updatedString = $projectJson | ConvertTo-Json -Depth 10 -Compress
    [System.IO.File]::WriteAllText($projectPath, $updatedString)
    Write-Host "Update complete" -ForegroundColor Green
}

updateAssemblyInfoVersion
updatePackageVersion
compileSolution
createPackage