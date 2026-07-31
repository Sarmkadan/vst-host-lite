using System;

namespace VstHostLite.Native
{
    public interface INoiseGeneratorNode
    {
        string Name { get; }
        void Process(float[] output);
    }
}
