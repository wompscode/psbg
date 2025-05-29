#region Imports
using System.Text.Json;
using HtmlAgilityPack;
using static psbg.Structs;
using static psbg.IO;
using static psbg.Validation;
using static psbg.Logging;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
#endregion

#region ReSharper
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#endregion

// https://womp.gay/posts/psbg.html for a retrospective on its creation

namespace psbg;

internal static class Program
{
    private static readonly List<PostInfo> Posts = new();
    public static Config Config;

    static void Main(string[] args)
    {
        Log("phoebe's static blog generator", "psbg", ColourScheme.Init);

        if (!File.Exists("./config.json"))
        {
            Log("missing config", "fatal", ColourScheme.Fatal);
            return;
        }
        
        Config = JsonSerializer.Deserialize<Config>(File.ReadAllText("./config.json"));
        
        if (Config.Extensions.Length <= 0) Config.Extensions = [".png", ".jpg", ".jpeg", ".txt"];
        if(!Directory.Exists(Config.OutputDirectory))
        {
            Log("output directory doesn't exist, creating..", "warning", ColourScheme.Warning);
            Directory.CreateDirectory(Config.OutputDirectory);
        }
        if(!Directory.Exists(Config.PostDirectory))
        {
            Log("missing posts..", "fatal", ColourScheme.Fatal);
            return;
        }
        if (!Directory.Exists(Config.TemplateDirectory))
        {
            Log("no templates.", "fatal", ColourScheme.Fatal);
            return;
        } 
        
        
        if (args.Length >= 1)
        {
            if (!string.IsNullOrEmpty(args[0]))
            {
                string arg = args[0];
                arg = arg.ToLower();
                if (arg == "validate_pt")
                {
                    ValidatePostTemplate("postTemplate.html");
                    return;
                }

                if (arg == "validate_pl")
                {
                    ValidatePostList("postList.html");
                    return;
                }

                if (arg == "validate_all")
                {
                    ValidatePostTemplate("postTemplate.html");
                    ValidatePostList("postList.html");
                    return;
                }
                
                if (arg == "help")
                {
                    Log("validate_pt, validate_pl, validate_all", "help", ColourScheme.Init);
                    return;
                }
            }
        }
        Log($"converting all .md files in {Config.PostDirectory} to HTML, outputting to {Config.OutputDirectory}", "post-conversion", ColourScheme.Status);
        string[] files = Directory.GetFiles(Config.PostDirectory, "*.md");
        foreach (string file in files)
        {
            FileInfo fileInfo = new FileInfo(file);
            Log($"{fileInfo.Name}", "post-conversion", ColourScheme.Status);
            PostMarkdownToPostHtml(file, @$"{Path.Join(Config.OutputDirectory, Path.GetFileNameWithoutExtension(fileInfo.Name) + ".html")}");
        }
        
        Log($"creating posts.html, outputting to {Config.OutputDirectory}", "post-list", ColourScheme.Status);
        GeneratePostList(Path.Join(Config.OutputDirectory, Config.PostListOutput));
        
        Log($"copying all files with extensions listed in config from {Config.PostDirectory} to {Config.OutputDirectory}", "copy-files", ColourScheme.Status);
        CopyFiles(Config.PostDirectory, Config.OutputDirectory, Config.Extensions);
    }

