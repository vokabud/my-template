# Template.Api

Simple template for a REST api

For local orchestration with PostgreSQL, run AppHost:

`dotnet run --project ../Template.AppHost/Template.AppHost.csproj`

# Help

## DB migration

Run script from the folder were .sln file is placed.


`dotnet ef migrations add test --output-dir Persistence/Migrations`
