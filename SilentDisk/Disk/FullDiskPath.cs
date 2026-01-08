
using System.Runtime.InteropServices;

namespace SilentOrbit.Disk;

/// <summary>
/// Represents a file or directory path on disk.
/// Base class for <see cref="FilePath"/> and <see cref="DirPath"/>
/// </summary>
public class FullDiskPath : BasePath, IComparable
{
    public virtual bool Exists() => File.Exists(Path) || Directory.Exists(Path);

    public FullDiskPath(string path) : base(ConstructorPath(path))
    {

    }

    static string ConstructorPath(string path)
    {
        //Keep C:\
        var root = System.IO.Path.GetPathRoot(path);

        // Only trim trailing separators if it's NOT just a root (e.g. trim "C:\Temp\" -> "C:\Temp", but keep "C:\")
        if (path.Length > root.Length)
            path = path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

        var full = System.IO.Path.GetFullPath(path);

        if (PathEquals(full, path))
            return full;

        throw new ArgumentException("Expected a full path: " + path + " != " + full);
    }

    static bool PathEquals(string path1, string path2)
        => string.Equals(path1, path2, PathComparison);

    /// <summary>
    /// Case Sensitivity based on OS
    /// </summary>
    static StringComparison PathComparison =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    #region Path string operations

    public bool StartsWith(FullDiskPath test)
    {
        if (test is null)
            return false;

        if (Path.StartsWith(test.Path, PathComparison) == false)
            return false;

        if (Path.Length == test.Path.Length)
            return true;

        // Boundary Check: Prevent "C:\Program Files" matching "C:\Program"
        // We check if the next char in our Path is a separator.
        char separator = Path[test.Path.Length];
        return separator == System.IO.Path.DirectorySeparatorChar ||
               separator == System.IO.Path.AltDirectorySeparatorChar;
    }

    public bool EndsWith(string name)
    {
        return Path.EndsWith(name, PathComparison);
    }

    public override string ToString() => Path;

    #endregion

    public virtual DirPath Parent => new DirPath(System.IO.Path.GetDirectoryName(Path));

    #region Comparison

    public static bool operator ==(FullDiskPath a, FullDiskPath b)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;

        Debug.Assert(a.GetType() == b.GetType());

        return PathEquals(a.Path, b.Path);
    }

    public static bool operator !=(FullDiskPath a, FullDiskPath b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is FullDiskPath fdp)
            return PathEquals(fdp.Path, Path);
        return false;
    }

    public override int GetHashCode()
    {
        return Path.GetHashCode();
    }

    public int CompareTo(object obj)
    {
        //Assume it's the right type, don't handle compare with other types.
        var o = (FullDiskPath)obj;
        return Path.CompareTo(o.Path);
    }

    #endregion

}
