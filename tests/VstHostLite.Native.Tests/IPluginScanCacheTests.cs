public interface IPluginScanCacheTests
{
    void TryGetFresh_ReturnsFalse_WhenPluginDoesNotExist();
    void SaveAndTryGetFresh_RoundtripWorks();
    void TryGetFresh_ReturnsFalse_WhenCacheIsStale();
    void Clear_RemovesCacheFile();
    void ClearAll_RemovesAllCacheFiles();
}
