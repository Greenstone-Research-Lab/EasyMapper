# Contributing to EasyMapper

Thank you for helping improve EasyMapper. The repository uses a protected promotion flow so that
the public package remains usable while new behavior is tested progressively.

## Branch flow

```text
contributor fork or feature/*
             |
             v
        development
             |
             v
          staging  -- beta NuGet packages
             |
             v
           master  -- stable source of record
```

Open all contributor and feature pull requests against `development`. Pull requests targeting
`staging` are accepted only from `development`; pull requests targeting `master` are accepted only
from `staging`. The promotion workflow rejects every other path.

Do not place unrelated changes in the same pull request. Keep commits reviewable and explain public
API or behavioral decisions in the pull-request description.

## Required quality checks

Run the complete local pipeline before opening a pull request:

```powershell
./eng/qa.ps1
```

The pipeline requires a warning-free Release build, formatting and analyzer compliance, at least
99% line/branch/method coverage, smoke verification, NuGet consumer verification, and the sustained
load test.

Public members require professional XML documentation. New API behavior requires focused tests and
README or sample updates when users need guidance.

## Releases

Every push promoted into `staging` runs the complete QA pipeline and produces a beta package. The
Beta publishing uses NuGet Trusted Publishing instead of a long-lived API key. The GitHub `staging`
environment must define the public NuGet profile name as the `NUGET_USER` variable. The corresponding
nuget.org trusted publishing policy must target repository owner `Greenstone-Research-Lab`, repository
`EasyMapper`, workflow `publish-beta.yml`, and environment `staging`.
Stable releases are promoted from `staging` to `master` after beta validation; stable NuGet
publication will be attached to a version tag in a dedicated release workflow.
