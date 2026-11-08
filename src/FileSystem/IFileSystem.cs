namespace TreeCD.FileSystem;

/// <summary>
/// Abstraction over directory enumeration, allowing the browser to be tested
/// without touching the real file system.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Enumerates the immediate children of <paramref name="directoryPath"/>.
    /// Implementations may throw <see cref="IOException"/> or
    /// <see cref="UnauthorizedAccessException"/> when the directory cannot be read.
    /// </summary>
    IReadOnlyList<FileSystemEntry> GetChildren(string directoryPath);
}