    private static void GeneratePostList(string output)
    {
        Posts.Sort((info, postInfo) => DateTime.Compare(info.DateTime, postInfo.DateTime));
        Posts.Reverse();
        
        var doc = new HtmlDocument();
        doc.Load(Path.Join(Config.TemplateDirectory, "postList.html"));
        if(ValidateLoadedPostListTemplate(doc) == false)
        {
            if (!Config.SkipValidation)
            {
                Log($"failed to generate post list, try validating your post list template. (psbg validate_pl)", "fatal", ColourScheme.Fatal);
                return;  
            } 
        }

        List<int> years = new List<int>();
        var list = doc.GetElementbyId("psbg_list");
        foreach (PostInfo post in Posts)
        {
            if (!years.Contains(post.DateTime.Year))
            {
                var year = HtmlNode.CreateNode($"<h2>{post.DateTime.Year}</h2>");
                year.Id = $"psbg_{post.DateTime.Year}";
                list.AppendChild(year);
                
                years.Add(post.DateTime.Year);
            }

            string postListSnippet = File.ReadAllText(Path.Join(Config.TemplateDirectory, "postList.snippet"));
            postListSnippet = postListSnippet.Replace("{{postTitle}}", post.Title)
                .Replace("{{postAuthor}}", post.Author)
                .Replace("{{postDateTime}}", post.DateTime.ToShortDateString())
                .Replace("{{postFileName}}", post.FileName)
                .Replace("{{postSummary}}", post.Summary);
            HtmlNode postElement = HtmlNode.CreateNode($"{postListSnippet}");
            list.AppendChild(postElement);
        }

        doc.Save(output);
    }

    private static void PostMarkdownToPostHtml(string input, string output)
    {
        if (!File.Exists(input)) return;

        string fileContent = File.ReadAllText(input);
        FileInfo fileInfo = new FileInfo(input);
        
        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();
        StringWriter writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);

        MarkdownDocument document = Markdown.Parse(fileContent, pipeline);
        // Parsing Markdown frontmatter
        // https://atashbahar.com/post/2020-06-16-extract-front-matter-in-dotnet-with-markdig helped tremendously with this.
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        PostInfo postInfo = new PostInfo();
        if (yamlBlock != null)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();
            string yaml = fileContent.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);

            postInfo = deserializer.Deserialize<PostInfo>(yaml.Replace("---", String.Empty));
        }
        postInfo.Title = string.IsNullOrEmpty(postInfo.Title) ? "No title." : postInfo.Title;
        postInfo.Author = string.IsNullOrEmpty(postInfo.Author) ? "No author." : postInfo.Author;
        postInfo.DateTime = DateTime.TryParse(postInfo.Date, out DateTime date) ? date : DateTime.Now;
        postInfo.Date = postInfo.DateTime.ToShortDateString();
        postInfo.FileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + ".html";
        Posts.Add(postInfo);
        
        renderer.Render(document);
        writer.Flush();
        string markdownContent = writer.ToString();
        
        var doc = new HtmlDocument();
        doc.Load(Path.Join(Config.TemplateDirectory, "postTemplate.html"));
        
        HtmlNode pageReturn = doc.GetElementbyId("psbg_goBack");
        if (pageReturn != null)
        {
            HtmlAttribute href = pageReturn.Attributes["href"];
            if (href != null)
            {
                href.Value = Config.PostListOutput;
            }
            else
            {
                pageReturn.Attributes.Add("href", Config.PostListOutput);
            }
        }
        
        if(ValidateLoadedPostTemplate(doc) == false)
        {
            if (!Config.SkipValidation)
            {
                Log($"failed to generate post {postInfo.FileName}, try validating your post template. (psbg validate_pt)", "fatal", ColourScheme.Fatal);
                Posts.Remove(postInfo);
                return;
            } 
        }
        
        var pageTitle = doc.GetElementbyId("psbg_pageTitle");
        var articleTitle = doc.GetElementbyId("psbg_articleTitle");
        var articleDate = doc.GetElementbyId("psbg_articleDate");
        var articleAuthor = doc.GetElementbyId("psbg_articleAuthor");
        var articleContent = doc.GetElementbyId("psbg_articleContent");

        if(pageTitle != null) pageTitle.InnerHtml = postInfo.Title;
        if(articleTitle != null) articleTitle.InnerHtml = postInfo.Title;
        if(articleDate != null) articleDate.InnerHtml = postInfo.DateTime.ToShortDateString();
        if(articleAuthor != null) articleAuthor.InnerHtml = postInfo.Author;
        if(articleContent != null) articleContent.InnerHtml = markdownContent;
        
        doc.Save(output);
    }
}