namespace MCP_Blueprint.Models;

public class TemplateFile
{
    /// <summary>Filename of the .scriban template, relative to the blueprint folder.</summary>
    public string Template { get; set; } = "";

    /// <summary>Output path relative to outputPath; may contain Scriban expressions.</summary>
    public string Output { get; set; } = "";

    public bool SkipIfExists { get; set; } = false;
}
