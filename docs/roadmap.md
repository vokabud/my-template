# Roadmap and technical-debt register

This is the authoritative register of unimplemented direction. It records direction only; it does not describe implemented behavior, assign ownership, set delivery dates, or make commitments. For the implemented current state, see [architecture.md](architecture.md). Follow the operating rules in [AGENTS.md](../AGENTS.md) when changing either implementation or this register.

Document order is suggested sequencing, not a schedule. Foundations appear before higher-risk feature expansion so that implementation can be validated and maintained as it grows.

## Status definitions and maintenance

| Status | Definition |
| --- | --- |
| `Planned` | Direction that has not started implementation. |
| `In progress` | Implementation work has started but the observable acceptance outcome is not yet satisfied. |
| `Blocked` | Work cannot proceed because a concrete dependency or decision is unresolved. |
| `Complete` | Implementation and verification evidence exist, and current-state documentation has been updated. |

Every entry must retain a status, concise rationale, and observable acceptance outcome. Update those fields in the same change as relevant implementation or status changes. An entry may change to `Complete` only when its implementation and verification evidence exist and current-state documentation is updated.

## Technical debt and engineering maturity

| Direction | Status | Rationale | Observable acceptance outcome |
| --- | --- | --- | --- |
| Migrate to .NET 10 and align framework/package versions. | `Complete` | All projects now target .NET 10 with aligned ASP.NET Core, EF Core, Npgsql, OpenAPI, and Aspire package lines. | Both solutions restore and build with zero warnings and errors, the dependency audit reports no known vulnerabilities, EF Core reports no pending model changes, and current-state documentation reflects the verified versions. Docker image and Aspire startup checks were not run because the local Docker engine was unavailable. |
| Add unit, integration, and architecture tests. | `Complete` | Separate suites now verify task behavior, PostgreSQL-backed API and outbox workflows, and documented architecture boundaries. | The unit suite passes 19 tests, the PostgreSQL integration suite passes 7 tests using a Podman-hosted disposable database, the architecture suite passes 7 tests, and both solutions build with zero warnings and errors. |
| Add CI for restore, build, test, formatting/static analysis, and migration validation. | `Planned` | CI behavior is currently undocumented. | A version-controlled CI workflow runs restore, build, test, formatting or static analysis, and migration validation; its documented commands can be executed by contributors. |
| Establish formatting and analyzer rules. | `Planned` | Consistent automated code-quality checks are not yet defined. | Version-controlled formatting and analyzer configuration is enforced by documented local and CI checks. |
| Add repeatable local-infrastructure and integration-test guidance. | `Planned` | Local service setup and integration-test execution need repeatable guidance. | Documentation provides repeatable local-infrastructure and integration-test steps that a contributor can follow successfully. |
| Define compatibility/versioning policies for HTTP APIs, Kafka contracts, and database migrations. | `Planned` | These compatibility-sensitive surfaces require explicit evolution rules. | Documented policies cover HTTP APIs, Kafka contracts, and database migrations, with change-review expectations that are applied to related changes. |
| Improve observability, resilience, secrets handling, and production-readiness guidance. | `Planned` | Production operating expectations need clear, verifiable guidance. | Documented and verified guidance addresses observability, resilience, secrets handling, and production readiness for the implemented services. |
| Resolve documentation and naming inconsistencies when encountered. | `Planned` | Consistency across code and documentation supports reliable maintenance. | Identified inconsistencies are corrected in the same relevant change, with affected documentation and names kept consistent. |

## Product and platform direction

| Direction | Status | Rationale | Observable acceptance outcome |
| --- | --- | --- | --- |
| Background-job processing service. | `Planned` | Some work may need execution outside the request-serving API process. | A separately identified background-job processing service has implemented, verified job-processing behavior and current-state documentation. |
| Kafka consumption. | `Planned` | The current implementation publishes task records but does not consume Kafka records. | A verified consumer processes defined Kafka records with documented contract and failure behavior. |
| AI-agent example that decomposes plain text into tasks. | `Planned` | The template has no example of AI-assisted task decomposition. | A documented, runnable example converts plain-text input into observable task decomposition results and has verification evidence. |
| Task-management UI. | `Planned` | The current application exposes an API only. | A usable task-management interface performs documented task workflows against the API and has verification evidence. |
| Authentication and authorization. | `Planned` | No authentication or authorization is implemented. | Verified authentication and authorization enforce documented access rules for the applicable API operations. |
