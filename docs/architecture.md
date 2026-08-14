# Implemented Architecture Reference

> This document describes **implemented current state only**. It is the architecture reference for autonomous agents and maintainers; planned work belongs in the [roadmap](roadmap.md), not here.

The operating rules for working in this repository are in [AGENTS.md](../AGENTS.md). In particular, `Template.Api` remains independently runnable: `Template.AppHost` is optional local orchestration and is not a required production host.

## Components and responsibilities

| Component | Implemented responsibility | Boundary |
| --- | --- | --- |
| `Template.Api` | .NET 10 ASP.NET Core Minimal API that maps task HTTP endpoints, runs EF Core 10 migrations through the Npgsql 10 provider at startup, performs task use cases, and hosts the outbox processor. | `src/services/Template.Api` |
| PostgreSQL | Stores the `Tasks` and `OutboxMessages` tables through EF Core/Npgsql migrations. | Connection string `ApiDatabase`; mappings and migrations under `src/services/Template.Api/Persistence` |
| Outbox processor | Hosted polling service that creates a scope and delegates one eligible batch to a scoped processor, which publishes rows and records processing or retry state. | `src/services/Template.Api/Messaging/Outbox/OutboxMessageProcessor.cs` and `OutboxBatchProcessor.cs` |
| Kafka | Receives task records on the configured topic from the Confluent producer. | Topic and payload contract under `src/services/Template.Api/Messaging/Kafka`; publisher under `src/common/Template.ServiceDefaults/Messaging/Kafka` |
| `Template.ServiceDefaults` | Reusable health-endpoint and Kafka-publishing defaults. It registers health checks and maps `/health` and `/alive`; it also supplies `KafkaOptions`, `IMessagePublisher`, and `KafkaMessagePublisher`. | `src/common/Template.ServiceDefaults` |
| `Template.AppHost` | Optional Aspire 13.4.6 AppHost which provisions PostgreSQL, its `ApiDatabase` database, Kafka, and Kafka UI; it references those resources from `Template.Api` and waits for the database and Kafka. | `src/services/Template.AppHost` |

## Repository and project boundaries

- `src/services/Template.Api` is the independently runnable application. HTTP contracts and mappings live in `Endpoints`; use cases live under `Features`; EF Core context, configurations, and migrations live in `Persistence`; Kafka-task and transactional-outbox infrastructure live in `Messaging`.
- `src/common/Template.ServiceDefaults` contains reusable hosting and messaging defaults only. It does not own task HTTP contracts, feature handlers, or persistence models.
- `src/services/Template.AppHost` is the optional Aspire composition project. It owns resource orchestration, not API behavior.
- `docs` contains the architecture reference, roadmap, and related documentation.

These boundaries match [AGENTS.md](../AGENTS.md). HTTP routes and DTOs, Kafka topics and payloads, EF migrations and schema, configuration keys, and DI registrations are compatibility-sensitive surfaces.

## HTTP request path

The Tasks endpoint group in `src/services/Template.Api/Endpoints/Tasks/TasksEndpoints.cs` maps the versioned prefix `/api/v1/tasks`:

| Method and route | Handler | HTTP result shape |
| --- | --- | --- |
| `GET /api/v1/tasks` | `GetTasksHandler` | `200 OK` with task responses |
| `GET /api/v1/tasks/{id:guid}` | `GetTaskByIdHandler` | `200 OK` or `404 Not Found` |
| `POST /api/v1/tasks` | `CreateTaskHandler` | `201 Created` or validation problem |
| `PUT /api/v1/tasks/{id:guid}` | `UpdateTaskHandler` | `200 OK`, `404 Not Found`, or validation problem |
| `DELETE /api/v1/tasks/{id:guid}` | `DeleteTaskHandler` | `204 No Content` or `404 Not Found` |

For each request, the mapped endpoint invokes its feature handler. The handler receives the scoped `IApplicationDbContext`, uses EF Core to query or change `Tasks`, and returns a typed HTTP result. `IApplicationDbContext` is backed by `ApplicationDbContext`, which uses Npgsql with PostgreSQL (`src/services/Template.Api/Configuration/Configure.Persistence.cs`). Task DTOs are `CreateTaskRequest`, `UpdateTaskRequest`, and `TaskResponse` in `src/services/Template.Api/Endpoints/Tasks`.

At startup, `Template.Api` applies EF Core migrations before mapping endpoints (`Configure.App.cs` and `Configure.Persistence.cs`). Service defaults map `/health` and `/alive` (`src/common/Template.ServiceDefaults/Extensions.cs`).

## Task mutation and transactional outbox path

Create and update build a `TaskSnapshot`; delete builds a tombstone. The handlers do not publish to Kafka directly.

```text
task mutation handler
  -> changes TaskEntity
  -> IOutboxMessageWriter adds TaskSnapshot or tombstone to OutboxMessages
  -> one IApplicationDbContext.SaveChangesAsync persists both changes
  -> HTTP response

hosted OutboxMessageProcessor
  -> polls eligible pending OutboxMessages
  -> IMessagePublisher publishes record or tombstone to Kafka
  -> records ProcessedAt on success
     or Attempts, LastError, and NextAttemptAt on failure
```

