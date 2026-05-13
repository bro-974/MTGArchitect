namespace MCP_Blueprint.Models;

public class TemplateParameter
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public string Description { get; set; } = "";
    public string? Default { get; set; }
    public bool Required { get; set; }
}
