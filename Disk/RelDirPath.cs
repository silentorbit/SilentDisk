
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
            path = Path.Combine(path, p.RelativePath);

        return new RelFilePath(path);
    }

}
