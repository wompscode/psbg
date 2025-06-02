#region Imports
using System.Drawing;
#endregion

namespace psbg;

public class Structs
{
    internal struct Config
    {
        public string OutputDirectory { get; set; }
        public string PostDirectory { get; set; }
        public string TemplateDirectory { get; set; }
        public string PostListOutput { get; set; }
        public string[] Extensions { get; set; }
        public bool SkipValidation { get; set; }
    }

    public record struct Post
    {
        public string Title;
        public string Author;
        public string Date;
        public DateTime DateTime;
        public string FileName;
        public string Summary;
        public string Content;
    }
    
    public struct ConsoleColourScheme
    {
        public ConsoleColourSet Init { get; set; }
        public ConsoleColourSet Warning { get; set; }
        public ConsoleColourSet Status { get; set; }
        public ConsoleColourSet Fatal { get; set; }
        public ConsoleColourSet Verbose { get; set; }
        public ConsoleColourSet Finish { get; set; }
    }
    
    public struct ConsoleColourSet
    {
        public Color Prefix { get; init; }
        public Color Message { get; init; }
    }
}