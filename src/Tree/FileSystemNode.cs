using System.Collections;
using TreeCD.FileSystem;

namespace TreeCD.Tree;

/// <summary>
/// A mutable node in the in-memory view of the file-system tree.
/// </summary>
public sealed class FileSystemNode
{
    private readonly List<FileSystemNode> _children = new();

    public FileSystemNode(
        string fullPath,
        bool isDirectory,
        bool isReparsePoint,
        FileSystemNode? parent = null)
    {
        FullPath = Path.GetFullPath(fullPath);
        IsDirectory = isDirectory;
        IsReparsePoint = isReparsePoint;
        Parent = parent;
        CanonicalPath = ComputeCanonicalPath(FullPath, isReparsePoint);
    }

    /// <summary>The entry name without its parent path.</summary>
    public string Name
    {
        get
        {
            var trimmed = FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.IsNullOrEmpty(trimmed) ? FullPath : Path.GetFileName(trimmed);
        }
    }

    public string FullPath { get; }

    public string CanonicalPath { get; }

    public bool IsDirectory { get; }

    public bool IsReparsePoint { get; }

    public FileSystemNode? Parent { get; }

    public bool IsExpanded { get; set; }

    public bool IsLoaded { get; set; }

    public bool HasError { get; set; }

    public string? ErrorMessage { get; set; }

    public IReadOnlyList<FileSystemNode> Children => _children;

    /// <summary>All ancestors from the parent up to the root.</summary>
    public IEnumerable<FileSystemNode> Ancestors
    {
        get
        {
            var current = Parent;
            while (current is not null)
            {
                yield return current;
                current = current.Parent;
            }
        }
    }

    internal void AddChild(FileSystemNode child) => _children.Add(child);

    internal void ClearChildren() => _children.Clear();

    internal void SortChildren()
    {
        var directories = _children
            .Where(static node => node.IsDirectory)
            .OrderBy(static node => node.Name, StringComparer.OrdinalIgnoreCase);
        var files = _children
            .Where(static node => !node.IsDirectory)
            .OrderBy(static node => node.Name, StringComparer.OrdinalIgnoreCase);

        var sortedDirectories = directories.ToList();
        var sortedFiles = files.ToList();
        _children.Clear();
        _children.AddRange(sortedDirectories);
        _children.AddRange(sortedFiles);
    }

    private static string ComputeCanonicalPath(string path, bool isReparsePoint)
    {
        if (!isReparsePoint)
        {
            return path;
        }

        try
        {
            var info = new DirectoryInfo(path);
            var target = info.ResolveLinkTarget(returnFinalTarget: true);
            if (target is not null)
            {
                return Path.GetFullPath(target.FullName);
            }
        }
        catch
        {
            // Fall back to the link path if resolution fails.
        }

        return path;
    }
}
