using CommandLine;
using Spectre.Console;
using System.IO;
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

        new Parser(settings =>
        {
            settings.AutoHelp = false;
            settings.AutoVersion = false;
        }).ParseArguments<Options>(args)
            .WithParsed(static options =>
            {
                if (options.Help)
                {
                    WriteHelp();
                    Environment.Exit(0);
                }

                if (options.Version)
                {
                    var version = typeof(Program).Assembly.GetName().Version;
                    Console.WriteLine(version is null ? "treecd" : $"treecd {version}");
                    Environment.Exit(0);
                }

                var startPath = options.StartPath ?? Directory.GetCurrentDirectory();

                try
                {
                    startPath = Path.GetFullPath(startPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Invalid path: {ex.Message}");
                    Environment.Exit(2);
                }

                if (!Directory.Exists(startPath))
                {
                    Console.Error.WriteLine($"Directory does not exist: {startPath}");
                    Environment.Exit(2);
                }

                IFileSystem fileSystem = new PhysicalFileSystem();
                string? selectedPath = null;
                try
                {
                    selectedPath = TreeBrowser.Browse(startPath, fileSystem);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"treecd failed: {ex.Message}");
                    Environment.Exit(2);
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
                    Environment.Exit(1);
                }

                try
                {
                    Directory.SetCurrentDirectory(selectedPath);
                    Environment.CurrentDirectory = selectedPath;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Could not change directory: {ex.Message}");
                    Environment.Exit(2);
                }

                Console.WriteLine(selectedPath);
                Environment.Exit(0);
            })
            .WithNotParsed(_ => Environment.Exit(2));

        return -1;
    }

    private const string HelpText = """
        Usage: treecd [DIRECTORY]

        Browse DIRECTORY (or the current directory) as an expandable tree.

        Options:
          -h, --help       Show this help text.
          -v, --version    Show the version.

        Keys:
          Up/Down          Move the selection.
          Right            Expand a directory.
          Left             Collapse a directory.
          Enter            Select the highlighted directory and exit.
          Esc or Q         Cancel without changing directory.
        """;

    private static void WriteHelp() => Console.WriteLine(HelpText);
}