`OutboxMessageWriter` adds its row through the same scoped `IApplicationDbContext` used by the handler. The mutation handlers then call one `SaveChangesAsync`, so the task mutation and its outbox row are persisted together by that EF Core save operation. The processor runs independently of the request and polls pending rows; therefore Kafka publication is asynchronous relative to the HTTP response.

When publishing fails, the processor increments attempts, records a truncated error, and schedules the next attempt using exponential backoff capped by its implementation. A successful publish marks the row processed. This is a transactional-outbox implementation with retry, not a claim of exactly-once Kafka delivery.

## Kafka contract

`TaskKafkaTopics.Tasks` defines the topic as `tasks.data` in `src/services/Template.Api/Messaging/Kafka/TaskKafkaTopics.cs`. Snapshot messages use `TaskSnapshot` (`Id`, `Name`, `Description`) from `TaskSnapshot.cs`; delete messages are Kafka tombstones (the same task identifier key with a null value). `OutboxMessageWriter` serializes snapshots using web-default JSON and stores the payload in `OutboxMessages`; `KafkaMessagePublisher` publishes string keys and JSON values.

The outbox processor is registered as a hosted service, the writer as scoped, and the publisher as singleton in `src/services/Template.Api/Configuration/Configure.Messaging.cs`. These topic, payload, and DI details are compatibility-sensitive.

## Optional Aspire AppHost flow

`src/services/Template.AppHost/Program.cs` creates a PostgreSQL resource, creates the `ApiDatabase` database, creates Kafka, and adds Kafka UI. It adds `Template.Api` as a project, passes database and Kafka references to it, and waits for both resources. This flow is an AppHost composition path; it does not replace the API's own PostgreSQL and Kafka configuration requirements when the API runs independently.

## Configuration inventory

The verified application settings files are `src/services/Template.Api/appsettings.json` and `appsettings.Development.json`. `Configure.Persistence.cs`, `Configure.Messaging.cs`, and `Configure.Cors.cs` consume these configuration keys:

| Key or section | Consumer | Current behavior |
| --- | --- | --- |
| `ConnectionStrings:ApiDatabase` | `ConfigurePersistence` | Supplies the PostgreSQL connection to `UseNpgsql`. AppHost names its database resource `ApiDatabase`. |
| `ConnectionStrings:kafka` | `ConfigureMessaging` | When nonblank, overrides `Kafka:BootstrapServers`. AppHost names its Kafka resource `kafka`. |
| `Kafka:BootstrapServers` | `KafkaOptions` | Default bootstrap-server setting bound to `KafkaOptions`. |
| `Kafka:ClientId` | `KafkaOptions` | Client identifier bound to `KafkaOptions`. |
| `Cors:AllowedOrigins` | `ConfigureCors` | Defines the origins supplied to the `ClientOrigins` policy; the development file adds additional origins. |

`Template.Api.csproj` and `Template.AppHost.csproj` also declare user-secrets identifiers; their secret values are not repository configuration. No other configuration keys are asserted here.

## Compatibility and change review

Before changing these surfaces, review both call sites and the listed source of truth:

| Surface | Source of truth |
| --- | --- |
| Versioned task routes and DTOs | `src/services/Template.Api/Endpoints/Tasks/TasksEndpoints.cs` and DTO files in the same folder |
| Task event topic and payload | `src/services/Template.Api/Messaging/Kafka/TaskKafkaTopics.cs`, `TaskSnapshot.cs`, and `src/common/Template.ServiceDefaults/Messaging/Kafka/KafkaTaskEventPublisher.cs` |
| Database schema and migration history | `src/services/Template.Api/Persistence/Configurations` and `Persistence/Migrations` |
| Configuration keys | `src/services/Template.Api/appsettings*.json` and the `Configure.*.cs` consumers |
| DI registrations and hosted processing | `src/services/Template.Api/Configuration/Configure.Persistence.cs` and `Configure.Messaging.cs` |

## Known current-state gaps

- CI behavior is undocumented.
- No authentication is implemented.
- No Kafka consumer, user interface, dedicated background-job service, or AI-agent example is implemented.

## Automated testing

The API solution contains three .NET 10 xUnit v3 test projects under `src/services/Template.Api/tests`:

- `Template.Api.UnitTests` covers task handlers, outbox serialization, retry policy, and messaging DI registrations without Docker.
- `Template.Api.IntegrationTests` hosts the real API with `WebApplicationFactory`, runs real migrations and Npgsql behavior against a disposable Testcontainers PostgreSQL database, and replaces `IMessagePublisher` with a recording test double. It verifies HTTP CRUD, persisted outbox contracts, and deterministic single-batch outbox processing without starting Kafka.
- `Template.ArchitectureTests` enforces namespace, dependency, and project-reference boundaries without Docker.

The integration suite requires a running Docker-compatible engine. `IOutboxBatchProcessor` is the deterministic scoped seam used by both the hosted polling service and integration tests; tests do not wait for polling intervals.

For future intent and prioritization, see [docs/roadmap.md](roadmap.md). For operating rules and the required transactional-outbox boundary, see [AGENTS.md](../AGENTS.md).
