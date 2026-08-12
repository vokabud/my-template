# my-template

# Project structure

.github - workflows for git actions
help - helm charts
src - source code for services

# TODO

1. [x] Add transactional outbox for Kafka publishing
2. Add service for background job processing
3. Add kafka consumption
4. Add ai agent example (task decomposition from a plain text)
5. Add base ui for task managemetn
6. Add auth

# Aspire

Aspire orchestration is isolated from the API solution.

AppHost solution:

`src/services/Template.AppHost/Template.AppHost.sln`

Run distributed app (API + PostgreSQL + dashboard):

`dotnet run --project src/services/Template.AppHost/Template.AppHost.csproj --launch-profile https`

Visual Studio:

Open `Template.AppHost.sln` and run `Template.AppHost` as startup project.

# API only

API solution stays independent:

`src/services/Template.Api/Template.Api.sln`

