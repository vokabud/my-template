var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var database = postgres.AddDatabase("DefaultConnection");

builder.AddProject<Projects.Template_Api>("template-api")
    .WithReference(database);

builder.Build().Run();
