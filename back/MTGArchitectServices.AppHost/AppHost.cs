var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var scryfallService = builder.AddProject<Projects.MTGArchitect_Scryfall_Service>("scryfallservice")
    .WithHttpHealthCheck("/health");

// Front Angular
builder.AddExecutable("frontend",
    command: "npm",
    workingDirectory:"../../front",
    args: ["start"])
       .WithHttpEndpoint(port: 4201);

//backend
var apiService = builder.AddProject<Projects.MTGArchitectServices_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(scryfallService)
    .WaitFor(scryfallService);

builder.Build().Run();
