namespace MCP_Blueprint.Models;

public class TemplateSummary
{
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ParameterSummary> Parameters { get; set; } = [];
}

public class ParameterSummary
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Default { get; set; }
    public bool Required { get; set; }
}

public class ApplyTemplateResponse
{
    public bool Success { get; set; }
    public List<string> CreatedFiles { get; set; } = [];
    public List<PostActionResult> PostActionResults { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Error { get; set; } = "";
}
