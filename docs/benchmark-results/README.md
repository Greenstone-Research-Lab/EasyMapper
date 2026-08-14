# Latest benchmark results

![EasyMapper performance overview](benchmark-overview.svg)

Lower latency is better. Allocated bytes are managed allocations per benchmark operation.

## Cached convention mapping

| Implementation | Method | Mean | Allocated |
|---|---|---:|---:|
| Manual | `Manual` | 7.89 ns | 96 B |
| EasyMapper | `EasyMapper_CachedPlan` | 23.15 ns | 96 B |
| AutoMapper | `AutoMapper_CachedPlan` | 51.84 ns | 96 B |

## Existing destination

| Implementation | Method | Mean | Allocated |
|---|---|---:|---:|
| Manual | `Manual` | 8.33 ns | 96 B |
| EasyMapper | `EasyMapper_CachedPlan` | 32.67 ns | 96 B |
| AutoMapper | `AutoMapper_CachedPlan` | 58.60 ns | 96 B |

## Inline configured mapping

| Implementation | Method | Mean | Allocated |
|---|---|---:|---:|
| Manual | `Manual` | 11.75 ns | 80 B |
| EasyMapper | `EasyMapper_InlineLambda` | 842.81 us | 13.22 KB |
| AutoMapper | `AutoMapper_Preconfigured` | 83.51 ns | 112 B |

## 1,000 element maps

| Implementation | Method | Mean | Allocated |
|---|---|---:|---:|
| Manual | `Manual` | 9.46 us | 93.75 KB |
| EasyMapper | `EasyMapper_CachedPlan` | 32.56 us | 156.25 KB |
| AutoMapper | `AutoMapper_CachedPlan` | 60.66 us | 93.75 KB |

## First usable map

| Implementation | Method | Mean | Allocated |
|---|---|---:|---:|
| Manual | `Manual` | 860.00 ns | 96 B |
| EasyMapper | `EasyMapper_FirstMap` | 556.87 us | 17.11 KB |
| AutoMapper | `AutoMapper_FirstMap` | 2.06 ms | 234.82 KB |

## Environment

- Source commit: `ecc28d1`
- BenchmarkDotNet: 0.15.8
- Runtime: .NET 10.0.9 (10.0.9, 10.0.926.27113)
- SDK: 10.0.301
- OS: Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
- Processor: AMD Ryzen 7 8845HS w/ Radeon 780M Graphics
- Throughput job: ShortRun (three warmup and three measurement iterations)
- Cold-start job: one invocation per iteration, five warmups and fifteen measurements

These measurements are a transparent baseline, not a cross-machine performance guarantee. The inline EasyMapper lambda result includes isolated plan construction on every call and identifies a current optimization target.

