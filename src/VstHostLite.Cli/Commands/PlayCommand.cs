namespace VstHostLite.Cli.Commands;

public class PlayCommand : ICliCommand
{
    public string Name => "play";
    public string Description => "(unfinished) stream audio through plugin";

    public int Run(string[] args)
    {
        Console.Error.WriteLine("`play` is not implemented - audio graph routing is unfinished.");
        Console.Error.WriteLine("See README.md (\"Where it stalled\").");
        return 2;
    }
}