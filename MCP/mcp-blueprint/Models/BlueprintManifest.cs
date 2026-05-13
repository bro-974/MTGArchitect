using System.Text.Json.Serialization;

namespace MCP_Blueprint.Models;

public class BlueprintManifest
{
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public List<TemplateParameter> Parameters { get; set; } = [];
    public List<TemplateFile> Files { get; set; } = [];
    public List<PostAction> PostActions { get; set; } = [];

    /// <summary>Absolute path to the blueprint folder. Set at load time, not from JSON.</summary>
    [JsonIgnore]
    public string BlueprintPath { get; set; } = "";
}
