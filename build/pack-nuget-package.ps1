

$ProjectRoot = "$PSScriptRoot/project"
$LibraryDirectory = "$ProjectRoot/Axe.SimpleHttpMock"
$TestDirectory = "$ProjectRoot/Axe.SimpleHttpMock.Test"

function build_solution() {
    Write-Host "Start building solution..." -ForegroundColor Green
    Write-Host "Restoring package dependencies..." -ForegroundColor Green
    Invoke-Expression "dotnet restore '$ProjectRoot'"
    Write-Host "Restoring complete" -ForegroundColor Green

    Write-Host "Build projects..." -ForegroundColor Green
    Invoke-Expression "dotnet build --configuration Release '$LibraryDirectory'"
    Invoke-Expression "dotnet build --configuration Release '$TestDirectory'"
    Write-Host "Build complete" -ForegroundColor Green

    Write-Host "Running test for all targeting frameworks..." -ForegroundColor Green
    Invoke-Expression "dotnet test --configuration Release '$TestDirectory'"
    Write-Host "Build succeeded"
}

function build_nuget_package() {
    Write-Host "Start packaging..." -ForegroundColor Green
    Invoke-Expression "dotnet pack --configuration Release '$LibraryDirectory'"
    Write-Host "Package complete" -ForegroundColor Green
}

build_solution
build_nuget_package