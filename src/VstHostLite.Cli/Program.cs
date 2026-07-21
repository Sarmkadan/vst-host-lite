using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VstHostLite.Native;

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
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        switch (args[0])
        {
            case "info":
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("usage: vsthost info <path-to.vst3>");
                    return 1;
                }
                return Info(args[1]);

    case "play":
        Console.Error.WriteLine("`play` is not implemented - audio graph routing is unfinished.");
        Console.Error.WriteLine("See README.md (\"Where it stalled\").");
        return 2;

    case "validate":
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: vsthost validate <path-to-graph.json>");
            return 1;
        }
        return Validate(args[1]);

case "graph":
	if (args.Length < 2)
	{
		Console.Error.WriteLine("usage: vsthost graph <path-to-graph.json>");
		return 1;
	}
	return Graph(args[1]);

        case "stats":
            if (args.Length < 2)
            {
                Console.Error.WriteLine("usage: vsthost stats <path-to-graph.json>");
                return 1;
            }
            return Stats(args[1]);

        case "scan":
            if (args.Length < 2)
            {
                Console.Error.WriteLine("usage: vsthost scan <path-to-directory> [--filter <substring>] [--category <category>]");
                return 1;
            }

            // Parse arguments
            string path = args[1];
            string? filter = null;
            string? category = null;

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--filter" && i + 1 < args.Length)
                {
                    filter = args[++i];
                }
                else if (args[i] == "--category" && i + 1 < args.Length)
                {
                    category = args[++i];
                }
                else
                {
                    Console.Error.WriteLine($"unknown argument: {args[i]}");
                    return 1;
                }
            }

            return Scan(path, filter, category);

    default:
        PrintUsage();
        return 1;
}


static int Scan(string path, string? filter, string? category)
{
    try
    {
        if (!Directory.Exists(path))
        {
            Console.Error.WriteLine($"error: directory not found: {path}");
            return 1;
        }

        var pluginFiles = Directory.EnumerateFiles(path, "*.vst3", SearchOption.AllDirectories).ToList();
        if (pluginFiles.Count == 0)
        {
            Console.WriteLine("No .vst3 files found.");
            return 0;
        }

        Console.WriteLine($"Scanning {pluginFiles.Count} .vst3 file(s) in {path}");
        Console.WriteLine();

        int totalClasses = 0;
        int matchingClasses = 0;

        foreach (var pluginPath in pluginFiles)
        {
            try
            {
                using var module = NativeModule.Load(pluginPath);
                var infos = module.ScanPluginClasses();
                totalClasses += infos.Count;

                // Apply filters
                var filteredInfos = ApplyFilters(infos, filter, category);
                if (filteredInfos.Count > 0)
                {
                    matchingClasses += filteredInfos.Count;
                    Console.WriteLine($"Plugin: {Path.GetFileName(pluginPath)}");
                    Console.WriteLine($"  Path: {pluginPath}");
                    Console.WriteLine($"  Classes: {filteredInfos.Count}");

                    for (var i = 0; i < filteredInfos.Count; i++)
                    {
                        var info = filteredInfos[i];
                        Console.WriteLine($"   [{i}] {info.Name} ({info.Category}) cid={info.Cid}");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"error scanning {pluginPath}: {ex.Message}");
            }
        }

        Console.WriteLine($"Summary: {matchingClasses}/{totalClasses} classes matched filters");
        return 0;
    }
    catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
    {
        Console.Error.WriteLine($"error: file not found: {path}");
        return 1;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        return 1;
    }
}

static List<PluginClassInfo> ApplyFilters(List<PluginClassInfo> infos, string? filter, string? category)
{
    if (filter == null && category == null)
    {
        return infos;
    }

    var result = new List<PluginClassInfo>();

    foreach (var info in infos)
    {
        bool matches = true;

        if (filter != null)
        {
            // Case-insensitive substring match on name
            matches &= info.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        if (category != null)
        {
            // Exact case-insensitive match on category
            matches &= string.Equals(info.Category, category, StringComparison.OrdinalIgnoreCase);
        }

        if (matches)
        {
            result.Add(info);
        }
    }

    return result;
}
static int Info(string path)
{
    try
    {
        using var module = NativeModule.Load(path);
        var infos = module.ScanPluginClasses();

        Console.WriteLine($"module : {module.Path}");
        Console.WriteLine($"classes: {infos.Count}");
        for (var i = 0; i < infos.Count; i++)
        {
            var info = infos[i];
            Console.WriteLine($" [{i}] {info.Name} ({info.Category}) cid={info.Cid}");
        }
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        return 1;
    }
}

static int Validate(string jsonPath)
{
    try
    {
        string json = File.ReadAllText(jsonPath);
        var graph = AudioGraphJsonExtensions.FromJson(json);
        
        var problems = graph.Validate();
        
        if (problems.Count == 0)
        {
            Console.WriteLine("Graph is valid.");
            return 0;
        }
        else
        {
            Console.Error.WriteLine($"Graph validation failed with {problems.Count} problem(s):");
            for (int i = 0; i < problems.Count; i++)
            {
                Console.Error.WriteLine($"[{i + 1}] {problems[i]}");
            }
            return 1;
        }
    }
    catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
    {
        Console.Error.WriteLine($"error: file not found: {jsonPath}");
        return 1;
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"error: invalid JSON: {ex.Message}");
        return 1;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        return 1;
    }
}

static int Graph(string jsonPath)
{
    try
    {
        string json = File.ReadAllText(jsonPath);
        var graph = AudioGraphJsonExtensions.FromJson(json);

        Console.WriteLine("Audio Graph:");
        Console.WriteLine($"  Nodes: {graph.Nodes.Count}");

        var nodesInOrder = graph.GetNodesInOrder().ToList();

        if (nodesInOrder.Count == 0)
        {
            Console.WriteLine("  (no nodes in graph)");
            return 0;
        }

        Console.WriteLine();
        Console.WriteLine("Nodes:");

        for (int i = 0; i < nodesInOrder.Count; i++)
        {
            var node = nodesInOrder[i];
            Console.WriteLine($"  [{i}] {node.Name} (component: 0x{node.Component.ToInt64():X})");

            if (node.Prev != null)
            {
                Console.WriteLine($"      ← connected to: {node.Prev.Name}");
            }
            if (node.Next != null)
            {
                Console.WriteLine($"      → connected to: {node.Next.Name}");
            }
            else
            {
                Console.WriteLine("      → (end of chain)");
            }
        }

        return 0;
    }
    catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
    {
        Console.Error.WriteLine($"error: file not found: {jsonPath}");
        return 1;
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"error: invalid JSON: {ex.Message}");
        return 1;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        return 1;
    }
}

