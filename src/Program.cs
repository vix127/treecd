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

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(static options =>
            {
                if (options.Help)
                {
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
            });

        return -1;
    }
}
