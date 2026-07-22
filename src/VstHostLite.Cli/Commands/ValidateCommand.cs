using System;
using System.IO;
using System.Text.Json;
using VstHostLite.Native;

namespace VstHostLite.Cli.Commands;

public class ValidateCommand : ICliCommand
{
    public string Name => "validate";
    public string Description => "validate audio graph";

    public int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: vsthost validate <path-to-graph.json>");
            return 1;
        }

        string jsonPath = args[0];
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
}