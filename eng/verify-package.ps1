[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDirectory,

    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$consumerRoot = Join-Path $repositoryRoot 'artifacts\package-consumer'

if (Test-Path -LiteralPath $consumerRoot)
{
    Remove-Item -LiteralPath $consumerRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $consumerRoot | Out-Null

$project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsTestProject>true</IsTestProject>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="EasyMapper" Version="$Version" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" PrivateAssets="all" />
  </ItemGroup>
</Project>
"@

$program = @'
using EasyMapper;
using Xunit;

namespace PackageConsumer;

public sealed class InstalledPackageTests
{
    [Fact]
    public void InstalledPackageMapsAConsumerType()
    {
        var result = new Source { Name = "package-smoke" }.MapTo<Destination>();
        Assert.Equal("package-smoke", result.Name);
    }
}

sealed class Source
{
    public string Name { get; set; } = string.Empty;
}

sealed class Destination
{
    public string Name { get; set; } = string.Empty;
}
'@

$nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-easymapper" value="$PackageDirectory" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
"@

Set-Content -LiteralPath (Join-Path $consumerRoot 'PackageConsumer.csproj') -Value $project -Encoding utf8
Set-Content -LiteralPath (Join-Path $consumerRoot 'Program.cs') -Value $program -Encoding utf8
Set-Content -LiteralPath (Join-Path $consumerRoot 'NuGet.Config') -Value $nugetConfig -Encoding utf8

dotnet restore (Join-Path $consumerRoot 'PackageConsumer.csproj') `
    --configfile (Join-Path $consumerRoot 'NuGet.Config')
if ($LASTEXITCODE -ne 0)
{
    throw "Package consumer restore failed with exit code $LASTEXITCODE."
}

dotnet test `
    (Join-Path $consumerRoot 'PackageConsumer.csproj') `
    --configuration Release `
    --no-restore `
    --nologo
if ($LASTEXITCODE -ne 0)
{
    throw "Package consumer test failed with exit code $LASTEXITCODE."
}
