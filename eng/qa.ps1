[CmdletBinding()]
param(
    [switch]$SkipLoadTest
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $repositoryRoot 'artifacts'
$coverage = Join-Path $artifacts 'coverage'
$packages = Join-Path $artifacts 'packages'

New-Item -ItemType Directory -Force -Path $coverage, $packages | Out-Null

dotnet restore "$repositoryRoot\EasyMapper.slnx" --configfile "$repositoryRoot\NuGet.Config"
if ($LASTEXITCODE -ne 0) { throw "Restore failed with exit code $LASTEXITCODE." }
dotnet build "$repositoryRoot\EasyMapper.slnx" --configuration Release --no-restore --nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }
dotnet format "$repositoryRoot\EasyMapper.slnx" --verify-no-changes --no-restore --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "Formatting or analyzer verification failed with exit code $LASTEXITCODE." }
dotnet test "$repositoryRoot\tests\EasyMapper.Tests\EasyMapper.Tests.csproj" `
    --configuration Release `
    --no-build `
    --nologo `
    /p:CollectCoverage=true `
    /p:CoverletOutput="$coverage\" `
    /p:CoverletOutputFormat=json `
    /p:Threshold=99 `
    /p:ThresholdType=line%2cbranch%2cmethod `
    /p:ThresholdStat=total
if ($LASTEXITCODE -ne 0) { throw "Unit tests or coverage gate failed with exit code $LASTEXITCODE." }
dotnet test "$repositoryRoot\tests\EasyMapper.SmokeTests\EasyMapper.SmokeTests.csproj" `
    --configuration Release `
    --no-build `
    --nologo
if ($LASTEXITCODE -ne 0) { throw "Smoke tests failed with exit code $LASTEXITCODE." }
dotnet pack "$repositoryRoot\src\EasyMapper\EasyMapper.csproj" `
    --configuration Release `
    --no-build `
    --nologo `
    --output $packages
if ($LASTEXITCODE -ne 0) { throw "NuGet pack failed with exit code $LASTEXITCODE." }

& "$PSScriptRoot\verify-package.ps1" -PackageDirectory $packages -Version '0.1.0-alpha.1'

if (-not $SkipLoadTest)
{
    dotnet test "$repositoryRoot\tests\EasyMapper.LoadTests\EasyMapper.LoadTests.csproj" `
        --configuration Release `
        --no-build `
        --nologo
    if ($LASTEXITCODE -ne 0) { throw "Load tests failed with exit code $LASTEXITCODE." }
}
