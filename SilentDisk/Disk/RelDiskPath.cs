
namespace SilentOrbit.Disk;

/// <summary>
/// Relative path within a filesystem to a file or directory.
/// </summary>
public class RelDiskPath
{
    public readonly string RelativePath;

    public RelDiskPath(string relativePath)
    {
        relativePath = relativePath.Replace('/', '\\');

        if (relativePath.StartsWith("\\"))
            throw new ArgumentException("Rel paths can't start with \\");

        RelativePath = relativePath;
    }

    public static explicit operator RelDiskPath(string value)
    {
        return new RelDiskPath(value);
    }

    #region String operations

    public string Name => Path.GetFileName(RelativePath);

    public override string ToString() => RelativePath;

    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(RelativePath);

    public bool StartsWith(string relPath) => RelativePath.StartsWith(relPath);

    #endregion

    #region Path operations

    public static RelDiskPath operator +(RelDiskPath a, RelDiskPath b)
    {
        var path = Path.Combine(a.RelativePath, b.RelativePath);
        return new RelDiskPath(path);
    }

    public RelDirPath Parent => new RelDirPath(Path.GetDirectoryName(RelativePath));

    public RelDiskPath Combine(params string[] parts)
    {
        var path = this;
        foreach (var p in parts)
        {
            path += new RelDiskPath(p);
        }

        return new RelDirPath(path.RelativePath);
    }

    public RelDirPath CombineDir(params string[] parts) => new RelDirPath(Combine(parts).RelativePath);
    public RelFilePath CombineFile(params string[] parts) => new RelFilePath(Combine(parts).RelativePath);

    #endregion

    #region Comparison

    public static bool operator ==(RelDiskPath a, RelDiskPath b)
    {
        if (b is null && a is null)
        {
            return true;
        }

        if (b is null || a is null)
        {
            return false;
        }

        return a.RelativePath == b.RelativePath;
    }

    public static bool operator !=(RelDiskPath a, RelDiskPath b)
    {
        if (b is null && a is null)
        {
            return false;
        }

        if (b is null || a is null)
        {
            return true;
        }

        return a.RelativePath != b.RelativePath;
    }

    public override bool Equals(object obj)
    {
        return ((RelDiskPath)obj).RelativePath == RelativePath;
    }

    public override int GetHashCode()
    {
        return RelativePath.GetHashCode();
    }

    #endregion
}
