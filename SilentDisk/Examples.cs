#if DEBUG
using SilentOrbit.Disk;

namespace SilentOrbit;

internal class Example
{
    public void Demo()
    {
        //Creating paths (nothing is changed on disk)
        FilePath file = (FilePath)@"C:\Hello\World";
        DirPath directory = file.Parent; // C:\Hello
        RelFilePath relative = file - directory; // World
        relative = relative.AppendPath(".txt"); // World.txt
        DirPath otherDirectory = (DirPath)@"C:\New";
        FilePath newFile = otherDirectory + relative; // C:\New\World.txt
        FilePath pdfFile = newFile.GetWithExtension("pdf"); // C:\New\World.pdf
        DirPath otherDir = otherDirectory.CombineDir("Strong", "Typed"); // C:\New\Strong\Typed
        FilePath otherFile = otherDir.CombineFile("Test.txt"); // C:\New\Strong\Typed\Test.txt

        //Exploring disk
        IEnumerable<FilePath> files = directory.GetFiles("*", SearchOption.AllDirectories);
        IEnumerable<DirPath> directories = directory.GetDirectories();
        var baseName = file.NameWithoutExtension;
        bool fileExists = file.Exists();
        bool dirExists = directory.Exists();

        //Operations on disk
        directory.CreateDirectory();
        file.DeleteFile();
        file.WriteAllText("Hello World");
        file.ClearReadOnlyAttribute();
        file.SetLastWriteTimeUtc(DateTime.UtcNow);
        directory.DeleteDir();
        file.Move(pdfFile);
        newFile.CopyTo(otherFile);
        var newTarget = newFile.CopyTo(otherDir); // otherDir + newFile.Name;
        otherDir.CopyDirectory(otherDirectory);

        //Use path as a string
        File.ReadAllBytes(file.Path);
    }

}

#endif
