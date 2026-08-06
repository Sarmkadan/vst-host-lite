internal static class DelayNodeConstants
{
    public const int DefaultMaxDelayTimeMs = 44100 / 4;
    public const int DefaultFrames = 1;
    public const float DefaultFeedback = 0.5f;
    public const float DefaultDryWetMix = 0.5f;
    public const int MinDelaySamples = 1;
    public const int MaxDelaySamples = DefaultMaxDelayTimeMs;
}
