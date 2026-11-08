namespace TreeCD.CommandLine;

/// <summary>
/// Parsed command-line options for <c>treecd</c>.
/// </summary>
public sealed record CommandLineOptions(
    string? StartPath,
    bool ShowHelp,
    bool ShowVersion)
{
    public static CommandLineOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string? startPath = null;
        var showHelp = false;
        var showVersion = false;

        foreach (var arg in args)
        {
            switch (arg)
            {
                case "-h":
                case "--help":
                    showHelp = true;
                    break;
                case "-v":
                case "--version":
                    showVersion = true;
                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        throw new ArgumentException($"Unknown option: {arg}", nameof(args));
                    }

                    if (startPath is not null)
                    {
                        throw new ArgumentException("Only one starting path may be provided.", nameof(args));
                    }

                    startPath = arg;
                    break;
            }
        }

        return new CommandLineOptions(startPath, showHelp, showVersion);
    }

    public static void WriteHelp(TextWriter writer)
    {
        writer.WriteLine("Usage: treecd [DIRECTORY]");
        writer.WriteLine();
        writer.WriteLine("Browse DIRECTORY (or the current directory) as an expandable tree.");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  -h, --help       Show this help text.");
        writer.WriteLine("  -v, --version    Show the version.");
        writer.WriteLine();
        writer.WriteLine("Keys:");
        writer.WriteLine("  Up/Down          Move the selection.");
        writer.WriteLine("  Right            Expand a directory.");
        writer.WriteLine("  Left             Collapse a directory.");
        writer.WriteLine("  Enter            Select the highlighted directory and exit.");
        writer.WriteLine("  Esc or Q         Cancel without changing directory.");
    }
}
