using CommandLine;

namespace TreeCD.CommandLine;

/// <summary>
/// Parsed command-line options for <c>treecd</c>.
/// </summary>
public class Options
{
    [Value(0, Default = null, HelpText = "The starting directory.", Required = false)]
    public string? StartPath { get; set; }

    [Option('h', "help", Required = false, HelpText = "Show this help text.", Default = false)]
    public bool Help { get; set; }

    [Option('v', "version", Required = false, HelpText = "Show the treecd version.", Default = false)]
    public bool Version { get; set; }
}