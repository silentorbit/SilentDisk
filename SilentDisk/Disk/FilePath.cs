using System.Security.Cryptography;

namespace SilentOrbit.Disk;

/// <summary>
/// Full path to a file
/// </summary>
public class FilePath : FullDiskPath
{
    public readonly FileInfo FileInfo;

    public FilePath(FileInfo info) : base(info.FullName)
    {
        this.FileInfo = info;
    }

    public FilePath(string path) : this(new FileInfo(path))
    {
    }

    //Only explicit
    public static explicit operator FilePath(string value)
    {
        return new FilePath(value);
    }

    #region File operations

    public override bool Exists() => File.Exists(Path);

    public void ClearReadOnlyAttribute()
    {
        if (FileInfo.Exists == false)
            return;

        var a = FileInfo.Attributes;
        if ((a & FileAttributes.ReadOnly) != 0)
        {
            FileInfo.Attributes = a & ~FileAttributes.ReadOnly;
        }
    }

    public void SetAttributes(FileAttributes fileAttributes)
    {
        FileInfo.Attributes = fileAttributes;
    }

    public void DeleteFile()
    {
        ClearReadOnlyAttribute();
        try
        {
            File.Delete(Path);
        }
        catch (IOException ex)
        {
            Debug.Fail(ex.Message);
            System.Threading.Thread.Sleep(500);
            File.Delete(Path);
        }
    }

    #endregion

    #region PathOperations

    public override DirPath Parent => new DirPath(FileInfo.Directory);

    /// <summary>
    /// Adds to path without adding path separator
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public FilePath AppendPath(string text)
    {
        return new FilePath(Path + text);
    }

    /// <summary>
    /// Replace the current extension with a new extension.
    /// </summary>
    /// <param name="newExtension"></param>
    /// <returns></returns>
    public FilePath GetWithExtension(string newExtension)
    {
        var dir = System.IO.Path.GetDirectoryName(Path);
        var file = System.IO.Path.GetFileNameWithoutExtension(Path);
        if (string.IsNullOrEmpty(newExtension) == false)
            newExtension = "." + newExtension.TrimStart('.');

        var disk = new FilePath(System.IO.Path.Combine(dir, file + newExtension));
        return disk;
    }

    public void SetLastWriteTimeUtc(FilePath file)
    {
        var sourceUTC = File.GetLastWriteTimeUtc(file.Path);
        var targetUTC = File.GetLastWriteTimeUtc(Path);
        if (sourceUTC != targetUTC)
        {
            File.SetLastWriteTimeUtc(Path, sourceUTC);
        }
    }

    public void SetLastWriteTimeUtc(DateTime modifiedUTC)
    {
        File.SetLastWriteTimeUtc(Path, modifiedUTC);
    }

    public FilePath ReplaceEnd(string expectedEnd, string newEnd)
    {
        if (Path.EndsWith(expectedEnd))
            return new FilePath(Path.Substring(0, Path.Length - expectedEnd.Length) + newEnd);
        else
            throw new ArgumentException("Path does not end in " + expectedEnd + " path: " + Path);
    }

    #endregion

    #region String operations

    public string FileName => Name;

    #endregion

    #region File content operations

    public byte[] ReadAllBytes()
    {
        return File.ReadAllBytes(Path);
    }

    public string ContentHash()
    {
        var content = ReadAllBytes();
        using (SHA1 sha1 = SHA1.Create())
        {
            return BitConverter.ToString(sha1.ComputeHash(content));
        }
    }

    public void WriteAllBytes(byte[] data)
    {
        Parent.CreateDirectory();
        File.WriteAllBytes(Path, data);
    }

    public void WriteAllBytesRO(byte[] data)
    {
        Parent.CreateDirectory();

        //Read only generated files
        if (Exists())
        {
            File.SetAttributes(Path, FileAttributes.Normal);
        }

        File.WriteAllBytes(Path, data);
        SetAttributes(FileAttributes.ReadOnly);
    }

