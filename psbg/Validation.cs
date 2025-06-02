#region Imports
using static psbg.Logging;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#endregion
namespace psbg;

internal static class Validation
{
    public static bool ValidateLoadedPostTemplate(string value)
    {
        bool title = value.Contains("{{title}}");
        bool author = value.Contains("{{author}}");
        bool date = value.Contains("{{date}}");
        bool content = value.Contains("{{content}}");
        
        if (title == false || author == false || content == false || date == false) return false;
        return true;
    }

    public static bool ValidateLoadedPostListTemplate(string value)
    {
        return value.Contains("{{posts}}");
    }
    public static void ValidatePostTemplate(string template)
    {
        int warnings = 0;
        int issues = 0;

        string unparsed = Path.Join(Program.Config.TemplateDirectory, "postTemplate.html");

        bool title = unparsed.Contains("{{pageTitle}}");
        bool articleTitle = unparsed.Contains("{{title}}");
        bool author = unparsed.Contains("{{author}}");
        bool date = unparsed.Contains("{{date}}");
        bool content = unparsed.Contains("{{content}}");

        if (!title)
        {
            warnings++;
            ValidationOutput(template, "warning", "pageTitle", "title");
        }
        if (!articleTitle)
        {
            issues++;
            ValidationOutput(template, "issue", "title", "h1");
        }
        if (!author)
        {
            warnings++;
            ValidationOutput(template, "warning", "author", "span");
        }
        if (!date)
        {
            warnings++;
            ValidationOutput(template, "warning", "date", "span");
        }
        if (!content)
        {
            issues++;
            ValidationOutput(template, "issue", "content", "div");
        }

        Log($"{template}: {warnings} warning(s), {issues} issue(s).", "validation: complete", ColourScheme.Finish);
        Log($"{template}: warnings are not fatal issues, but issues can make articles unreadable.", "validation: complete", ColourScheme.Finish);
    }

    public static void ValidatePostList(string template)
    {
        int warnings = 0;
        int issues = 0;
        
        string unparsed = Path.Join(Program.Config.TemplateDirectory, "postTemplate.html");

        bool posts = unparsed.Contains("{{posts}}");

        if (!posts)
        {
            warnings++;
            ValidationOutput(template, "warning", "posts", "title");
        }

        Log($"{template}: {warnings} warning(s), {issues} issue(s).", "validation: complete", ColourScheme.Finish);
        Log($"{template}: warnings are not fatal issues, but issues can make articles unreadable.", "validation: complete", ColourScheme.Finish);
    }

    private static void ValidationOutput(string template, string type, string element, string tag)
    {
        if (type == "issue")
        {
            Log($"{template}: missing {element} ("+"{"+element+"}"+")", $"validation: {type}", ColourScheme.Fatal);
        } else if (type == "warning")
        {
            Log($"{template}: missing {element} ("+"{"+element+"}"+")", $"validation: {type}", ColourScheme.Warning);
        }
    }
}