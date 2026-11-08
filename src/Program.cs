using System.IO;
using Spectre.Console;
using TreeCD.CommandLine;
using TreeCD.FileSystem;
using TreeCD.Input;
using TreeCD.Tree;

namespace TreeCD;

public static class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding = new System.Text.UTF8Encoding(false);

        CommandLineOptions options;
        try
        {
            options = CommandLineOptions.Parse(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }

        if (options.ShowHelp)
        {
            CommandLineOptions.WriteHelp(Console.Out);
            return 0;
        }

        if (options.ShowVersion)
        {
            var version = typeof(Program).Assembly.GetName().Version;
            Console.WriteLine(version is null ? "treecd" : $"treecd {version}");
            return 0;
        }

        var startPath = options.StartPath ?? Directory.GetCurrentDirectory();
        try
        {
            startPath = Path.GetFullPath(startPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Invalid path: {ex.Message}");
            return 2;
        }

        if (!Directory.Exists(startPath))
        {
            Console.Error.WriteLine($"Directory does not exist: {startPath}");
            return 2;
        }

        IFileSystem fileSystem = new PhysicalFileSystem();
        string? selectedPath;
        try
        {
            selectedPath = TreeBrowser.Browse(startPath, fileSystem);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"treecd failed: {ex.Message}");
            return 2;
        }
        finally
        {
            try
            {
                Console.ResetColor();
            }
            catch
            {
                // Best effort only.
            }
        }

        if (selectedPath is null)
        {
            return 1;
        }

        try
        {
            Directory.SetCurrentDirectory(selectedPath);
            Environment.CurrentDirectory = selectedPath;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not change directory: {ex.Message}");
            return 2;
        }

        Console.WriteLine(selectedPath);
        return 0;
    }
}
