using System.Collections.Generic;
using VstHostLite.Cli.Commands;

// vst-host-lite CLI
//
// Four subcommands work: `info` loads a .vst3 and lists its factory classes.
// `validate` loads a graph JSON file and runs validation checks.
// `graph` loads a graph JSON file and displays nodes with their connections.
// `play` is supposed to build the audio graph and stream through the plugin -
// it does not work (audio graph routing is unfinished). It stays here so the
// wiring is visible for whoever picks this up.

public static class Program
{
    private static readonly Dictionary<string, ICliCommand> _commands = CreateCommands();

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        string commandName = args[0];

        if (!_commands.TryGetValue(commandName, out var command))
        {
            PrintUsage();
            return 1;
        }

        // Pass remaining arguments (skip the command name)
        return command.Run(args.Skip(1).ToArray());
    }

    private static Dictionary<string, ICliCommand> CreateCommands()
    {
        var commands = new Dictionary<string, ICliCommand>(StringComparer.Ordinal)
        {
            { "info", new InfoCommand() },
            { "validate", new ValidateCommand() },
            { "graph", new GraphCommand() },
            { "stats", new StatsCommand() },
            { "scan", new ScanCommand() },
            { "play", new PlayCommand() },
            { "scan-one", new ScanOneCommand() }
        };
        return commands;
    }

    static void PrintUsage()
    {
        Console.WriteLine("vst-host-lite - minimal VST3 host experiment");
        Console.WriteLine();
        Console.WriteLine("usage:");
        Console.WriteLine(" vsthost info <path-to.vst3> list plugin factory classes");
        Console.WriteLine(" vsthost validate <path-to-graph.json> validate audio graph");
        Console.WriteLine(" vsthost graph <path-to-graph.json> display audio graph structure");
        Console.WriteLine(" vsthost stats <path-to-graph.json> print audio graph statistics");
        Console.WriteLine(" vsthost play <path-to.vst3> (unfinished) stream audio through plugin");
        Console.WriteLine(" vsthost scan <path-to-directory> [--filter <substring>] [--category <category>] recursively scan directory for plugins");

        Console.WriteLine();
        Console.WriteLine("Available commands:");
        foreach (var cmd in _commands.Values)
        {
            Console.WriteLine($" {cmd.Name,-10} - {cmd.Description}");
        }
    }
}
