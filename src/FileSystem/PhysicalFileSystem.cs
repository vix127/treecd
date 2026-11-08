namespace TreeCD.FileSystem;

/// <summary>
/// <see cref="IFileSystem"/> implementation backed by the operating-system file system.
/// </summary>
public sealed class PhysicalFileSystem : IFileSystem
{
    public IReadOnlyList<FileSystemEntry> GetChildren(string directoryPath)
    {
        var entries = new List<FileSystemEntry>();

        foreach (var directory in Directory.EnumerateDirectories(directoryPath))
        {
            var info = new DirectoryInfo(directory);
            entries.Add(new FileSystemEntry(
                info.Name,
                info.FullName,
                IsDirectory: true,
                IsReparsePoint: info.Attributes.HasFlag(FileAttributes.ReparsePoint)));
        }

        foreach (var file in Directory.EnumerateFiles(directoryPath))
        {
            var info = new FileInfo(file);
            entries.Add(new FileSystemEntry(
                info.Name,
                info.FullName,
                IsDirectory: false,
                IsReparsePoint: info.Attributes.HasFlag(FileAttributes.ReparsePoint)));
        }

        return entries;
    }
}
