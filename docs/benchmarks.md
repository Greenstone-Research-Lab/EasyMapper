# Performance benchmarks

EasyMapper uses BenchmarkDotNet to compare observable mapping costs without treating a competing
library as the baseline. Every throughput benchmark includes direct property assignments, EasyMapper,
and AutoMapper.

## Methodology

- Release binaries run on .NET 10.
- BenchmarkDotNet controls warmup, measurement, process isolation, and statistical reporting.
- `MemoryDiagnoser` reports managed allocations and garbage collections.
- Convention benchmarks warm both mapping libraries before measurement.
- Cold-start benchmarks include mapper construction and the first execution-plan compilation.
- Collection benchmarks invoke each mapper once per element so collection orchestration is not
  attributed to only one library.
- The configured benchmark names the configuration lifetime explicitly: EasyMapper receives an
  inline lambda, while AutoMapper uses its normal preconfigured type map.
- AutoMapper is referenced only by the benchmark executable and is not a dependency of the
  `Greenstone.EasyMapper` package.

EasyMapper currently performs shallow object mapping. Nested-object and automatic collection-plan
benchmarks will be introduced with those corresponding features rather than simulating unsupported
behavior.

## Run locally

List the available benchmarks without running measurements:

```powershell
dotnet run --project benchmarks/EasyMapper.Benchmarks/EasyMapper.Benchmarks.csproj `
    --configuration Release -- --list flat
```

Run the complete suite:

```powershell
dotnet run --project benchmarks/EasyMapper.Benchmarks/EasyMapper.Benchmarks.csproj `
    --configuration Release -- --filter "*"
```

Run one benchmark class while developing:

```powershell
dotnet run --project benchmarks/EasyMapper.Benchmarks/EasyMapper.Benchmarks.csproj `
    --configuration Release -- --filter "*ConventionMappingBenchmarks*" --job short
```

Reports are written under `BenchmarkDotNet.Artifacts/results`. The repository does not enforce an
absolute timing threshold in pull-request CI because shared runners have variable hardware and load.
The scheduled workflow preserves its reports as build artifacts so changes can be evaluated with
their runtime, operating system, processor, and BenchmarkDotNet metadata. A report-verification step
fails the workflow when BenchmarkDotNet creates an empty or statistics-free report.

## Publish a README baseline

README graphics and tables are generated from successful BenchmarkDotNet JSON reports. Produce the
throughput and cold-start reports on the same machine and then run:

```powershell
./eng/publish-benchmark-summary.ps1 `
    -ThroughputReport ./artifacts/benchmarks/readme/throughput/results/*-report-full.json `
    -ColdStartReport ./artifacts/benchmarks/readme/cold/results/*-report-full.json `
    -OutputDirectory ./docs/benchmark-results `
    -SourceCommit (git rev-parse --short HEAD)
```

Commit all three generated files: the SVG overview, the detailed Markdown report, and the machine-
readable JSON baseline. Never edit the displayed values independently of their source report.

## Interpreting results

Compare distributions and allocations, not a single fastest observation. Results from different
machines or runtime versions are not directly comparable. A claimed regression or improvement should
be reproduced on the same machine, power profile, runtime, benchmark commit, and benchmark filter.
