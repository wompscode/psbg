namespace psbg;

#region ReSharper
// ReSharper disable once InconsistentNaming
#endregion

internal static class IO
{
    internal static void CopyFiles(string from, string to, string[] ext)
    {
        // adapted from https://stackoverflow.com/a/35742118 - I'm lazy, and reinventing the wheel is only fun sometimes.
        foreach (string dir in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Join(to, dir.Remove(0, from.Length)));
        foreach (string file in Directory.GetFiles(from, "*.*",  SearchOption.AllDirectories))
            if(ext.Contains(Path.GetExtension(file))) File.Copy(file, Path.Join(to, file.Remove(0, from.Length)), true);
    }
}