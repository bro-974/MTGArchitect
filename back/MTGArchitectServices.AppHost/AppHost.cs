var builder = DistributedApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Configuration value 'Jwt:Key' is missing.");

const string jwtIssuer = "MTGArchitect.Auth";
const string jwtAudience = "MTGArchitect";

builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithDataVolume()
    .WithPgAdmin();

var authDb = postgres.AddDatabase("authdb");
var ragDb = postgres.AddDatabase("ragdb");

var lmStudioUri = builder.AddParameter("LmStudioUri", "http://localhost:1234/v1");

var aiService = builder.AddProject<Projects.MTGArchitect_AI_Service>("aiservice")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("LmStudioUri", lmStudioUri)
    .WithReference(ragDb)
    .WaitFor(ragDb);

var chatMessageService = builder.AddProject<Projects.MTGArchitect_ChatMessage_Service>("chatmessageservice")
    .WithHttpHealthCheck("/health")
    .WithReference(authDb)
    .WaitFor(authDb);

var scryfallService = builder.AddProject<Projects.MTGArchitect_Scryfall_Service>("scryfallservice")
    .WithHttpHealthCheck("/health");

var authApiService = builder.AddProject<Projects.MTGArchitect_Auth_Api>("authapiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(authDb)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WaitFor(authDb);

builder.AddExecutable("frontend",
    command: "npm",
    workingDirectory: "../../front",
    args: ["start"])
       .WithHttpEndpoint(port: 4201);

var apiService = builder.AddProject<Projects.MTGArchitect_Api>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(scryfallService)
    .WithReference(authDb)
    .WithReference(aiService)
    .WithReference(authApiService)
    .WithReference(chatMessageService)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WaitFor(scryfallService)
    .WaitFor(authApiService)
    .WaitFor(authDb)
    .WaitFor(aiService)
    .WaitFor(chatMessageService)
    .WithHttpEndpoint(port: 4300, name: "api-http");

var ingestor = builder.AddProject<Projects.MTGArchitect_Ingestor>("ingestor")
    .WithReference(ragDb)
    .WithEnvironment("LmStudioUri", lmStudioUri)
    .WithExplicitStart();

builder.Build().Run();
