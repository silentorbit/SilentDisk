
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
        if (path.EndsWith(":\\"))
        {
            //Keep "C:\\"
        }
        else
        {
            path = path.TrimEnd('\\', '/');
        }

        var full = System.IO.Path.GetFullPath(path);
        if (full == path)
            return full;

        if (full.ToLowerInvariant() == path.ToLowerInvariant())
            return full;

        Debug.Assert(full.Length == path.Length);
        for (int n = 0; n < full.Length; n++)
            Debug.Assert(full[n] == path[n]);

        throw new ArgumentException("Expected a full path: " + path + " != " + full);
    }

    #region Path string operations

    public bool StartsWith(FullDiskPath test)
    {
        if (Path.StartsWith(test.Path) == false)
            return false;

        if (Path.Length == test.Path.Length)
            return true;

        if (Path[test.Path.Length] == System.IO.Path.DirectorySeparatorChar)
            return true;

        return false;
    }

    public bool EndsWith(string name)
    {
        return Path.EndsWith(name);
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

        return a.Path == b.Path;
    }

    public static bool operator !=(FullDiskPath a, FullDiskPath b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return ((FullDiskPath)obj).Path == Path;
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
