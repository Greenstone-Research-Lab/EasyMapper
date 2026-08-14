[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ResultsDirectory
)

$ErrorActionPreference = 'Stop'
$resolvedResultsDirectory = Resolve-Path -LiteralPath $ResultsDirectory
$reports = @(Get-ChildItem -LiteralPath $resolvedResultsDirectory -Filter '*-report-full.json' -File)

if ($reports.Count -eq 0)
{
    throw "No BenchmarkDotNet JSON reports were found in '$resolvedResultsDirectory'."
}

$benchmarkCount = 0
foreach ($report in $reports)
{
    $content = Get-Content -LiteralPath $report.FullName -Raw | ConvertFrom-Json
    $benchmarks = @($content.Benchmarks)
    if ($benchmarks.Count -eq 0)
    {
        throw "Benchmark report '$($report.Name)' contains no benchmark results."
    }

    $failedBenchmarks = @($benchmarks | Where-Object { $null -eq $_.Statistics })
    if ($failedBenchmarks.Count -gt 0)
    {
        $failedNames = ($failedBenchmarks.FullName -join ', ')
        throw "Benchmark report '$($report.Name)' has no statistics for: $failedNames."
    }

    $benchmarkCount += $benchmarks.Count
}

Write-Host "Validated $benchmarkCount benchmark results across $($reports.Count) reports."
