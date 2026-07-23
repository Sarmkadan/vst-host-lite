using System;
using System.Text.Json;
using VstHostLite.Native;

namespace VstHostLite.Cli.Commands;

public class ScanOneCommand : ICliCommand
{
    public string Name => "scan-one";
    public string Description => "scan a single plugin (used internally for process isolation)";

    public int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: vsthost scan-one <path-to.vst3>");
            return 1;
        }

        string path = args[0];
        try
        {
            using var module = NativeModule.Load(path);
            var infos = module.ScanPluginClasses();

            // Output as JSON for the parent process to read
            var json = JsonSerializer.Serialize(infos, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Console.WriteLine(json);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }
}
