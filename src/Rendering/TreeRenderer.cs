using Spectre.Console;
using TreeCD.Navigation;
using TreeCD.Tree;

namespace TreeCD.Rendering;

/// <summary>
/// Renders the current navigation state to the console.
/// </summary>
public static class TreeRenderer
{
    public static void Render(TreeNavigator navigator, IAnsiConsole? console = null)
    {
        console ??= AnsiConsole.Console;
        try { console.Clear(); } catch { }

        if (!SafeConsole(out var windowWidth, out var windowHeight))
        {
            windowWidth = 120;
            windowHeight = 30;
        }

        console.MarkupLine($"[bold]treecd[/]  {Markup.Escape(navigator.Root.FullPath)}");
        console.MarkupLine("[grey]Up/Down move  Right expand  Left collapse  Enter select  Esc/Q quit[/]");

        if (!string.IsNullOrEmpty(navigator.StatusMessage))
        {
            console.MarkupLine($"[yellow]{Markup.Escape(navigator.StatusMessage)}[/]");
        }

        var visible = navigator.VisibleNodes;
        if (visible.Count == 0)
        {
            console.MarkupLine("[grey](empty directory)[/]");
            return;
        }

        const int headerLines = 4;
        var pageSize = Math.Max(5, windowHeight - headerLines - 1);
        var startIndex = 0;
        if (navigator.SelectedIndex >= pageSize)
        {
            startIndex = navigator.SelectedIndex - (pageSize / 2);
        }

        startIndex = Math.Clamp(startIndex, 0, Math.Max(0, visible.Count - pageSize));
        var endIndex = Math.Min(visible.Count, startIndex + pageSize);

        for (var i = startIndex; i < endIndex; i++)
        {
            var node = visible[i];
            var depth = GetDepth(node, navigator.Root);
            var indent = new string(' ', depth * 2);
            var marker = node.IsDirectory
                ? node.IsExpanded ? "▾" : "▸"
                : " ";

            var name = Markup.Escape(node.Name);
            if (node.IsDirectory)
            {
                name = $"[blue]{name}[/]";
            }
            else
            {
                name = $"[grey]{name}[/]";
            }

            if (node.HasError)
            {
                name += " [red](unreadable)[/]";
            }

            var line = $"{indent}{marker} {name}";
            if (i == navigator.SelectedIndex)
            {
                // Pad and highlight the selected row.
                console.MarkupLine($"[black on white]{line}[/]");
            }
            else
            {
                console.MarkupLine(line);
            }
        }
    }

    private static int GetDepth(FileSystemNode node, FileSystemNode root)
    {
        var depth = 0;
        var current = node.Parent;
        while (current is not null && current != root)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    private static bool SafeConsole(out int width, out int height)
    {
        try
        {
            width = Console.WindowWidth;
            height = Console.WindowHeight;
            return width > 0 && height > 0;
        }
        catch
        {
            width = 0;
            height = 0;
            return false;
        }
    }
}
