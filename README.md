# Task Management Backend Template

A .NET task-management backend template built around a Minimal API, PostgreSQL persistence, and reliable Kafka event publishing through a transactional outbox.

## Current capabilities

- Versioned task CRUD Minimal API.
- PostgreSQL persistence through EF Core.
- Kafka publishing through a transactional outbox.
- Health endpoints at `/health` and `/alive`.
- Optional .NET Aspire orchestration for local API, PostgreSQL, and Kafka resources.

## Repository structure

| Path | Purpose |
| --- | --- |
| `src/services/Template.Api` | Independently runnable task API. |
| `src/services/Template.AppHost` | Optional Aspire orchestration for local development. |
| `src/common/Template.ServiceDefaults` | Shared health-endpoint and Kafka-publishing defaults. |
| `docs` | Architecture reference, roadmap, and supporting documentation. |

## Run the API

The API can run independently of Aspire. This path requires a reachable PostgreSQL instance and `ConnectionStrings:ApiDatabase`; Kafka configuration is required for publishing. See the [configuration inventory](docs/architecture.md#configuration-inventory). From the repository root:

```powershell
dotnet run --project src/services/Template.Api/Template.Api.csproj
```

The API solution is `src/services/Template.Api/Template.Api.sln`.

## Run with Aspire (optional)

For local orchestration of the API, PostgreSQL, and Kafka, run:

```powershell
dotnet run --project src/services/Template.AppHost/Template.AppHost.csproj --launch-profile https
```

The AppHost has its own solution at `src/services/Template.AppHost/Template.AppHost.sln`; it is optional and does not replace the API-only run path.

## Verify compilation

These commands are a convenience mirror of `AGENTS.md`, which is authoritative.

From the repository root:

```powershell
dotnet restore src/services/Template.Api/Template.Api.sln
dotnet build src/services/Template.Api/Template.Api.sln --no-restore
dotnet restore src/services/Template.AppHost/Template.AppHost.sln
dotnet build src/services/Template.AppHost/Template.AppHost.sln --no-restore
```

No automated tests currently exist. Successful compilation verifies that the projects build; it is not behavioral verification.

## Documentation

- [AGENTS.md](AGENTS.md) — operating contract and same-change documentation-maintenance expectations for contributors and autonomous agents.
- [Architecture reference](docs/architecture.md) — verified, implemented runtime architecture and compatibility-sensitive boundaries.
- [Roadmap](docs/roadmap.md) — planned work and technical-debt direction; it is separate from current capabilities.

## Using AI coding agents

Use the [AI-assisted development guide](docs/ai-assisted-development.md) for tool-neutral prompt templates covering architecture questions, planning, implementation, debugging, review, and documentation updates. It explains how to tell an agent which repository documents to read and how to make scope, constraints, verification, and deliverables explicit.

## Roadmap

See the [roadmap](docs/roadmap.md) for planned work.
