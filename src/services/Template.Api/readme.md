# Template.Api

Simple template for a REST api

## Run API only

Use API solution:

`src/services/Template.Api/Template.Api.sln`

## Run with Aspire (API + PostgreSQL)

Use AppHost solution:

`../Template.AppHost/Template.AppHost.sln`

Run AppHost with launch profile (required for dashboard endpoints):

`dotnet run --project ../Template.AppHost/Template.AppHost.csproj --launch-profile https`

Visual Studio:

Open `Template.AppHost.sln` when running Aspire orchestration.

# Help

## DB migration

Run script from the folder were .sln file is placed.


`dotnet ef migrations add test --output-dir Persistence/Migrations`
