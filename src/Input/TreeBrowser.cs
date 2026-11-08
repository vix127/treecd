using Spectre.Console;
using TreeCD.Navigation;
using TreeCD.Rendering;

namespace TreeCD.Input;

/// <summary>
/// Runs the interactive browse loop and returns the selected directory path.
/// </summary>
public static class TreeBrowser
{
    private static readonly IAnsiConsole Console = AnsiConsole.Create(
        new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(System.Console.Error)
        });

    public static string? Browse(string startPath, TreeCD.FileSystem.IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(startPath);
        ArgumentNullException.ThrowIfNull(fileSystem);

        if (System.Console.IsInputRedirected)
        {
            return null;
        }

        var navigator = new TreeNavigator(startPath, fileSystem);

        while (!navigator.IsCompleted && !navigator.IsCancelled)
        {
            try
            {
                TreeRenderer.Render(navigator, Console);
            }
            catch
            {
                navigator.Cancel();
                break;
            }

            ConsoleKeyInfo key;
            try
            {
                key = System.Console.ReadKey(intercept: true);
            }
            catch (InvalidOperationException)
            {
                navigator.Cancel();
                break;
            }

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    navigator.MoveUp();
                    break;
                case ConsoleKey.DownArrow:
                    navigator.MoveDown();
                    break;
                case ConsoleKey.RightArrow:
                    navigator.ExpandSelected();
                    break;
                case ConsoleKey.LeftArrow:
                    navigator.CollapseSelected();
                    break;
                case ConsoleKey.Enter:
                    navigator.TryCompleteSelection();
                    break;
                case ConsoleKey.Escape:
                case ConsoleKey.Q:
                    navigator.Cancel();
                    break;
            }
        }

        try
        {
            Console.Clear();
        }
        catch
        {
            // Ignore terminals that cannot be cleared.
        }

        return navigator.SelectedPath;
    }
}
