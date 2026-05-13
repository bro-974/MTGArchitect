using System.Text.Json;
using MCP_Blueprint.Models;
using MCP_Blueprint.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MCP_Blueprint.Services;

public interface ITemplateManagementService
{
    Task<UpsertResult> UpsertTemplateFileAsync(
        string templateId,
        string templateFileName,
        string fileContent,
        string outputPath,
        string? description,
        List<TemplateParameter>? parameters,
        List<PostAction>? postActions,
        bool skipIfExists);
}

public class TemplateManagementService(IConfiguration config, ILogger<TemplateManagementService> logger)
    : ITemplateManagementService
{
    private string TemplatesRoot =>
        Path.GetFullPath(config["Mcp:TemplatesPath"] ?? "./Templates");

    public async Task<UpsertResult> UpsertTemplateFileAsync(
        string templateId,
        string templateFileName,
        string fileContent,
        string outputPath,
        string? description,
        List<TemplateParameter>? parameters,
        List<PostAction>? postActions,
        bool skipIfExists)
    {
        var blueprintDir = Path.Combine(TemplatesRoot, templateId);
        var manifestPath = Path.Combine(blueprintDir, "manifest.json");
        var templateFilePath = Path.Combine(blueprintDir, templateFileName);

        var templateCreated = !Directory.Exists(blueprintDir);
        var fileAdded = !File.Exists(templateFilePath);

        BlueprintManifest manifest;
        if (File.Exists(manifestPath))
        {
            try
            {
                await using var stream = File.OpenRead(manifestPath);
                manifest = await JsonSerializer.DeserializeAsync(stream, AppJsonContext.Default.BlueprintManifest)
                    ?? new BlueprintManifest { Id = templateId };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load existing manifest from {Path}", manifestPath);
                return new UpsertResult { Success = false, Error = $"Failed to read existing manifest: {ex.Message}" };
            }
        }
        else
        {
            manifest = new BlueprintManifest { Id = templateId };
        }

        if (description is not null)
            manifest.Description = description;

        if (parameters is not null)
            manifest.Parameters = parameters;

        if (postActions is not null)
            manifest.PostActions = postActions;

        var existingFile = manifest.Files.FirstOrDefault(f => f.Template == templateFileName);
        if (existingFile is not null)
        {
            existingFile.Output = outputPath;
            existingFile.SkipIfExists = skipIfExists;
        }
        else
        {
            manifest.Files.Add(new TemplateFile
            {
                Template = templateFileName,
                Output = outputPath,
                SkipIfExists = skipIfExists,
            });
        }

        try
        {
            Directory.CreateDirectory(blueprintDir);

            var json = JsonSerializer.Serialize(manifest, AppJsonContext.Default.BlueprintManifest);
            await File.WriteAllTextAsync(manifestPath, json);

            await File.WriteAllTextAsync(templateFilePath, fileContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write template files for {TemplateId}", templateId);
            return new UpsertResult { Success = false, Error = $"Failed to write files: {ex.Message}" };
        }

        return new UpsertResult
        {
            Success = true,
            TemplateCreated = templateCreated,
            FileAdded = fileAdded,
            ManifestPath = manifestPath,
            FilePath = templateFilePath,
        };
    }
}
