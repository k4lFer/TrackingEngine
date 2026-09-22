namespace App.Interfaces.Ports.Emails;

public interface ITemplateRenderer
{
    Task<string> RenderAsync<TModel>(string templateName, TModel model);
}
