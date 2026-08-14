[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ThroughputReport,

    [Parameter(Mandatory)]
    [string]$ColdStartReport,

    [Parameter(Mandatory)]
    [string]$OutputDirectory,

    [Parameter(Mandatory)]
    [string]$SourceCommit
)

$ErrorActionPreference = 'Stop'
$invariantCulture = [System.Globalization.CultureInfo]::InvariantCulture
$ThroughputReport = (Resolve-Path -Path $ThroughputReport).Path
$ColdStartReport = (Resolve-Path -Path $ColdStartReport).Path

function Get-BenchmarkResult
{
    param(
        [Parameter(Mandatory)]
        [object]$Report,

        [Parameter(Mandatory)]
        [string]$Type,

        [Parameter(Mandatory)]
        [string]$Method,

        [string]$Parameters = ''
    )

    $matches = @($Report.Benchmarks | Where-Object {
        $_.Type -eq $Type -and
        $_.Method -eq $Method -and
        ([string]::IsNullOrEmpty($Parameters) -or $_.Parameters -eq $Parameters)
    })

    if ($matches.Count -ne 1 -or $null -eq $matches[0].Statistics)
    {
        throw "Expected one successful result for $Type.$Method ($Parameters), found $($matches.Count)."
    }

    $label = switch -Wildcard ($Method)
    {
        'Manual' { 'Manual' }
        'EasyMapper*' { 'EasyMapper' }
        'AutoMapper*' { 'AutoMapper' }
        default { $Method }
    }

    return [pscustomobject][ordered]@{
        label = $label
        method = $Method
        meanNanoseconds = [math]::Round([double]$matches[0].Statistics.Mean, 3)
        allocatedBytes = [long]$matches[0].Memory.BytesAllocatedPerOperation
    }
}

function Format-Latency
{
    param([double]$Nanoseconds)

    if ($Nanoseconds -ge 1000000)
    {
        return [string]::Format($invariantCulture, '{0:N2} ms', $Nanoseconds / 1000000)
    }

    if ($Nanoseconds -ge 1000)
    {
        return [string]::Format($invariantCulture, '{0:N2} us', $Nanoseconds / 1000)
    }

    return [string]::Format($invariantCulture, '{0:N2} ns', $Nanoseconds)
}

function Format-Bytes
{
    param([long]$Bytes)

    if ($Bytes -ge 1024)
    {
        return [string]::Format($invariantCulture, '{0:N2} KB', $Bytes / 1024)
    }

    return "$Bytes B"
}

function New-ChartPanel
{
    param(
        [Parameter(Mandatory)]
        [string]$Title,

        [Parameter(Mandatory)]
        [string]$Subtitle,

        [Parameter(Mandatory)]
        [int]$X,

        [Parameter(Mandatory)]
        [object[]]$Items
    )

    $colors = @('#94a3b8', '#34d399', '#a78bfa')
    $maximum = ($Items | Measure-Object -Property meanNanoseconds -Maximum).Maximum
    $panel = [System.Text.StringBuilder]::new()
    [void]$panel.AppendLine("  <rect x='$X' y='92' width='360' height='326' rx='20' fill='#0f172a' stroke='#334155'/>")
    [void]$panel.AppendLine("  <text x='$($X + 24)' y='132' class='panel-title'>$Title</text>")
    [void]$panel.AppendLine("  <text x='$($X + 24)' y='158' class='subtitle'>$Subtitle</text>")

    for ($index = 0; $index -lt $Items.Count; $index++)
    {
        $item = $Items[$index]
        $rowY = 198 + ($index * 68)
        $barY = $rowY + 14
        $barWidth = [math]::Max(4, [math]::Round(([double]$item.meanNanoseconds / $maximum) * 310, 1))
        $value = Format-Latency -Nanoseconds $item.meanNanoseconds
        [void]$panel.AppendLine("  <text x='$($X + 24)' y='$rowY' class='label'>$($item.label)</text>")
        [void]$panel.AppendLine("  <text x='$($X + 336)' y='$rowY' text-anchor='end' class='value'>$value</text>")
        [void]$panel.AppendLine("  <rect x='$($X + 24)' y='$barY' width='310' height='12' rx='6' fill='#1e293b'/>")
        [void]$panel.AppendLine("  <rect x='$($X + 24)' y='$barY' width='$barWidth' height='12' rx='6' fill='$($colors[$index])'/>")
    }

    return $panel.ToString()
}

