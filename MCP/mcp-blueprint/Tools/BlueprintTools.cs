using System.ComponentModel;
using System.Text.Json;
using MCP_Blueprint.Models;
using MCP_Blueprint.Serialization;
using MCP_Blueprint.Services;
using ModelContextProtocol.Server;

namespace MCP_Blueprint.Tools;

[McpServerToolType]
public class BlueprintTools(ITemplateDiscoveryService discovery, IScaffoldingService scaffolding, ITemplateManagementService management)
{
    [McpServerTool(Name = "list_templates")]
    [Description("Lists all available blueprint templates with their IDs, descriptions, and expected parameters.")]
    public async Task<string> ListTemplates()
    {
        var blueprints = await discovery.GetAllBlueprintsAsync();

        var summary = blueprints.Select(b => new TemplateSummary
        {
            Id = b.Id,
            Description = b.Description,
            Parameters = b.Parameters.Select(p => new ParameterSummary
            {
                Name = p.Name,
                Type = p.Type,
                Description = p.Description,
                Default = p.Default,
                Required = p.Required,
            }).ToList(),
        }).ToList();

        return JsonSerializer.Serialize(summary, AppJsonContext.Default.ListTemplateSummary);
    }

    [McpServerTool(Name = "apply_template")]
    [Description(
        "Applies a blueprint template to generate project files on disk. " +
        "Returns a JSON report of created files and post-action results.")]
    public async Task<string> ApplyTemplate(
        [Description("ID of the blueprint to apply (from list_templates)")]
        string templateId,
        [Description("Absolute or relative path where the output files will be written")]
        string outputPath,
        [Description("Template parameters as a JSON object with string values (e.g. {\"project_name\": \"MyApi\"})")]
        string parameters)
    {
        var manifest = await discovery.GetBlueprintAsync(templateId);
        if (manifest is null)
            return JsonSerializer.Serialize(
                new ErrorResponse { Error = $"Template '{templateId}' not found." },
                AppJsonContext.Default.ErrorResponse);

        Dictionary<string, string> parsedParams;
        try
        {
            parsedParams = JsonSerializer.Deserialize(parameters, AppJsonContext.Default.DictionaryStringString)
                ?? [];
        }
        catch (JsonException ex)
        {
            return JsonSerializer.Serialize(
                new ErrorResponse { Error = $"Invalid parameters JSON: {ex.Message}" },
                AppJsonContext.Default.ErrorResponse);
        }

        var resolvedOutput = Path.GetFullPath(outputPath);

        ScaffoldResult result;
        try
        {
            result = await scaffolding.ScaffoldAsync(manifest, resolvedOutput, parsedParams);
        }
        catch (ArgumentException ex)
        {
            return JsonSerializer.Serialize(
                new ErrorResponse { Error = ex.Message },
                AppJsonContext.Default.ErrorResponse);
        }

        return JsonSerializer.Serialize(new ApplyTemplateResponse
        {
            Success = result.Success,
            CreatedFiles = result.CreatedFiles,
            PostActionResults = result.PostActionResults,
            Errors = result.Errors,
        }, AppJsonContext.Default.ApplyTemplateResponse);
    }

    [McpServerTool(Name = "upsert_template_file")]
    [Description(
        "Creates or updates a single .scriban file inside a blueprint template. " +
        "The server automatically creates the blueprint folder and manifest.json if they do not exist, " +
        "and keeps the manifest in sync on every call. " +
        "Returns a JSON report indicating whether the blueprint and/or file were newly created.")]
    public async Task<string> UpsertTemplateFile(
        [Description("ID of the blueprint (becomes the folder name under ./Templates)")]
        string templateId,
        [Description("File name of the template file, e.g. \"Program.cs.scriban\"")]
        string templateFileName,
        [Description("Raw Scriban template content to write into the file")]
        string fileContent,
        [Description("Scriban expression for the output path, e.g. \"{{ project_name }}/Program.cs\"")]
        string outputPath,
        [Description("Optional. Updates manifest Description if provided")]
        string? description = null,
        [Description("Optional. JSON array of TemplateParameter objects — replaces the manifest parameter list")]
        string? parameters = null,
        [Description("Optional. JSON array of PostAction objects — replaces the manifest post-action list")]
        string? postActions = null,
        [Description("Optional. When true, the scaffolding engine skips this file if it already exists on disk")]
        bool skipIfExists = false)
    {
        List<TemplateParameter>? parsedParameters = null;
        if (parameters is not null)
        {
            try
            {
                parsedParameters = JsonSerializer.Deserialize(parameters, AppJsonContext.Default.ListTemplateParameter);
            }
            catch (JsonException ex)
            {
                return JsonSerializer.Serialize(
                    new ErrorResponse { Error = $"Invalid parameters JSON: {ex.Message}" },
                    AppJsonContext.Default.ErrorResponse);
            }
        }

        List<PostAction>? parsedPostActions = null;
        if (postActions is not null)
        {
            try
            {
                parsedPostActions = JsonSerializer.Deserialize(postActions, AppJsonContext.Default.ListPostAction);
            }
            catch (JsonException ex)
            {
                return JsonSerializer.Serialize(
                    new ErrorResponse { Error = $"Invalid postActions JSON: {ex.Message}" },
                    AppJsonContext.Default.ErrorResponse);
            }
        }

        var result = await management.UpsertTemplateFileAsync(
            templateId, templateFileName, fileContent, outputPath,
            description, parsedParameters, parsedPostActions, skipIfExists);

        return JsonSerializer.Serialize(result, AppJsonContext.Default.UpsertResult);
    }
}
