using System.Text.Json;
using HtmlAgilityPack;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace psbg;

class Program
{
    // Super unclean single-file initial version
    struct Config
    {
        public string OutputDirectory { get; set; }
        public string PostDirectory { get; set; }
        public string TemplateDirectory { get; set; }
    }
    
    struct YamlPostInfo
    {
        public string Title;
        public string Author;
        public string Date;
    }

    internal struct PostInfo
    {
        public string Title;
        public string Author;
        public DateTime Date;
        public string FileName;
    }
    
    public static List<PostInfo> Posts = new List<PostInfo>();
    public static string PostListOutput = "";
    public static string OutputDirectory = "";
    public static string PostDirectory = "";
    public static string TemplateDirectory = "";
    static void Main(string[] args)
    {
        Console.WriteLine($"psbg - Phoebe's Static Blog Generator");

        if (!File.Exists("./config.json"))
        {
            Console.WriteLine("[fatal] No config.");
            return;
        }
        Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText("./config.json"));
        /*OutputDirectory = @"./out/";
        PostDirectory = @"./posts/";*/
        PostListOutput = $@"posts.html";
        OutputDirectory = config.OutputDirectory;
        if(!Directory.Exists(OutputDirectory))
        {
            Console.WriteLine("[warning] Output directory does not exist, creating..");
            Directory.CreateDirectory(OutputDirectory);
        }
        PostDirectory = config.PostDirectory;
        if(!Directory.Exists(PostDirectory))
        {
            Console.WriteLine("[fatal] No posts directory.");
            return;
        }
        TemplateDirectory = config.TemplateDirectory;
        if (!Directory.Exists(TemplateDirectory))
        {
            Console.WriteLine("[fatal] No templates.");
            return;
        }
        Console.WriteLine($"[psbg] Converting all .md files in {PostDirectory} to HTML, outputting to {OutputDirectory}.");
        string[] files = Directory.GetFiles(PostDirectory, "*.md");
        foreach (string file in files)
        {
            FileInfo fileInfo = new FileInfo(file);
            Console.WriteLine($"[post-conversion] {fileInfo.Name}");
            PostMarkdownToPostHtml(file, @$"{Path.Join(OutputDirectory, Path.GetFileNameWithoutExtension(fileInfo.Name) + ".html")}");
        }
        Console.WriteLine($"[psbg] Creating posts.html, outputting to {OutputDirectory}.");
        GeneratePostList(Path.Join(OutputDirectory, "posts.html"));
    }

    public static bool GeneratePostList(string output)
    {
        Posts.Sort((info, postInfo) => DateTime.Compare(info.Date, postInfo.Date));
        Posts.Reverse();
        var doc = new HtmlDocument();
        doc.Load(Path.Join(TemplateDirectory, "postList.html"));
        List<int> years = new List<int>();
        
        var list = doc.GetElementbyId("psbg_list");
        foreach (PostInfo post in Posts)
        {
            if (!years.Contains(post.Date.Year))
            {
                var year = HtmlNode.CreateNode($"<h2>{post.Date.Year}</h2>");
                year.Id = $"psbg_{post.Date.Year}";
                list.AppendChild(year);
                years.Add(post.Date.Year);
            }
            HtmlNode postElement = HtmlNode.CreateNode($"<li><a href=\"{post.FileName}\">{post.Title}</a> - {post.Author} ({post.Date.ToShortDateString()})</li>");
            list.AppendChild(postElement);
        }

        doc.Save(output);
        return true;
    }
    
    public static bool PostMarkdownToPostHtml(string input, string output)
    {
        if (!File.Exists(input)) return false;
        
        string fileContent = File.ReadAllText(input);
        FileInfo fileInfo = new FileInfo(input);
        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();
        StringWriter writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);

        MarkdownDocument document = Markdown.Parse(fileContent, pipeline);
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        var p = new PostInfo();
        var yp = new YamlPostInfo();
        if (yamlBlock != null)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();
            string yaml = fileContent.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);

            yp = deserializer.Deserialize<YamlPostInfo>(yaml.Replace("---", String.Empty));
            
        }
        p.Title = string.IsNullOrEmpty(yp.Title) ? "No title." : yp.Title;
        p.Author = string.IsNullOrEmpty(yp.Author) ? "No author." : yp.Author;
        p.Date = DateTime.TryParse(yp.Date, out DateTime date) ? date : DateTime.Now;
        p.FileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + ".html";
        Posts.Add(p);
        
        renderer.Render(document);
        writer.Flush();
        string markdownContent = writer.ToString();
        var doc = new HtmlDocument();
        doc.Load(Path.Join(TemplateDirectory, "postTemplate.html"));
        
        var pageReturn = doc.GetElementbyId("psbg_goBack");
        if (pageReturn != null)
        {
            var href = pageReturn.Attributes["href"];
            if (href != null)
            {
                href.Value = PostListOutput;
            } 
        }
        var pageTitle = doc.GetElementbyId("psbg_pageTitle");
        pageTitle.InnerHtml = p.Title;
        var articleTitle = doc.GetElementbyId("psbg_articleTitle");
        articleTitle.InnerHtml = p.Title;
        var articleDate = doc.GetElementbyId("psbg_articleDate");
        articleDate.InnerHtml = p.Date.ToShortDateString();
        var articleAuthor = doc.GetElementbyId("psbg_articleAuthor");
        articleAuthor.InnerHtml = p.Author;
        var articleContent = doc.GetElementbyId("psbg_articleContent");
        articleContent.InnerHtml = markdownContent;
        doc.Save(output);
        return true;
    }
}