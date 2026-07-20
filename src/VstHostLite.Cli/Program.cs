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

    default:
        PrintUsage();
        return 1;
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
    Console.WriteLine(" vsthost play <path-to.vst3>     (unfinished) stream audio through plugin");
}
