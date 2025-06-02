using System.Text.RegularExpressions;
using static psbg.Structs;
namespace psbg;

public class Parser
{
    private static string Parse(string value, string baseTemplate, Post post)
    {
        value = value.Replace("{{", "").Replace("}}", "");
        if (value.Contains(":"))
        {
            string[] parts = value.Split(':');
            parts[0] = parts[0].Trim();
            
            switch (parts[0])
            {
                case "template":
                    parts[1] = parts[1].Trim();
                    string parts1 = Path.Join(Program.Config.TemplateDirectory, parts[1]);
                    if (File.Exists(parts1))
                    {
                        string name = new FileInfo(parts1).Name;
                        if (name == baseTemplate)
                        {
                            return string.Empty;
                        }
                        Template template = new Template();
                        if (!Pool.HasTemplate(name))
                        {
                            Template newTemplate = new(parts1);
                            Pool.AddTemplate(newTemplate);
                            template = newTemplate;
                        }
                        else
                        {
                            Template? _ = Pool.GetTemplate(name);
                            if (_ != null && _.UnparsedTemplate != null && _.TemplateName != null)
                            {
                                template = _;
                            }
                        }

                        string outputValue = "";
                        if (template.UnparsedTemplate != null)
                        {
                            outputValue = ParseTemplate(template, baseTemplate,  post);
                        }

                        return outputValue;
                    }
                    break;
            }
        }
        else
        {
            switch (value)
            {
                case "content":
                    return post.Content;
                case "title":
                    return post.Title;
                case "pageTitle":
                    return post.Title;
                case "author":
                    return post.Author;
                case "date":
                    return post.Date;
                case "summary":
                    return post.Summary;
                case "fileName":
                    return post.FileName;
                default:
                    return string.Empty;
            }
        }
        return string.Empty;
    } 
    private static string Parse(string value, string baseTemplate)
    {
        value = value.Replace("{{", "").Replace("}}", "");
        if (value.Contains(":"))
        {
            string[] parts = value.Split(':');
            parts[0] = parts[0].Trim();
            
            switch (parts[0])
            {
                case "template":
                    parts[1] = parts[1].Trim();
                    string parts1 = Path.Join(Program.Config.TemplateDirectory, parts[1]);
                    if (File.Exists(parts1))
                    {
                        string name = new FileInfo(parts1).Name;
                        if (name == baseTemplate)
                        {
                            return string.Empty;
                        }
                        Template template = new Template();
                        if (!Pool.HasTemplate(name))
                        {
                            Template newTemplate = new(parts1);
                            Pool.AddTemplate(newTemplate);
                            template = newTemplate;
                        }
                        else
                        {
                            Template? _ = Pool.GetTemplate(name);
                            if (_ != null && _.UnparsedTemplate != null && _.TemplateName != null)
                            {
                                template = _;
                            }
                        }

                        string outputValue = "";
                        if (template.UnparsedTemplate != null)
                        {
                            outputValue = ParseTemplate(template, baseTemplate);
                        }

                        return outputValue;
                    }
                    break;
            }
        }
        else
        {
            switch (value)
            {
                default:
                    return string.Empty;
            }
        }
        return string.Empty;
    } 
    private static string Parse(string value, string baseTemplate, List<Post> posts)
    {
        value = value.Replace("{{", "").Replace("}}", "");
        if (value.Contains(":"))
        {
            string[] parts = value.Split(':');
            parts[0] = parts[0].Trim();
            
            switch (parts[0])
            {
                case "template":
                    parts[1] = parts[1].Trim();
                    string parts1 = Path.Join(Program.Config.TemplateDirectory, parts[1]);
                    if (File.Exists(parts1))
                    {
                        string name = new FileInfo(parts1).Name;
                        if (name == baseTemplate)
                        {
                            return string.Empty;
                        }
                        Template template = new Template();
                        if (!Pool.HasTemplate(name))
                        {
                            Template newTemplate = new(parts1);
                            Pool.AddTemplate(newTemplate);
                            template = newTemplate;
                        }
                        else
                        {
                            Template? _ = Pool.GetTemplate(name);
                            if (_ != null && _.UnparsedTemplate != null && _.TemplateName != null)
                            {
                                template = _;
                            }
                        }

                        string outputValue = "";
                        if (template.UnparsedTemplate != null)
                        {
                            outputValue = ParseTemplate(template, baseTemplate, posts);
                        }

                        return outputValue;
                    }
                    break;
            }
        }
        else
        {
            switch (value)
            {
                case "posts":
                    string returnValue = "";
                    List<int> years = new List<int>();
                    foreach (Post post in posts)
                    {
                        if (!years.Contains(post.DateTime.Year))
                        {
                            returnValue = $"<h2>{post.DateTime.Year}</h2>";
                            years.Add(post.DateTime.Year);
                        }

                        Template? template;
                        if (Pool.HasTemplate("post.html"))
                        {
                            template = Pool.GetTemplate("post.html");
                        }
                        else
                        {
                            Template newTemplate = new(Path.Join(Program.Config.TemplateDirectory, "post.html"));
                            Pool.AddTemplate(newTemplate);
                            template = newTemplate;
                        }
                        if (template != null)
                        {
                            string postListSnippet = ParseTemplate(template, baseTemplate, post);
                            returnValue += postListSnippet;
                        }
                    }
                    return returnValue;
                default:
                    return string.Empty;
            }
        }
        return string.Empty;
    } 
    public static string ParseTemplate(Template template, string baseTemplate, Post post, bool init = false)
    {
        if (template.TemplateName == null) return string.Empty;
        if (template.UnparsedTemplate == null) return string.Empty;
        if (template.UnparsedTemplate.Contains($"{{template: {baseTemplate}}}"))         
        {
            Logging.Log($"Recursion found in template {template.TemplateName} ({template.TemplateName} refers to {baseTemplate}, which is where we came from), exiting early..", "fatal", Logging.ColourScheme.Fatal);
            Environment.Exit(1);
        }
        
        MatchCollection matches = Regex.Matches(template.UnparsedTemplate,@"({{)(.*?)(}})");

        string output = template.UnparsedTemplate;
        foreach(Match m in matches)
        {
            output = output.Replace(m.Value, Parse(m.Value, template.TemplateName, post));
        }
        return output;
    }
    public static string ParseTemplate(Template template, string baseTemplate, bool init = false)
    {
        if (template.TemplateName == null) return string.Empty;
        if (template.UnparsedTemplate == null) return string.Empty;
        if (template.UnparsedTemplate.Contains($"{{template: {baseTemplate}}}"))         
        {
            Logging.Log($"Recursion found in template {template.TemplateName} ({template.TemplateName} refers to {baseTemplate}, which is where we came from), exiting early..", "fatal", Logging.ColourScheme.Fatal);
            Environment.Exit(1);
        }
        
        MatchCollection matches = Regex.Matches(template.UnparsedTemplate,@"({{)(.*?)(}})");

        string output = template.UnparsedTemplate;
        foreach(Match m in matches)
        {
            output = output.Replace(m.Value, Parse(m.Value, template.TemplateName));
        }
        return output;
    }
    public static string ParseTemplate(Template template, string baseTemplate, List<Post> posts, bool init = false)
    {
        if (template.TemplateName == null) return string.Empty;
        if (template.UnparsedTemplate == null) return string.Empty;
        if (template.UnparsedTemplate.Contains($"{{template: {baseTemplate}}}"))         
        {
            Logging.Log($"Recursion found in template {template.TemplateName} ({template.TemplateName} refers to {baseTemplate}, which is where we came from), exiting early..", "fatal", Logging.ColourScheme.Fatal);
            Environment.Exit(1);
        }
     
        MatchCollection matches = Regex.Matches(template.UnparsedTemplate,@"({{)(.*?)(}})");
        string output = template.UnparsedTemplate;
        foreach(Match m in matches)
        {
            output = output.Replace(m.Value, Parse(m.Value, template.TemplateName, posts));
        }
        return output;
    }
}