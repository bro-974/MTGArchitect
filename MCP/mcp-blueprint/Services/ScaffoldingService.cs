using System.Diagnostics;
using MCP_Blueprint.Models;
using Microsoft.Extensions.Logging;

namespace MCP_Blueprint.Services;

public interface IScaffoldingService
{
    Task<ScaffoldResult> ScaffoldAsync(
        BlueprintManifest manifest,
        string outputPath,
        Dictionary<string, string> parameters);
}

public class ScaffoldingService(ITemplateRenderService renderer, ILogger<ScaffoldingService> logger)
    : IScaffoldingService
{
    public async Task<ScaffoldResult> ScaffoldAsync(
        BlueprintManifest manifest,
        string outputPath,
        Dictionary<string, string> parameters)
    {
        var result = new ScaffoldResult();

        // Apply parameter defaults for missing optional params
        var vars = BuildVariables(manifest, parameters);

        // Generate files
        foreach (var templateFile in manifest.Files)
        {
            try
            {
                var renderedOutputPath = await renderer.RenderAsync(templateFile.Output, vars);
                var absoluteOutputPath = Path.GetFullPath(Path.Combine(outputPath, renderedOutputPath));

                if (templateFile.SkipIfExists && File.Exists(absoluteOutputPath))
                {
                    logger.LogInformation("Skipping existing file: {Path}", absoluteOutputPath);
                    continue;
                }

                var templateSourcePath = Path.Combine(manifest.BlueprintPath, templateFile.Template);
                if (!File.Exists(templateSourcePath))
                {
                    result.Errors.Add($"Template source not found: {templateSourcePath}");
                    continue;
                }

                var templateContent = await File.ReadAllTextAsync(templateSourcePath);
                var renderedContent = await renderer.RenderAsync(templateContent, vars);

                var dir = Path.GetDirectoryName(absoluteOutputPath)!;
                Directory.CreateDirectory(dir);
                await File.WriteAllTextAsync(absoluteOutputPath, renderedContent);

                result.CreatedFiles.Add(absoluteOutputPath);
                logger.LogInformation("Created: {Path}", absoluteOutputPath);
            }
            catch (Exception ex)
            {
                var msg = $"Error generating '{templateFile.Output}': {ex.Message}";
                result.Errors.Add(msg);
                logger.LogError(ex, "{Message}", msg);
            }
        }

        // Run post-actions
        foreach (var action in manifest.PostActions)
        {
            var actionResult = await ExecutePostActionAsync(action, vars, outputPath);
            result.PostActionResults.Add(actionResult);
            if (!actionResult.Success)
                result.Errors.Add($"PostAction '{action.Type}' failed: {actionResult.Error}");
        }

        result.Success = result.Errors.Count == 0;
        return result;
    }

    private async Task<PostActionResult> ExecutePostActionAsync(
        PostAction action,
        Dictionary<string, object> vars,
        string outputPath)
    {
        try
        {
            // Render all parameter values through Scriban
            var renderedParams = new Dictionary<string, string>();
            foreach (var (k, v) in action.Parameters)
                renderedParams[k] = await renderer.RenderAsync(v, vars);

            return action.Type switch
            {
                "addToSolution" => await RunDotnetAsync(
                    $"sln \"{ResolveAbsolute(renderedParams["solutionPath"], outputPath)}\" add \"{ResolveAbsolute(renderedParams["projectPath"], outputPath)}\"",
                    outputPath, action.Type),

                "addProjectReference" => await RunDotnetAsync(
                    $"add \"{ResolveAbsolute(renderedParams["projectPath"], outputPath)}\" reference \"{ResolveAbsolute(renderedParams["referencePath"], outputPath)}\"",
                    outputPath, action.Type),

                "runCommand" => await RunShellAsync(
                    renderedParams.GetValueOrDefault("command", ""),
                    renderedParams.GetValueOrDefault("args", ""),
                    renderedParams.TryGetValue("workingDirectory", out var wd)
                        ? ResolveAbsolute(wd, outputPath)
                        : outputPath,
                    action.Type),

                _ => new PostActionResult { Type = action.Type, Success = false, Error = $"Unknown post-action type: {action.Type}" }
            };
        }
        catch (Exception ex)
        {
            return new PostActionResult { Type = action.Type, Success = false, Error = ex.Message };
        }
    }

    private static string ResolveAbsolute(string path, string basePath) =>
        Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(basePath, path));

    private static async Task<PostActionResult> RunDotnetAsync(string args, string workingDir, string actionType)
        => await RunShellAsync("dotnet", args, workingDir, actionType);

    private static async Task<PostActionResult> RunShellAsync(
        string command, string args, string workingDir, string actionType)
    {
        var psi = new ProcessStartInfo(command, args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start process: {command}");

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var succeeded = process.ExitCode == 0;
        return new PostActionResult
        {
            Type = actionType,
            Success = succeeded,
            Output = stdout.Trim(),
            Error = succeeded ? null : stderr.Trim()
        };
    }

    private static Dictionary<string, object> BuildVariables(
        BlueprintManifest manifest,
        Dictionary<string, string> parameters)
    {
        var vars = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in manifest.Parameters)
        {
            if (parameters.TryGetValue(param.Name, out var value))
                vars[param.Name] = value;
            else if (param.Default is not null)
                vars[param.Name] = param.Default;
            else if (param.Required)
                throw new ArgumentException($"Required parameter '{param.Name}' was not provided.");
        }

        // Also pass through any extra parameters the caller supplied
        foreach (var (k, v) in parameters)
            vars.TryAdd(k, v);

        return vars;
    }
}
