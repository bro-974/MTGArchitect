using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MTGArchitect.Ingestor.Services;
using MTGArchitect.RAG.Data.Data;
using MTGArchitect.RAG.Data.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var ragConnectionString = context.Configuration.GetConnectionString("ragdb")
            ?? throw new InvalidOperationException("Connection string 'ragdb' is missing.");

        var lmStudioUri = context.Configuration["LmStudioUri"]
            ?? throw new InvalidOperationException("LmStudioUri configuration is missing.");

        services.AddRagData(ragConnectionString);
        services.AddHttpClient();
        services.AddSingleton(sp =>
            new EmbeddingService(sp.GetRequiredService<IHttpClientFactory>().CreateClient(), lmStudioUri));
        services.AddScoped<CardIngestionService>();
    })
    .Build();

var mtgJsonPath = args.FirstOrDefault()
    ?? @"C:\temp\AllPrintings.json";

using (var scope = host.Services.CreateScope())
{
    var ragDb = scope.ServiceProvider.GetRequiredService<RagDbContext>();
    await ragDb.Database.EnsureCreatedAsync();
}

using (var scope = host.Services.CreateScope())
{
    var ingestor = scope.ServiceProvider.GetRequiredService<CardIngestionService>();
    await ingestor.IngestAsync(mtgJsonPath);
}
