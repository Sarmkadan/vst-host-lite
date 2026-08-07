namespace VstHostLite.Native;

internal static class PluginScanCacheConstants
{
    public const string ScanCommandArgument = "scan-one";
    public const int ProcessTimeoutMilliseconds = 10000;
    public const int SuccessfulExitCode = 0;
    public const string PluginPathEmptyOrWhitespaceErrorMessage = "Plugin path cannot be empty or whitespace.";
    public const string CouldNotDetermineProcessPathErrorMessage = "Could not determine current process path.";
}