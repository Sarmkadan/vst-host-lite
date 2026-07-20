using VstHostLite.Native;

// vst-host-lite CLI
//
// Two subcommands work: `info` loads a .vst3 and lists its factory classes.
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

static void PrintUsage()
{
    Console.WriteLine("vst-host-lite - minimal VST3 host experiment");
    Console.WriteLine();
    Console.WriteLine("usage:");
    Console.WriteLine(" vsthost info <path-to.vst3> list plugin factory classes");
    Console.WriteLine(" vsthost play <path-to.vst3> (unfinished) stream audio through plugin");
}
