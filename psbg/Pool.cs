namespace psbg;

public class Pool
{
    private static readonly List<Template> Templates = new();

    public static Template? GetTemplate(string templateName)
    {
        return Templates.Find(template => template.TemplateName == templateName);
    }

    public static bool HasTemplate(string templateName)
    {
        return Templates.Any(template => template.TemplateName == templateName);
    }
    public static void AddTemplate(Template template)
    {
        if (Templates.Find(_ => _.TemplateName == template.TemplateName) == null)
        {
            Templates.Add(template);
        }
    }
}