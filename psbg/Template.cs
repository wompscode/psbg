namespace psbg;

public class Template
{
    public string? UnparsedTemplate { get; set; }
    public string? TemplateName { get; set; }
    public Template(string template)
    {
        if (!File.Exists(template))
        {
            throw new FileNotFoundException("Template not found", template);
        }

        UnparsedTemplate = File.ReadAllText(template);
        TemplateName = new FileInfo(template).Name;
    }

    public Template()
    {
        UnparsedTemplate = "";
        TemplateName = "";
    }
}