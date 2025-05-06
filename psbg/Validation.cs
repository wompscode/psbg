#region Imports
using HtmlAgilityPack;
using static psbg.Logging;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#endregion
namespace psbg;

internal static class Validation
{
    public static bool ValidateLoadedPostTemplate(HtmlDocument doc)
    {
        // Only checks if the template would be unreadable, not for other issues.
        var articleTitle = doc.GetElementbyId("psbg_articleTitle");
        var articleAuthor = doc.GetElementbyId("psbg_articleAuthor");
        var articleContent = doc.GetElementbyId("psbg_articleContent");

        if (articleTitle == null || articleAuthor == null ||
            articleContent == null) return false;
        return true;
    }

    public static bool ValidateLoadedPostListTemplate(HtmlDocument doc)
    {
        var pageReturn = doc.GetElementbyId("psbg_list");
        if (pageReturn == null) return false;
        return true;
    }
    public static void ValidatePostTemplate(string template)
    {
        int warnings = 0;
        int issues = 0;
        
        var doc = new HtmlDocument();
        doc.Load(Path.Join(Program.Config.TemplateDirectory, "postTemplate.html"));
        
        var pageReturn = doc.GetElementbyId("psbg_goBack");
        var pageTitle = doc.GetElementbyId("psbg_pageTitle");
        var articleTitle = doc.GetElementbyId("psbg_articleTitle");
        var articleDate = doc.GetElementbyId("psbg_articleDate");
        var articleAuthor = doc.GetElementbyId("psbg_articleAuthor");
        var articleContent = doc.GetElementbyId("psbg_articleContent");

        if (pageReturn == null)
        {
            warnings++;
            ValidationOutput(template, "warning", "psbg_goBack", "a");
        }

        if (pageTitle == null)
        {
            warnings++;
            ValidationOutput(template, "warning", "psbg_pageTitle", "title");
        }
        if (articleTitle == null)
        {
            issues++;
            ValidationOutput(template, "issue", "psbg_articleTitle", "h1");
        }
        if (articleAuthor == null)
        {
            warnings++;
            ValidationOutput(template, "warning", "psbg_articleAuthor", "span");
        }
        if (articleDate == null)
        {
            warnings++;
            ValidationOutput(template, "warning", "psbg_articleDate", "span");
        }
        if (articleContent == null)
        {
            issues++;
            ValidationOutput(template, "issue", "psbg_articleContent", "div");
        }

        Log($"{template}: {warnings} warning(s), {issues} issue(s).", "validation: complete", ColourScheme.Finish);
        Log($"{template}: warnings are not fatal issues, but issues can make articles unreadable.", "validation: complete", ColourScheme.Finish);
    }

    public static void ValidatePostList(string template)
    {
        int warnings = 0;
        int issues = 0;
        
        var doc = new HtmlDocument();
        doc.Load(Path.Join(Program.Config.TemplateDirectory, "postList.html"));
        
        var list = doc.GetElementbyId("psbg_list");

        if (list == null)
        {
            issues++;
            ValidationOutput(template, "issue", "psbg_list", "ul");
        }
        Log($"{template}: {warnings} warning(s), {issues} issue(s).", "validation: complete", ColourScheme.Finish);
        Log($"{template}: warnings are not fatal issues, but issues can make articles unreadable.", "validation: complete", ColourScheme.Finish);
    }

    private static void ValidationOutput(string template, string type, string element, string tag)
    {
        if (type == "issue")
        {
            Log($"{template}: missing {element} (<{tag} id=\"{element}\"></{tag}>)", $"validation: {type}", ColourScheme.Fatal);
        } else if (type == "warning")
        {
            Log($"{template}: missing {element} (<{tag} id=\"{element}\"></{tag}>)", $"validation: {type}", ColourScheme.Warning);
        }
    }
}