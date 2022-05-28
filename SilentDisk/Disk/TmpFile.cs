using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilentOrbit.Disk;

public class TmpFile : FilePath, IDisposable
{
    internal TmpFile(FileInfo info) : base(info)
    {
    }

    public void Dispose()
    {
        //Delete temporary file if not moved elsewhere
        if (Exists())
            DeleteFile();
    }
}
