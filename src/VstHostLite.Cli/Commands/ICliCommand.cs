namespace VstHostLite.Cli.Commands;

public interface ICliCommand
{
    string Name { get; }
    string Description { get; }
    int Run(string[] args);
}
