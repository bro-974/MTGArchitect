namespace MCP_Blueprint.Models;

/// <summary>
/// Supported types: addToSolution, addProjectReference, runCommand
/// </summary>
public class PostAction
{
    public string Type { get; set; } = "";
    public Dictionary<string, string> Parameters { get; set; } = [];
}
