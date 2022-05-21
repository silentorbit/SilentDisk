
namespace SilentOrbit.Disk;

/// <summary>
/// Full path to a directory
/// </summary>
public partial class DirPath : FullDiskPath
{
    #region Static

    public static DirPath GetCurrentDirectory() => (DirPath)Directory.GetCurrentDirectory();

    #endregion

    public readonly DirectoryInfo DirectoryInfo;

    public DirPath(DirectoryInfo info) : base(info.FullName)
    {
        this.DirectoryInfo = info;
    }

    public DirPath(string path) : this(new DirectoryInfo(path))
    {
    }

    /// <summary>
    /// Combine a relative or absolute path with a base directory.
    /// </summary>
    /// <param name="baseDir"></param>
    /// <param name="pathRelAbs">Relative or absolute path</param>
    /// <returns></returns>
    public DirPath CombineRelative(string pathRelAbs)
    {
        if (System.IO.Path.IsPathRooted(pathRelAbs))
            return new DirPath(pathRelAbs);
        else
            return CombineDir(pathRelAbs);
    }

    //Only explicit
    public static explicit operator DirPath(string value)
    {
        return new DirPath(value);
    }

    #region Path operations

    public override DirPath Parent
    {
        get
        {
            var parent = DirectoryInfo.Parent;
            if (parent == null)
                return null;
            return new DirPath(parent);
        }
    }

    public DirPath CombineDir(params string[] parts)
    {
        string path = Path;
        foreach (var p in parts)
            path = Combine(path, p);

        return new DirPath(path);
    }

    public void Move(DirPath target) => Directory.Move(Path, target.Path);

    public FilePath CombineFile(params string[] parts)
    {
        string path = Path;
        foreach (var p in parts)
            path = Combine(path, p);

        return new FilePath(path);
    }

