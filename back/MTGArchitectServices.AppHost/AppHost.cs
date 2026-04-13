var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

// Front Angular
builder.AddExecutable("frontend",
    command: "npm",
    workingDirectory:"../../front",
    args: ["start"])
       .WithHttpEndpoint(port: 4201);

//backend
var apiService = builder.AddProject<Projects.MTGArchitectServices_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
