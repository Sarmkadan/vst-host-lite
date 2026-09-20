using System;
using VstHostLite.Native;

namespace VstHostLite.Cli.Commands;

public class InfoCommand : ICliCommand
{
    public string Name => "info";
    public string Description => "list plugin factory classes";

    public int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: vsthost info <path-to.vst3>");
            return 1;
        }

        string path = args[0];
        var result = NativeModule.TryLoad(path);
        if (!result.Success)
        {
            Console.Error.WriteLine($"error: {result.ErrorMessage}");
            return 1;
        }

        using var module = result.Module!;
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
}