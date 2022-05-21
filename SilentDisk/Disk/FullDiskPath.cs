
namespace SilentOrbit.Disk;

/// <summary>
/// Base class for <see cref="FilePath"/> and <see cref="DirPath"/>
/// </summary>
public abstract class FullDiskPath : IComparable
{
    /// <summary>
    /// Full path
    /// </summary>
    public readonly string Path;

    public abstract bool Exists();

    protected FullDiskPath(string value)
    {
        if (value.EndsWith(":\\"))
        {
            //Keep "C:\\"
        }
        else
        {
            value = value.TrimEnd('\\');
        }

        var full = ValidateFullPath(value);

        Path = full;
    }

    string ValidateFullPath(string value)
    {
        var full = System.IO.Path.GetFullPath(value);
        if (full == value)
            return full;

        if (full.ToLowerInvariant() == value.ToLowerInvariant())
            return full;

        Debug.Assert(full.Length == value.Length);
        for (int n = 0; n < full.Length; n++)
            Debug.Assert(full[n] == value[n]);

        throw new ArgumentException("Expected a full path: " + value + " != " + full);
    }

    #region Path string operations

    /// <summary>
    /// File or directory name.
    /// </summary>
    public virtual string Name
    {
        get
        {
            var n = System.IO.Path.GetFileName(Path);
            if (string.IsNullOrEmpty(n))
                throw new InvalidProgramException();
            return n;
        }
    }

    public bool StartsWith(FullDiskPath test)
    {
        return Path.StartsWith(test.Path);
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
