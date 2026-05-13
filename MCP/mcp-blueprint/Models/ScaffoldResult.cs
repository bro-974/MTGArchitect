namespace MCP_Blueprint.Models;

public class ScaffoldResult
{
    public bool Success { get; set; }
    public List<string> CreatedFiles { get; set; } = [];
    public List<PostActionResult> PostActionResults { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}

public class PostActionResult
{
    public string Type { get; set; } = "";
    public bool Success { get; set; }
    public string Output { get; set; } = "";
    public string? Error { get; set; }
}
