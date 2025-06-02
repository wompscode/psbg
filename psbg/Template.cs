using System.Text.RegularExpressions;
using static psbg.Structs;
namespace psbg;

public class Template
{
    public static string Parse(string value, Post post)
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
                    
                    if (File.Exists(Path.Join(Program.Config.TemplateDirectory, parts[1])))
                    {
                        string x = File.ReadAllText(Path.Join(Program.Config.TemplateDirectory, parts[1]));
                        MatchCollection matches = Regex.Matches(x,@"({{)(.*?)(}})");
                        
                        foreach(Match m in matches)
                        {
                            x = x.Replace(m.Value, Parse(m.Value, post));
                        }
                        return x;
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
    public static string Parse(string value)
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
                    
                    if (File.Exists(Path.Join(Program.Config.TemplateDirectory, parts[1])))
                    {
                        string x = File.ReadAllText(Path.Join(Program.Config.TemplateDirectory, parts[1]));
                        MatchCollection matches = Regex.Matches(x,@"({{)(.*?)(}})");
                        
                        foreach(Match m in matches)
                        {
                            x = x.Replace(m.Value, Parse(m.Value));
                        }
                        return x;
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
    public static string ParseTemplate(string template, Post post)
    {
        MatchCollection matches = Regex.Matches(template,@"({{)(.*?)(}})");

        foreach(Match m in matches)
        {
            template = template.Replace(m.Value, Parse(m.Value, post));
        }
        
        return template;
    }
    public static string ParseTemplate(string template)
    {
        MatchCollection matches = Regex.Matches(template,@"({{)(.*?)(}})");

        foreach(Match m in matches)
        {
            template = template.Replace(m.Value, Parse(m.Value));
        }
        
        return template;
    }
}