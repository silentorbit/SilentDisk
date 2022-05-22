
namespace SilentOrbit.Disk;

/// <summary>
/// Relative path to a directory.
/// When combined with a DirPath it will result in a DirPath
/// </summary>
public class RelDirPath : RelDiskPath
{
    public RelDirPath(string path) : base(path)
    {

    }

    public RelFilePath Combine(params RelFilePath[] parts)
    {
        var path = this.RelativePath;
        foreach (var p in parts)
            path = PathCombine(path, p.RelativePath);

        return new RelFilePath(path);
    }

    public static RelFilePath operator +(RelDirPath a, RelFilePath b)
    {
        var path = PathCombine(a.RelativePath, b.RelativePath);
        return new RelFilePath(path);
    }

    public static RelDirPath operator +(RelDirPath a, RelDirPath b)
    {
        var path = PathCombine(a.RelativePath, b.RelativePath);
        return new RelDirPath(path);
    }
}
