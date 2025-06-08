namespace SilentOrbit.Disk;

/// <summary>
/// Implementations to support .NET3.5
/// </summary>
internal static class Extensions
{

#if NET35

    public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo dir)
    {
        foreach (var d in dir.GetDirectories())
            yield return d;
    }

    public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo dir, string pattern)
    {
        foreach (var d in dir.GetDirectories(pattern))
            yield return d;
    }

    public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo dir, string searchPattern, SearchOption searchOption)
    {
        foreach (var d in dir.GetDirectories(searchPattern, searchOption))
            yield return d;
    }

    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo dir)
    {
        foreach (var f in dir.GetFiles())
            yield return f;
    }

    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo dir, string pattern)
    {
        foreach (var f in dir.GetFiles(pattern))
            yield return f;
    }

    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo dir, string searchPattern, SearchOption searchOption)
    {
        foreach (var f in dir.GetFiles(searchPattern, searchOption))
            yield return f;
    }

#endif

}
