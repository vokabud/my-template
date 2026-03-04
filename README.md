# my-template

# Project structure

.github - workflows for git actions
help - helm charts
src - source code for services

# TODO

1. Create simple REST api service with db integration, kafka (and outbox)
2. [x] Add Aspire

# Aspire

Run the distributed app (API + PostgreSQL + pgAdmin) from AppHost:

`dotnet run --project src/services/Template.AppHost/Template.AppHost.csproj`

