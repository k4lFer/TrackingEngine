using App.Interfaces.Ports.Emails;

namespace App.Infrastructure.Adapters.Templates;

public class HtmlTemplateRenderer : ITemplateRenderer
{
    private readonly string _templatesPath;

    public HtmlTemplateRenderer()
    {
        _templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates");
    }

    public async Task<string> RenderAsync<TModel>(string templateName, TModel model)
    {
        var filePath = Path.Combine(_templatesPath, templateName);
        var content = await File.ReadAllTextAsync(filePath);

        if (model is null) return content;

        foreach (var prop in model.GetType().GetProperties())
        {
            var key = $"{{{{{prop.Name}}}}}";
            var value = prop.GetValue(model)?.ToString() ?? "";
            content = content.Replace(key, value);
        }

        return content;
    }
}