$throughput = Get-Content -LiteralPath $ThroughputReport -Raw | ConvertFrom-Json
$coldStart = Get-Content -LiteralPath $ColdStartReport -Raw | ConvertFrom-Json

$scenarios = [ordered]@{
    convention = @(
        Get-BenchmarkResult -Report $throughput -Type 'ConventionMappingBenchmarks' -Method 'Manual'
        Get-BenchmarkResult -Report $throughput -Type 'ConventionMappingBenchmarks' -Method 'EasyMapper_CachedPlan'
        Get-BenchmarkResult -Report $throughput -Type 'ConventionMappingBenchmarks' -Method 'AutoMapper_CachedPlan'
    )
    existingDestination = @(
        Get-BenchmarkResult -Report $throughput -Type 'ExistingDestinationBenchmarks' -Method 'Manual'
        Get-BenchmarkResult -Report $throughput -Type 'ExistingDestinationBenchmarks' -Method 'EasyMapper_CachedPlan'
        Get-BenchmarkResult -Report $throughput -Type 'ExistingDestinationBenchmarks' -Method 'AutoMapper_CachedPlan'
    )
    configured = @(
        Get-BenchmarkResult -Report $throughput -Type 'ConfiguredMappingBenchmarks' -Method 'Manual'
        Get-BenchmarkResult -Report $throughput -Type 'ConfiguredMappingBenchmarks' -Method 'EasyMapper_InlineLambda'
        Get-BenchmarkResult -Report $throughput -Type 'ConfiguredMappingBenchmarks' -Method 'AutoMapper_Preconfigured'
    )
    collection1000 = @(
        Get-BenchmarkResult -Report $throughput -Type 'CollectionMappingBenchmarks' -Method 'Manual' -Parameters 'Count=1000'
        Get-BenchmarkResult -Report $throughput -Type 'CollectionMappingBenchmarks' -Method 'EasyMapper_CachedPlan' -Parameters 'Count=1000'
        Get-BenchmarkResult -Report $throughput -Type 'CollectionMappingBenchmarks' -Method 'AutoMapper_CachedPlan' -Parameters 'Count=1000'
    )
    coldStart = @(
        Get-BenchmarkResult -Report $coldStart -Type 'ColdStartBenchmarks' -Method 'Manual'
        Get-BenchmarkResult -Report $coldStart -Type 'ColdStartBenchmarks' -Method 'EasyMapper_FirstMap'
        Get-BenchmarkResult -Report $coldStart -Type 'ColdStartBenchmarks' -Method 'AutoMapper_FirstMap'
    )
}

$published = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ', $invariantCulture)
    sourceCommit = $SourceCommit
    benchmarkDotNet = $throughput.HostEnvironmentInfo.BenchmarkDotNetVersion
    runtime = $throughput.HostEnvironmentInfo.RuntimeVersion
    sdk = $throughput.HostEnvironmentInfo.DotNetCliVersion
    operatingSystem = $throughput.HostEnvironmentInfo.OsVersion
    processor = $throughput.HostEnvironmentInfo.ProcessorName
    throughputJob = 'ShortRun: 1 launch, 3 warmup iterations, 3 measurement iterations'
    coldStartJob = '1 invocation, 5 warmup iterations, 15 measurement iterations'
    scenarios = $scenarios
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$published | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'latest-results.json') -Encoding utf8

