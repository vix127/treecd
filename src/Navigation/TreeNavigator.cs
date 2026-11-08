using System.Collections;
using TreeCD.FileSystem;
using TreeCD.Tree;

namespace TreeCD.Navigation;

/// <summary>
/// Mutable navigation state for an interactive file-system tree.
/// </summary>
public sealed class TreeNavigator
{
    private readonly IFileSystem _fileSystem;
    private readonly List<FileSystemNode> _visible = new();
    private int _selectedIndex;

    public TreeNavigator(string rootPath, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        Root = new FileSystemNode(rootPath, isDirectory: true, isReparsePoint: false);
        Root.IsExpanded = true;
        Root.IsLoaded = true;

        LoadChildren(Root);
        RebuildVisible();
        _selectedIndex = _visible.Count > 0 ? 0 : -1;
    }

    public FileSystemNode Root { get; }

    public IReadOnlyList<FileSystemNode> VisibleNodes => _visible;

    public FileSystemNode? SelectedNode =>
        _selectedIndex >= 0 && _selectedIndex < _visible.Count
            ? _visible[_selectedIndex]
            : null;

    public int SelectedIndex => _selectedIndex;

    public string? SelectedPath { get; private set; }

    public bool IsCompleted { get; private set; }

    public bool IsCancelled { get; private set; }

    public string StatusMessage { get; private set; } = string.Empty;

    public void MoveUp()
    {
        if (_visible.Count == 0)
        {
            return;
        }

        _selectedIndex = Math.Max(0, _selectedIndex - 1);
    }

    public void MoveDown()
    {
        if (_visible.Count == 0)
        {
            return;
        }

        _selectedIndex = Math.Min(_visible.Count - 1, _selectedIndex + 1);
    }

    /// <summary>Expands the selected directory, loading its children on first expansion.</summary>
    public void ExpandSelected()
    {
        var node = SelectedNode;
        if (node is null || !node.IsDirectory)
        {
            return;
        }

        if (node.IsReparsePoint)
        {
            StatusMessage = "Reparse points are not expanded to avoid cycles.";
            return;
        }

        if (node.IsExpanded)
        {
            return;
        }

        if (IsCyclic(node))
        {
            StatusMessage = "Skipped cyclic directory.";
            return;
        }

        if (!node.IsLoaded)
        {
            LoadChildren(node);
        }

        if (!node.HasError)
        {
            node.IsExpanded = true;
            StatusMessage = string.Empty;
        }

        RebuildVisible();
        EnsureSelectionValid();
    }

    /// <summary>
    /// Collapses the selected directory, or moves to its parent when it is already collapsed.
    /// </summary>
    public void CollapseSelected()
    {
        var node = SelectedNode;
        if (node is null)
        {
            return;
        }

        if (node.IsDirectory && node.IsExpanded)
        {
            node.IsExpanded = false;
            StatusMessage = string.Empty;
            RebuildVisible();
            EnsureSelectionValid();
            return;
        }

        if (node.Parent is { } parent && parent != Root)
        {
            var parentIndex = _visible.IndexOf(parent);
            if (parentIndex >= 0)
            {
                _selectedIndex = parentIndex;
                StatusMessage = string.Empty;
            }
        }
    }

    /// <summary>Tries to complete the browser with the selected directory.</summary>
    public bool TryCompleteSelection()
    {
        var node = SelectedNode;
        if (node is null)
        {
            SelectedPath = Root.FullPath;
            IsCompleted = true;
            return true;
        }

        if (!node.IsDirectory)
        {
            StatusMessage = "Select a directory before pressing Enter.";
            return false;
        }

        SelectedPath = node.FullPath;
        IsCompleted = true;
        return true;
    }

    public void Cancel()
    {
        IsCancelled = true;
    }

    private void LoadChildren(FileSystemNode node)
    {
        node.ClearChildren();
        node.HasError = false;
        node.ErrorMessage = null;

        try
        {
            foreach (var entry in _fileSystem.GetChildren(node.FullPath))
            {
                var child = new FileSystemNode(
                    entry.FullPath,
                    entry.IsDirectory,
                    entry.IsReparsePoint,
                    parent: node);
                node.AddChild(child);
            }

            node.SortChildren();
            node.IsLoaded = true;
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            node.HasError = true;
            node.ErrorMessage = ex.Message;
            node.IsLoaded = true;
        }
    }

    private bool IsCyclic(FileSystemNode node)
    {
        return node.Ancestors.Any(ancestor =>
            string.Equals(ancestor.CanonicalPath, node.CanonicalPath, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRecoverable(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or System.Security.SecurityException
            or ArgumentException;

    private void RebuildVisible()
    {
        _visible.Clear();
        if (Root.IsExpanded)
        {
            foreach (var child in Root.Children)
            {
                CollectVisible(child, _visible);
            }
        }
    }

    private static void CollectVisible(FileSystemNode node, List<FileSystemNode> visible)
    {
        visible.Add(node);
        if (node.IsDirectory && node.IsExpanded)
        {
            foreach (var child in node.Children)
            {
                CollectVisible(child, visible);
            }
        }
    }

    private void EnsureSelectionValid()
    {
        if (_visible.Count == 0)
        {
            _selectedIndex = -1;
            return;
        }

        _selectedIndex = Math.Clamp(_selectedIndex, 0, _visible.Count - 1);
    }
}
