var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("webhooks");

builder.AddProject<Projects.WebHooks_Api>("webhooks-api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
