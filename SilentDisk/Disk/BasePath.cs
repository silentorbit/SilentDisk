using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilentOrbit.Disk;

using static System.IO.Path;

public class BasePath
{
    /// <summary>
    /// Full path
    /// </summary>
    public readonly string Path;

    public BasePath(string path)
    {
        path = path.Replace('/', DirectorySeparatorChar);
        path = path.Replace('\\', DirectorySeparatorChar);
        Path = path;
    }

    public string Name => GetFileName(Path);

    public string NameWithoutExtension => GetFileNameWithoutExtension(Path);

    public string Extension => GetExtension(Path);

    protected static string PathCombine(string basePath, string subPath)
    {
        subPath = subPath.Replace('/', DirectorySeparatorChar);
        subPath = subPath.Replace('\\', DirectorySeparatorChar);
        Debug.Assert((subPath.StartsWith(@"\") || subPath.StartsWith(@"/")) == false, "Found leading \\,/ in subPath");
        subPath = subPath.Trim('\\', '/');
        return Combine(basePath, subPath);
    }
}