static void PrintUsage()
{
    Console.WriteLine("vst-host-lite - minimal VST3 host experiment");
    Console.WriteLine();
    Console.WriteLine("usage:");
    Console.WriteLine(" vsthost info <path-to.vst3>     list plugin factory classes");
    Console.WriteLine(" vsthost validate <path-to-graph.json> validate audio graph");
    Console.WriteLine(" vsthost graph <path-to-graph.json> display audio graph structure");
    Console.WriteLine(" vsthost stats <path-to-graph.json> print audio graph statistics");
    Console.WriteLine(" vsthost play <path-to.vst3>     (unfinished) stream audio through plugin");
Console.WriteLine(" vsthost scan <path-to-directory> [--filter <substring>] [--category <category>] recursively scan directory for plugins");
}

static int Stats(string jsonPath)
{
    try
    {
        string json = File.ReadAllText(jsonPath);
        var graph = AudioGraphJsonExtensions.FromJson(json);

        var nodes = graph.Nodes;
        var nodesInOrder = graph.GetNodesInOrder().ToList();

        // Count edges
        int edgeCount = 0;
        int generatorCount = 0; // nodes with no Prev (start of chains)
        int sinkCount = 0;     // nodes with no Next (end of chains)
        
        foreach (var node in nodes)
        {
            if (node.Prev == null) generatorCount++;
            if (node.Next == null) sinkCount++;
            if (node.Prev != null || node.Next != null) edgeCount++;
        }

        // Calculate max depth using topological order
        int maxDepth = 0;
        var depthMap = new Dictionary<GraphNode, int>();
        
        foreach (var node in nodesInOrder)
        {
            int depth = 0;
            var current = node;
            while (current != null)
            {
                if (depthMap.TryGetValue(current, out int existingDepth))
                {
                    depth = existingDepth + 1;
                    break;
                }
                current = current.Prev;
            }
            
            depthMap[node] = depth;
            if (depth > maxDepth) maxDepth = depth;
        }

        // Validate graph
        var problems = graph.Validate();

        // Print statistics
        Console.WriteLine("Audio Graph Statistics:");
        Console.WriteLine($"  Node count: {nodes.Count}");
        Console.WriteLine($"  Edge count: {edgeCount}");
        Console.WriteLine($"  Max depth: {maxDepth}");
        Console.WriteLine($"  Generator count: {generatorCount}");
        Console.WriteLine($"  Sink count: {sinkCount}");
        Console.WriteLine($"  Detected issues: {problems.Count}");
        
        if (problems.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Validation issues:");
            for (int i = 0; i < problems.Count; i++)
            {
                Console.WriteLine($"  [{i + 1}] {problems[i]}");
            }
        }

        return problems.Count == 0 ? 0 : 1;
    }
    catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
    {
        Console.Error.WriteLine($"error: file not found: {jsonPath}");
        return 1;
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"error: invalid JSON: {ex.Message}");
        return 1;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        return 1;
    }
}

    }
}
