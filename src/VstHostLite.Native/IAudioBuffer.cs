public interface IAudioBuffer
{
    int Channels { get; }
    int Frames { get; }
    void Clear();
    void CopyFrom(AudioBuffer other);
    float[] ToFlatArray();
}