using System.Text.Json.Serialization;
using MCP_Blueprint.Models;

namespace MCP_Blueprint.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true)]
[JsonSerializable(typeof(BlueprintManifest))]
[JsonSerializable(typeof(TemplateParameter))]
[JsonSerializable(typeof(TemplateFile))]
[JsonSerializable(typeof(PostAction))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(List<TemplateSummary>))]
[JsonSerializable(typeof(TemplateSummary))]
[JsonSerializable(typeof(ParameterSummary))]
[JsonSerializable(typeof(ApplyTemplateResponse))]
[JsonSerializable(typeof(PostActionResult))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(List<TemplateParameter>))]
[JsonSerializable(typeof(List<PostAction>))]
[JsonSerializable(typeof(UpsertResult))]
internal partial class AppJsonContext : JsonSerializerContext { }
