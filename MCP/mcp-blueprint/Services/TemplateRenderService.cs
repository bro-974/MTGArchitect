using Scriban;
using Scriban.Runtime;

namespace MCP_Blueprint.Services;

public interface ITemplateRenderService
{
    Task<string> RenderAsync(string templateContent, Dictionary<string, object> variables);
}

public class TemplateRenderService : ITemplateRenderService
{
    public async Task<string> RenderAsync(string templateContent, Dictionary<string, object> variables)
    {
        var template = Template.Parse(templateContent);
        if (template.HasErrors)
            throw new InvalidOperationException(
                $"Template parse errors: {string.Join("; ", template.Messages)}");

        var scriptObj = new ScriptObject();
        foreach (var (key, value) in variables)
            scriptObj.Add(key, value);

        var context = new TemplateContext { StrictVariables = false };
        context.PushGlobal(scriptObj);

        return await template.RenderAsync(context);
    }
}
