using System;
using System.IO;
using VstHostLite.Native;

namespace VstHostLite.Cli.Commands;

public class ScanCommand : ICliCommand
{
    public string Name => "scan";
    public string Description => "recursively scan directory for plugins";

    public int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: vsthost scan <path-to-directory> [--filter <substring>] [--category <category>]");
            return 1;
        }

        // Parse arguments
        string path = args[0];
        string? filter = null;
        string? category = null;

        for (int i = 1; i < args.Length; i++)
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
    }

    private int Scan(string path, string? filter, string? category)
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
                        Console.WriteLine($" Path: {pluginPath}");
                        Console.WriteLine($" Classes: {filteredInfos.Count}");

                        for (var i = 0; i < filteredInfos.Count; i++)
                        {
                            var info = filteredInfos[i];
                            Console.WriteLine($" [{i}] {info.Name} ({info.Category}) cid={info.Cid}");
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

    private System.Collections.Generic.List<PluginClassInfo> ApplyFilters(System.Collections.Generic.List<PluginClassInfo> infos, string? filter, string? category)
    {
        if (filter == null && category == null)
        {
            return infos;
        }

        var result = new System.Collections.Generic.List<PluginClassInfo>();

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
}