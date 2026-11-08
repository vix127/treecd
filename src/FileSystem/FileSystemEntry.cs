namespace TreeCD.FileSystem;

/// <summary>
/// Describes a single file-system entry discovered while browsing a directory.
/// </summary>
/// <param name="Name">The entry name without its parent path.</param>
/// <param name="FullPath">The absolute path to the entry.</param>
/// <param name="IsDirectory">Whether the entry is a directory.</param>
/// <param name="IsReparsePoint">
/// Whether the entry is a reparse point (symbolic link, junction, mount point, ...).
/// </param>
public sealed record FileSystemEntry
(
    string Name,
    string FullPath,
    bool IsDirectory,
    bool IsReparsePoint
);
