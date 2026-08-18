var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var database = postgres.AddDatabase("ApiDatabase");
var backgroundJobsDatabase = postgres.AddDatabase("BackgroundJobsDatabase");
var kafka = builder
    .AddKafka("kafka")
    .WithKafkaUI(kafkaUi => kafkaUi.WithHostPort(8080));

builder
    .AddProject<Projects.Template_Api>("template-api")
    .WithReference(database)
    .WithReference(kafka)
    .WaitFor(kafka)
    .WaitFor(database);

builder
    .AddProject<Projects.Template_BackgroundJobs>("template-background-jobs")
    .WithReference(backgroundJobsDatabase)
    .WaitFor(backgroundJobsDatabase);

builder.Build().Run();