    /// <summary>
    /// Accepth both '/' and '\' as separators.
    /// Treat all subpaths as relative paths, trim '\' before combining.
    /// </summary>
    private static string Combine(string basePath, string subPath)
    {
        subPath = subPath.Replace('/', '\\');
        Debug.Assert(System.IO.Path.GetFullPath(basePath) == basePath, "Expected full path");
        Debug.Assert(subPath.StartsWith(@"\") == false, "Found leading / in subPath");
        subPath = subPath.Trim('\\');
        return System.IO.Path.Combine(basePath, subPath);
    }

    #endregion

    #region Directory properties

    public override bool Exists() => Directory.Exists(Path);

    public override string Name
    {
        get
        {
            if (DirectoryInfo.Parent == null)
                return Path;
            else
                return base.Name;
        }
    }

    #endregion

    #region Directory list

    public IEnumerable<DirPath> GetDirectories()
    {
        return GetDirectories(DirectoryInfo.EnumerateDirectories());
    }

    public IEnumerable<DirPath> GetDirectories(string pattern)
    {
        return GetDirectories(DirectoryInfo.EnumerateDirectories(pattern));
    }

    public IEnumerable<DirPath> GetDirectories(string pattern, SearchOption searchOption)
    {
        return GetDirectories(DirectoryInfo.EnumerateDirectories(pattern, searchOption));
    }

    static IEnumerable<DirPath> GetDirectories(IEnumerable<DirectoryInfo> dirs)
    {
        foreach (var d in dirs)
            yield return new DirPath(d);
    }

    public IEnumerable<FilePath> GetFiles()
    {
        return GetFiles(DirectoryInfo.EnumerateFiles());
    }

    public IEnumerable<FilePath> GetFiles(string pattern)
    {
        if (DirectoryInfo.Exists == false)
            return new List<FilePath>();

        return GetFiles(DirectoryInfo.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly));
    }

    public IEnumerable<FilePath> GetFiles(string pattern, SearchOption searchOption)
    {
        if (DirectoryInfo.Exists == false)
            return new List<FilePath>();

        return GetFiles(DirectoryInfo.EnumerateFiles(pattern, searchOption));
    }

    static IEnumerable<FilePath> GetFiles(IEnumerable<FileInfo> files)
    {
        foreach (var f in files)
            yield return new FilePath(f);
    }

    #endregion

    #region Directory operations

    /// <summary>
    /// Create the directory on disk.
    /// </summary>
    public void CreateDirectory()
    {
        Directory.CreateDirectory(Path);
    }

    /// <summary>
    /// Delete all files and folders inside
    /// but leave the root directory intact
    /// </summary>
    public void EmptyDirectory(bool preserveGit = false)
    {
        if (Exists() == false)
        {
            Directory.CreateDirectory(Path);
            return;
        }

        foreach (var f in GetFiles("*", SearchOption.AllDirectories))
        {
            //if (File.GetAttributes(f).HasFlag(FileAttributes.ReadOnly) == false)
            //    continue;

            if (preserveGit && f.Path.Contains("\\.git\\"))
                continue;

            f.SetAttributes(FileAttributes.Normal);
            f.DeleteFile();
        }

        foreach (var d in Directory.GetDirectories(Path))
        {
            if (preserveGit && System.IO.Path.GetFileName(d) == ".git")
                continue;

            Directory.Delete(d, true);
        }
    }

    /// <summary>
    /// Return number of files copied
    /// </summary>
    /// <param name="target"></param>
    /// <param name="overwrite"></param>
    /// <returns>Number of files copied</returns>
    public int CopyDirectory(DirPath target)
    {
        int files = 0;

        target.CreateDirectory();
        foreach (var f in GetFiles())
        {
            f.CopyTo(target.CombineFile(f.FileName));
            files += 1;
        }
        foreach (var d in GetDirectories())
            files += d.CopyDirectory(target.CombineDir(d.Name));

        return files;
    }

    public void DeleteDir()
    {
        if (File.Exists(Path))
            throw new InvalidOperationException("Expected a directory, found a file");

        while (Directory.Exists(Path))
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.Fail(ex.Message);
                System.Threading.Thread.Sleep(500);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                Debug.Fail(ex.Message);
                System.Threading.Thread.Sleep(3000);
            }
        }
    }

    /// <summary>
    /// Delete directory and all files even if they are readonly
    /// </summary>
    public void DeleteDirReadOnly()
    {
        if (File.Exists(Path))
            throw new InvalidOperationException("Expected a directory, found a file");

        //First try simple method
        try
        {
            Directory.Delete(Path, recursive: true);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (DirectoryNotFoundException)
        {
            //All good
            return;
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        if (Directory.Exists(Path) == false)
            return;

        foreach (var d in GetDirectories())
        {
            var info = d.DirectoryInfo;
            if (info.Attributes != FileAttributes.Normal)
                info.Attributes = FileAttributes.Normal;
            d.DeleteDirReadOnly();
        }

        foreach (var f in GetFiles())
        {
            f.DeleteFile(); //Handles readonly attribute
        }

        Directory.Delete(Path);
    }

    public void DeleteEmptyDir()
    {
        Directory.Delete(Path, recursive: false);
    }

    #endregion

    #region RelPath operations

    public static RelFilePath operator -(FilePath path, DirPath root)
    {
        if (path.Path.StartsWith(root.Path + System.IO.Path.DirectorySeparatorChar) == false)
            throw new ArgumentException("path must be in the Source directory");

        var rel = path.Path.Substring(root.Path.Length).TrimStart('\\');
        return new RelFilePath(rel);
    }


    public static FilePath operator +(DirPath root, RelFilePath rel)
    {
        return new FilePath(System.IO.Path.Combine(root.Path, rel.RelativePath));
    }


    public static RelDirPath operator -(DirPath path, DirPath root)
    {
        if (path.Path.StartsWith(root.Path + System.IO.Path.DirectorySeparatorChar) == false)
        {
            throw new ArgumentException("path must be in the Source directory");
        }

        var rel = path.Path.Substring(root.Path.Length).TrimStart('\\');
        return new RelDirPath(rel);
    }

    public static DirPath operator +(DirPath root, RelDirPath rel)
    {
        return new DirPath(System.IO.Path.Combine(root.Path, rel.RelativePath));
    }

    #endregion
}
