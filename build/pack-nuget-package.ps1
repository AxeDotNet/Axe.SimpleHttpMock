# & '..\.nuget\nuget.exe' 'pack' '..\src\Axe.SimpleHttpMock\Axe.SimpleHttpMock.csproj' '-build' '-prop' 'Configuration=Release'

$ProjectRoot = split-path "$PSScriptRoot"
$Net45SolutionPath = "$ProjectRoot/Axe.SimpleHttpMock.sln"
$CoreLibraryPath = "$ProjectRoot/src/Axe.SimpleHttpMock.NetCore"
$CoreTestPath = "$ProjectRoot/test/Axe.SimpleHttpMock.NetCore.Test"

function build_for_net45() {
    Import-Module -Name './modules/Invoke-MsBuild.psm1'

    Write-Host "Build project for dotnet framework 4.5..." -ForegroundColor Green
    $buildResult = Invoke-MsBuild -Path $Net45SolutionPath -MsBuildParameters "/target:Clean;Build /property:Configuration=Release /verbosity:m" -ShowBuildOutputInCurrentWindow
    if ($buildResult.BuildSucceeded -eq $true) {
        Write-Host "Build succeeded!" -ForegroundColor Green
    } else {
        Write-Host "Build failed!" -ForegroundColor Red
        return 1;
    }
}

function build_for_dotnet_core() {
    Write-Host "Restoring for dotnet core projects..." -ForegroundColor Green
    Invoke-Expression "dotnet restore '$ProjectRoot'"
    Write-Host "Restoring complete" -ForegroundColor Green

    Write-Host "Build dotnet core projects..." -ForegroundColor Green
    Invoke-Expression "dotnet build --configuration Release '$CoreLibraryPath'"
    Invoke-Expression "dotnet build --configuration Release '$CoreTestPath'"
    Invoke-Expression "dotnet test --configuration Release '$CoreTestPath'"
    Write-Host "Build complete" -ForegroundColor Green
}

build_for_dotnet_core
build_for_net45