using System;

public static class SampleRateConverter
{
    public static float[] Convert(float[] input, int inputRate, int outputRate)
    {
        if (input.Length == 0)
        {
            return new float[0];
        }

        if (inputRate == outputRate)
        {
            return input;
        }

        int outputLength = (int)Math.Round(input.Length * (double)outputRate / inputRate);
        float[] output = new float[outputLength];

        for (int i = 0; i < outputLength; i++)
        {
            double position = (double)i * inputRate / outputRate;
            int index = (int)position;
            double fraction = position - index;

            if (index >= input.Length - 1)
            {
                output[i] = input[input.Length - 1];
            }
            else
            {
                output[i] = input[index] + (float)fraction * (input[index + 1] - input[index]);
            }
        }

        return output;
    }
}
