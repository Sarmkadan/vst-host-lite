public class AudioBuffer
    {
        private float[] buffer;
        public int Channels { get; private set; }
        public int Frames { get; private set; }

        public AudioBuffer(int channels, int frames)
        {
            Channels = channels;
            Frames = frames;
            buffer = new float[channels * frames];
        }

        public void Clear()
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }
        }

        public void CopyFrom(AudioBuffer other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (other.Channels != Channels || other.Frames != Frames)
                throw new ArgumentException("AudioBuffer dimensions do not match", nameof(other));

            Array.Copy(other.buffer, buffer, other.buffer.Length);
        }

        public float[] ToFlatArray()
        {
            return (float[])buffer.Clone();
        }

        public static AudioBuffer Interleave(AudioBuffer buffer1, AudioBuffer buffer2)
        {
            if (buffer1.Channels != buffer2.Channels)
                throw new ArgumentException("Channel counts do not match", nameof(buffer1));

            AudioBuffer result = new AudioBuffer(buffer1.Channels, buffer1.Frames + buffer2.Frames);
            int frameOffset = 0;
            for (int i = 0; i < buffer1.Frames; i++)
            {
                Array.Copy(buffer1.buffer, 0, result.buffer, frameOffset, buffer1.Channels);
                frameOffset += buffer1.Channels;
            }
            for (int i = 0; i < buffer2.Frames; i++)
            {
                Array.Copy(buffer2.buffer, 0, result.buffer, frameOffset, buffer2.Channels);
                frameOffset += buffer2.Channels;
            }
            return result;
        }

        public static AudioBuffer Deinterleave(AudioBuffer buffer)
        {
            if (buffer.Channels < 2)
                throw new ArgumentException("At least two channels required for deinterleaving", nameof(buffer));

            AudioBuffer result = new AudioBuffer(1, buffer.Channels * buffer.Frames);
            for (int i = 0; i < buffer.Frames; i++)
            {
                for (int c = 0; c < buffer.Channels; c++)
                {
                    result.buffer[i * buffer.Channels + c] = buffer.buffer[c * buffer.Frames + i];
                }
            }
            return result;
        }

        public float this[int channel, int frame]
        {
            get => buffer[frame * Channels + channel];
            set => buffer[frame * Channels + channel] = value;
        }
}