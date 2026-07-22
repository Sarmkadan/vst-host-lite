using System;
using System.IO;
using System.Text.Json;
using VstHostLite.Native;

namespace VstHostLite.Cli.Commands;

public class GraphCommand : ICliCommand
{
    public string Name => "graph";
    public string Description => "display audio graph structure";

    public int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: vsthost graph <path-to-graph.json>");
            return 1;
        }

        string jsonPath = args[0];
        try
        {
            string json = File.ReadAllText(jsonPath);
            var graph = AudioGraphJsonExtensions.FromJson(json);

            Console.WriteLine("Audio Graph:");
            Console.WriteLine($" Nodes: {graph.Nodes.Count}");

            var nodesInOrder = graph.GetNodesInOrder().ToList();

            if (nodesInOrder.Count == 0)
            {
                Console.WriteLine(" (no nodes in graph)");
                return 0;
            }

            Console.WriteLine();
            Console.WriteLine("Nodes:");

            for (int i = 0; i < nodesInOrder.Count; i++)
            {
                var node = nodesInOrder[i];
                Console.WriteLine($" [{i}] {node.Name} (component: 0x{node.Component.ToInt64():X})");

                if (node.Prev != null)
                {
                    Console.WriteLine($" ← connected to: {node.Prev.Name}");
                }
                if (node.Next != null)
                {
                    Console.WriteLine($" → connected to: {node.Next.Name}");
                }
                else
                {
                    Console.WriteLine(" → (end of chain)");
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
}