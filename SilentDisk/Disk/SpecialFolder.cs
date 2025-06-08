namespace SilentOrbit.Disk;

public class SpecialFolder
{

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETSTANDARD

    /// <summary>
    /// C:\Users\[Username]
    /// </summary>
    public static DirPath ProfileDir { get; } = new DirPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

#endif

}
