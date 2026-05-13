namespace MCP_Blueprint.Models;

public class UpsertResult
{
    public bool Success { get; set; }
    public bool TemplateCreated { get; set; }
    public bool FileAdded { get; set; }
    public string ManifestPath { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string? Error { get; set; }
}
