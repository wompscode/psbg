#region Imports
using System.Text.Json;
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
    private static readonly List<Post> Posts = new();
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

        string[] templates = Directory.GetFiles(Config.TemplateDirectory, "*.html", SearchOption.AllDirectories);
        foreach (string template in templates)
        {
            Template temp = new(template);
            Pool.AddTemplate(temp);
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
        
        Template? template = Pool.GetTemplate("postList.html");
        if (template == null || template.UnparsedTemplate == null || template.TemplateName == null)
        {
            Log($"missing post list template.", "fatal", ColourScheme.Fatal);
            return;
        }
        if(ValidateLoadedPostListTemplate(template.UnparsedTemplate) == false)
        {
            if (!Config.SkipValidation)
            {
                Log($"failed to generate post list, try validating your post list template. (psbg validate_pl)", "fatal", ColourScheme.Fatal);
            } 
        }

        string outputValue = Parser.ParseTemplate(template, "postList.html", Posts, true);
        File.WriteAllText(output, outputValue);
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
        Post post = new Post();
        if (yamlBlock != null)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();
            string yaml = fileContent.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);

            post = deserializer.Deserialize<Post>(yaml.Replace("---", String.Empty));
        }
        post.Title = string.IsNullOrEmpty(post.Title) ? "No title." : post.Title;
        post.Author = string.IsNullOrEmpty(post.Author) ? "No author." : post.Author;
        post.DateTime = DateTime.TryParse(post.Date, out DateTime date) ? date : DateTime.Now;
        post.Date = post.DateTime.ToShortDateString();
        post.FileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + ".html";
        renderer.Render(document);
        writer.Flush();
        string markdownContent = writer.ToString();
        post.Content = markdownContent;
        
        Posts.Add(post);
        
        Template? template = Pool.GetTemplate("postTemplate.html");
        if (template == null || template.UnparsedTemplate == null || template.TemplateName == null)
        {
            Log($"missing post template.", "fatal", ColourScheme.Fatal);
            return;
        }
        if(ValidateLoadedPostTemplate(template.UnparsedTemplate) == false)
        {
            if (!Config.SkipValidation)
            {
                Log($"failed to generate post {post.FileName}, try validating your post template. (psbg validate_pt)", "fatal", ColourScheme.Fatal);
                Posts.Remove(post);
                return;
            } 
        }

        string outputValue = Parser.ParseTemplate(template, "postTemplate.html", post, true);
        File.WriteAllText(output, outputValue);
    }
}