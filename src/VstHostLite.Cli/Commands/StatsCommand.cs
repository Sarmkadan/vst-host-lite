using System;
using System.IO;
using System.Text.Json;
using VstHostLite.Native;

namespace VstHostLite.Cli.Commands;

public class StatsCommand : ICliCommand
{
    public string Name => "stats";
    public string Description => "print audio graph statistics";

    public int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: vsthost stats <path-to-graph.json>");
            return 1;
        }

        string jsonPath = args[0];
        try
        {
            string json = File.ReadAllText(jsonPath);
            var graph = AudioGraphJsonExtensions.FromJson(json);

            var nodes = graph.Nodes;
            var nodesInOrder = graph.GetNodesInOrder().ToList();

            // Count edges
            int edgeCount = 0;
            int generatorCount = 0; // nodes with no Prev (start of chains)
            int sinkCount = 0; // nodes with no Next (end of chains)

            foreach (var node in nodes)
            {
                if (node.Prev == null) generatorCount++;
                if (node.Next == null) sinkCount++;
                if (node.Prev != null || node.Next != null) edgeCount++;
            }

            // Calculate max depth using topological order
            int maxDepth = 0;
            var depthMap = new System.Collections.Generic.Dictionary<VstHostLite.Native.GraphNode, int>();

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
            Console.WriteLine($" Node count: {nodes.Count}");
            Console.WriteLine($" Edge count: {edgeCount}");
            Console.WriteLine($" Max depth: {maxDepth}");
            Console.WriteLine($" Generator count: {generatorCount}");
            Console.WriteLine($" Sink count: {sinkCount}");
            Console.WriteLine($" Detected issues: {problems.Count}");

            if (problems.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Validation issues:");
                for (int i = 0; i < problems.Count; i++)
                {
                    Console.WriteLine($" [{i + 1}] {problems[i]}");
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