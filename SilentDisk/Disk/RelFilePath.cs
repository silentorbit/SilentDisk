namespace SilentOrbit.Disk;

/// <summary>
/// Relative path to a file.
/// When combined with a DirPath it will result in a FilePath
/// </summary>
public class RelFilePath : RelDiskPath
{
    public RelFilePath(string path) : base(path)
    {

    }

    internal RelFilePath AppendPath(string text)
    {
        return new RelFilePath(base.RelativePath + text);
    }

    public static explicit operator RelFilePath(string value)
    {
        if(value == null)
            return null;
        return new RelFilePath(value);
    }
}
