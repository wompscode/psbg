#region Imports
using System.Drawing;
using Pastel;
using static psbg.Structs;
#endregion
// borrowed from phdt, palc, etc

#region ReSharper
// ReSharper disable AssignmentInsteadOfDiscard
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
#endregion

namespace psbg;
internal static class Logging
{
    internal static readonly ConsoleColourScheme ColourScheme = new()
    {
        Init = new()
        {
            Prefix = Color.DarkSlateBlue,
            Message = Color.SlateBlue
        },
        Warning = new()
        {
            Prefix = Color.DarkOrange,
            Message = Color.Orange
        },
        Status = new()
        {
            Prefix = Color.DarkViolet,
            Message = Color.BlueViolet
        },
        Fatal = new()
        {
            Prefix = Color.DarkRed,
            Message = Color.Red
        },
        Verbose = new()
        {
            Prefix = Color.DarkCyan,
            Message = Color.Cyan
        },
        Finish = new()
        {
            Prefix = Color.DarkGreen,
            Message = Color.Green
        }
    };
    public static void Log(string message, string? prefix = null, ConsoleColourSet? colourScheme = null)
    {
        DateTime now = DateTime.Now;
        string _ = $"{(prefix == null ? $"[unknown]" : $"[{prefix}]")} {now:HH:mm:ss}:";
        if (colourScheme.HasValue)
        {
            _ = _.Pastel(colourScheme.Value.Prefix);
        }
        string __ = $" {message}";
        if (colourScheme.HasValue)
        {
            __ = __.Pastel(colourScheme.Value.Message);
        }
        Console.WriteLine(_+__);
    }
}