# my-template

# Project structure

.github - workflows for git actions
help - helm charts
src - source code for services

# TODO

1. Create simple REST api service with db integration, kafka (and outbox)
2. [x] Add Aspire

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

