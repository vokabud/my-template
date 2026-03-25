var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var database = postgres.AddDatabase("ApiDatabase");
var kafka = builder
    .AddKafka("kafka")
    .WithKafkaUI(kafkaUi => kafkaUi.WithHostPort(8080));

builder
    .AddProject<Projects.Template_Api>("template-api")
    .WithReference(database)
    .WithReference(kafka)
    .WaitFor(kafka)
    .WaitFor(database);

builder.Build().Run();