    public void Move(FilePath target) => File.Move(Path, target.Path);
    public FilePath Move(DirPath target)
    {
        var targetFile = target.CombineFile(Name);
        File.Move(Path, targetFile.Path);
        return targetFile;
    }

    public string ReadAllText() => File.ReadAllText(Path, Encoding.UTF8);

    public void WriteAllTextRO(string text)
    {
        Parent.CreateDirectory();

        //Read only generated files
        if (Exists())
            FileInfo.Attributes = FileAttributes.Normal;

        File.WriteAllText(Path, text, new UTF8Encoding(false, true));
        SetAttributes(FileAttributes.ReadOnly);
    }

    static readonly Regex reUniqueSuffix = new Regex(@"^(.*) \(([0-9]+)\)$", RegexOptions.Compiled);

    /// <summary>
    /// Makes sure the file doesn't exist.
    /// Add " (1)" and increase numbers until there is no existing file there.
    /// </summary>
    public FilePath MakeUnique()
    {
        var targetPath = this;
        while (targetPath.Exists())
        {
            var m = reUniqueSuffix.Match(targetPath.NameWithoutExtension);
            if (m.Success)
            {
                var filename = m.Groups[1].Value + " (" + (int.Parse(m.Groups[2].Value) + 1) + ")" + targetPath.Extension;
                targetPath = targetPath.Parent.CombineFile(filename);
            }
            else
            {
                targetPath = targetPath.Parent.CombineFile(targetPath.NameWithoutExtension + " (1)" + targetPath.Extension);
            }
        }
        return targetPath;
    }

    public void WriteAllText(string text)
    {
        Parent.CreateDirectory();

        //Atomic
        var tmp = FindTmp();
        File.WriteAllText(tmp.Path, text, new UTF8Encoding(false, true));

        //Read only generated files
        if (Exists())
            FileInfo.Attributes = FileAttributes.Normal;

        var delete = this.AppendPath("-delete");
        if (Exists())
            File.Move(Path, delete.Path);
        File.Move(tmp.Path, Path);
        if (delete.Exists())
            delete.DeleteFile();
    }

    /// <summary>
    /// Atomic write to file using a stream.
    /// </summary>
    public void WriteStream(Action<Stream> action)
    {
        Parent.CreateDirectory();

        //Atomic
        var tmp = FindTmp();
        using (var stream = new FileStream(tmp.Path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            action(stream);
        }

        //Read only generated files
        if (Exists())
            FileInfo.Attributes = FileAttributes.Normal;

        var delete = this.AppendPath("-delete");
        if (Exists())
            File.Move(Path, delete.Path);
        File.Move(tmp.Path, Path);
        if (delete.Exists())
            delete.DeleteFile();
    }

    /// <summary>
    /// Find a new filename that doesn't exist on disk
    /// </summary>
    public TmpFile FindTmp()
    {
        var r = new Random();
        while (true)
        {
            var tmp = AppendPath("-" + r.Next() + "-tmp");
            if (tmp.Exists() == false)
                return new TmpFile(tmp.FileInfo);
        }
    }

    public FilePath CopyTo(DirPath targetDir)
    {
        var target = targetDir.CombineFile(Name);
        CopyTo(target);
        return target;
    }

    public void CopyTo(FilePath target)
    {
        if ((FileInfo.Attributes & FileAttributes.Encrypted) != 0)
            FileInfo.Attributes &= ~FileAttributes.Encrypted;

        if (target.FileInfo.IsReadOnly)
            target.ClearReadOnlyAttribute();

        File.Copy(Path, target.Path, overwrite: true);

        if (FileInfo.IsReadOnly) //Has now been copied to target file
            target.ClearReadOnlyAttribute(); //Required to allow modifying times

        //Preserve file dates
        var sourceCreationUTC = File.GetCreationTimeUtc(Path);
        var targetCreationUTC = File.GetCreationTimeUtc(Path);
        if (targetCreationUTC != sourceCreationUTC)
            File.SetCreationTimeUtc(target.Path, targetCreationUTC);
        target.SetLastWriteTimeUtc(this);
    }

    #endregion File content operations

}
