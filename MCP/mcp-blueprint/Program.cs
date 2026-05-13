using MCP_Blueprint.Services;
using MCP_Blueprint.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITemplateDiscoveryService, TemplateDiscoveryService>();
builder.Services.AddSingleton<ITemplateRenderService, TemplateRenderService>();
builder.Services.AddSingleton<IScaffoldingService, ScaffoldingService>();
builder.Services.AddSingleton<ITemplateManagementService, TemplateManagementService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<BlueprintTools>();

await builder.Build().RunAsync();
