# Agent operating contract

## Project at a glance

This repository is a task-management backend template: a .NET 10 ASP.NET Core Minimal API, PostgreSQL persistence through EF Core 10 and Npgsql 10, Kafka publishing through a transactional outbox, shared service defaults, and optional .NET Aspire 13.4.6 orchestration. Planned capabilities are not implemented unless source and [`docs/roadmap.md`](docs/roadmap.md) show otherwise.

Before changing anything, read [`docs/architecture.md`](docs/architecture.md) and [`docs/roadmap.md`](docs/roadmap.md), then inspect the relevant source and configuration.

## Repository map and boundaries

- `src/services/Template.Api` — independently runnable API.
- `src/services/Template.AppHost` — optional Aspire orchestration.
- `src/common/Template.ServiceDefaults` — reusable hosting and messaging defaults only.
- `docs` — architecture, roadmap, and supporting documentation.

Put HTTP contracts and route mappings in `Endpoints`; use cases in feature folders under `Features`; EF Core persistence in `Persistence`; and Kafka/outbox infrastructure in `Messaging`. Keep `Template.Api` runnable without `Template.AppHost`.

Task mutation and its outbox record must use the same EF Core transaction. Mutation handlers must not publish directly to Kafka; the outbox processor publishes pending records.

Treat HTTP routes and DTOs, Kafka topics and payloads, EF migrations, configuration keys, and DI registrations as compatibility-sensitive.

## Build, run, and verification

Run from the repository root:

```powershell
dotnet restore src/services/Template.Api/Template.Api.sln
dotnet build src/services/Template.Api/Template.Api.sln --no-restore
dotnet restore src/services/Template.AppHost/Template.AppHost.sln
dotnet build src/services/Template.AppHost/Template.AppHost.sln --no-restore
dotnet run --project src/services/Template.AppHost/Template.AppHost.csproj --launch-profile https
```

No automated test project exists. Build success is compilation verification, not behavioral verification.

## Change workflow

Inspect first, make the smallest coherent change, add or update tests when a suitable project exists, and run proportionate checks. Report any checks you could not run and why. Review compatibility and migrations, and update affected documentation in the same change. Use normative words such as “must” only for requirements. Do not mark roadmap work complete until implementation and verification exist.

## Documentation maintenance

- Architecture, boundaries, runtime, configuration, or compatibility: review `AGENTS.md` and `docs/architecture.md`.
- Build, run, or verification commands: review `AGENTS.md` and `README.md`.
- Implemented roadmap work: update `README.md`, `docs/architecture.md`, and `docs/roadmap.md` status.
- Roadmap priority or status: update `docs/roadmap.md`.
- Agent workflow, prompting conventions, or documentation responsibilities: review `AGENTS.md` and `docs/ai-assisted-development.md`.
