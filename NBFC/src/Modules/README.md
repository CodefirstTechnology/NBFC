# Modules

Bounded-context modules live here. Each module follows Clean Architecture:

```
Modules/{ModuleName}/
  {ModuleName}.Domain/
  {ModuleName}.Application/
  {ModuleName}.Infrastructure/
  {ModuleName}.Api/
```

## Build order

| Step | Module | Status |
|------|--------|--------|
| 1 | BuildingBlocks | Done |
| 2 | Identity | Done |
| 3 | Members (CQRS template) | Done |
| 4 | Shell + UI Kit + Auth | Done |
| 5 | mfe-member | Done |
| 6 | Deposits, Loans, Collections, Recovery, Accounting, Reporting + MFEs | Done |
| 6 | Admin (mfe-admin → Identity admin API) | Done |
| 7 | Caching, OpenTelemetry, Polly in BuildingBlocks | Done |
| 8 | YARP gateway, Hangfire jobs, module health checks | Done |

Cross-module communication is **only** via `Shared/Contracts` integration events processed through the transactional outbox.
