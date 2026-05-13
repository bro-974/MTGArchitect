using System.Text.Json;
using MCP_Blueprint.Models;
using MCP_Blueprint.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MCP_Blueprint.Services;

public interface ITemplateDiscoveryService
{
    Task<IReadOnlyList<BlueprintManifest>> GetAllBlueprintsAsync();
    Task<BlueprintManifest?> GetBlueprintAsync(string id);
}

public class TemplateDiscoveryService(IConfiguration config, ILogger<TemplateDiscoveryService> logger)
    : ITemplateDiscoveryService
{

    private string TemplatesRoot =>
        Path.GetFullPath(config["Mcp:TemplatesPath"] ?? "./Templates");

    public async Task<IReadOnlyList<BlueprintManifest>> GetAllBlueprintsAsync()
    {
        if (!Directory.Exists(TemplatesRoot))
        {
            logger.LogWarning("Templates folder not found: {Path}", TemplatesRoot);
            return [];
        }

        var manifests = new List<BlueprintManifest>();
        foreach (var dir in Directory.EnumerateDirectories(TemplatesRoot))
        {
            var manifest = await LoadManifestAsync(dir);
            if (manifest is not null)
                manifests.Add(manifest);
        }
        return manifests;
    }

    public async Task<BlueprintManifest?> GetBlueprintAsync(string id)
    {
        var dir = Path.Combine(TemplatesRoot, id);
        return Directory.Exists(dir) ? await LoadManifestAsync(dir) : null;
    }

    private async Task<BlueprintManifest?> LoadManifestAsync(string blueprintDir)
    {
        var manifestPath = Path.Combine(blueprintDir, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            logger.LogDebug("No manifest.json in {Dir}", blueprintDir);
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(manifestPath);
            var manifest = await JsonSerializer.DeserializeAsync(stream, AppJsonContext.Default.BlueprintManifest);
            if (manifest is null) return null;
            manifest.BlueprintPath = blueprintDir;
            return manifest;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load manifest from {Path}", manifestPath);
            return null;
        }
    }
}
