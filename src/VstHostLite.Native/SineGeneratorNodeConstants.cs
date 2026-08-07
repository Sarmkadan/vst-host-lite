internal static class SineGeneratorNodeConstants
{
    public const float DefaultFrequency = 440.0f; // Default: A4 (440 Hz)
    public const float DefaultAmplitude = 0.5f; // Default: -6dB
    public const float MinPhase = 0.0f; // Phase accumulator in radians
    public const float MaxPhase = (float)(Math.PI * 2.0f); // 2π for phase wrapping
}
