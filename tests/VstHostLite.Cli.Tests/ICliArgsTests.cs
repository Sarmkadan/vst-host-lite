namespace VstHostLite.Cli.Tests;

public interface ICliArgsTests
{
    void NoArguments_PrintsUsageAndReturns1();
    void UnknownCommand_PrintsUsageAndReturns1();
    void InfoCommand_MissingPath_Returns1();
    void ValidateCommand_MissingPath_Returns1();
    void GraphCommand_MissingPath_Returns1();
    void StatsCommand_MissingPath_Returns1();
    void PlayCommand_Returns2WithMessage();
    void InfoCommand_WithPath_ShowsUsageMessage();
    void ValidateCommand_WithPath_ShowsFileNotFound();
    void GraphCommand_WithPath_ShowsFileNotFound();
    void StatsCommand_WithPath_ShowsFileNotFound();
}