$svg = [System.Text.StringBuilder]::new()
[void]$svg.AppendLine('<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="470" viewBox="0 0 1200 470" role="img" aria-labelledby="title description">')
[void]$svg.AppendLine('  <title id="title">EasyMapper performance overview</title>')
[void]$svg.AppendLine('  <desc id="description">BenchmarkDotNet latency comparison of manual mapping, EasyMapper, and AutoMapper. Lower values are better.</desc>')
[void]$svg.AppendLine('  <defs>')
[void]$svg.AppendLine('    <linearGradient id="background" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#020617"/><stop offset="1" stop-color="#172554"/></linearGradient>')
[void]$svg.AppendLine('    <style>.title{font:700 28px Segoe UI,Arial,sans-serif;fill:#f8fafc}.kicker{font:600 12px Segoe UI,Arial,sans-serif;fill:#34d399;letter-spacing:1.8px}.panel-title{font:700 18px Segoe UI,Arial,sans-serif;fill:#f8fafc}.subtitle{font:400 12px Segoe UI,Arial,sans-serif;fill:#94a3b8}.label{font:600 13px Segoe UI,Arial,sans-serif;fill:#cbd5e1}.value{font:700 13px Consolas,monospace;fill:#f8fafc}.footer{font:400 11px Segoe UI,Arial,sans-serif;fill:#94a3b8}</style>')
[void]$svg.AppendLine('  </defs>')
[void]$svg.AppendLine('  <rect width="1200" height="470" rx="24" fill="url(#background)"/>')
[void]$svg.AppendLine('  <text x="28" y="38" class="kicker">BENCHMARKDOTNET | LOWER IS BETTER</text>')
[void]$svg.AppendLine('  <text x="28" y="72" class="title">EasyMapper performance overview</text>')
[void]$svg.Append((New-ChartPanel -Title 'Cached convention map' -Subtitle 'single flat object | warm plan' -X 28 -Items $scenarios.convention))
[void]$svg.Append((New-ChartPanel -Title 'First usable map' -Subtitle 'construction + first plan compilation' -X 420 -Items $scenarios.coldStart))
[void]$svg.Append((New-ChartPanel -Title '1,000 element maps' -Subtitle 'same cached element plan in a loop' -X 812 -Items $scenarios.collection1000))
[void]$svg.AppendLine("  <text x='28' y='450' class='footer'>.NET 10.0.9 | AMD Ryzen 7 8845HS | Windows 11 | ShortRun | source $SourceCommit</text>")
[void]$svg.AppendLine('</svg>')
$svg.ToString() | Set-Content -LiteralPath (Join-Path $OutputDirectory 'benchmark-overview.svg') -Encoding utf8

$markdown = [System.Text.StringBuilder]::new()
[void]$markdown.AppendLine('# Latest benchmark results')
[void]$markdown.AppendLine()
[void]$markdown.AppendLine('![EasyMapper performance overview](benchmark-overview.svg)')
[void]$markdown.AppendLine()
[void]$markdown.AppendLine('Lower latency is better. Allocated bytes are managed allocations per benchmark operation.')

foreach ($scenario in @(
    [ordered]@{ title = 'Cached convention mapping'; values = $scenarios.convention },
    [ordered]@{ title = 'Existing destination'; values = $scenarios.existingDestination },
    [ordered]@{ title = 'Inline configured mapping'; values = $scenarios.configured },
    [ordered]@{ title = '1,000 element maps'; values = $scenarios.collection1000 },
    [ordered]@{ title = 'First usable map'; values = $scenarios.coldStart }
))
{
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("## $($scenario.title)")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine('| Implementation | Method | Mean | Allocated |')
    [void]$markdown.AppendLine('|---|---|---:|---:|')
    foreach ($item in $scenario.values)
    {
        [void]$markdown.AppendLine("| $($item.label) | ``$($item.method)`` | $(Format-Latency $item.meanNanoseconds) | $(Format-Bytes $item.allocatedBytes) |")
    }
}

[void]$markdown.AppendLine()
[void]$markdown.AppendLine('## Environment')
[void]$markdown.AppendLine()
[void]$markdown.AppendLine("- Source commit: ``$SourceCommit``")
[void]$markdown.AppendLine("- BenchmarkDotNet: $($published.benchmarkDotNet)")
[void]$markdown.AppendLine("- Runtime: $($published.runtime)")
[void]$markdown.AppendLine("- SDK: $($published.sdk)")
[void]$markdown.AppendLine("- OS: $($published.operatingSystem)")
[void]$markdown.AppendLine("- Processor: $($published.processor)")
[void]$markdown.AppendLine('- Throughput job: ShortRun (three warmup and three measurement iterations)')
[void]$markdown.AppendLine('- Cold-start job: one invocation per iteration, five warmups and fifteen measurements')
[void]$markdown.AppendLine()
[void]$markdown.AppendLine('These measurements are a transparent baseline, not a cross-machine performance guarantee. The inline EasyMapper lambda result includes isolated plan construction on every call and identifies a current optimization target.')

$markdown.ToString() | Set-Content -LiteralPath (Join-Path $OutputDirectory 'README.md') -Encoding utf8

Write-Host "Published benchmark summary to '$OutputDirectory'."
