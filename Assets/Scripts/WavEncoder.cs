using System.IO;
using UnityEngine;

public static class WavEncoder
{
    public static byte[] Encode(AudioClip clip)
    {
        float[] floatSamples = new float[clip.samples * clip.channels];
        clip.GetData(floatSamples, 0);

        // Mix to mono
        short[] pcm = new short[clip.samples];
        for (int i = 0; i < clip.samples; i++)
        {
            float avg = 0f;
            for (int c = 0; c < clip.channels; c++)
                avg += floatSamples[i * clip.channels + c];
            avg /= clip.channels;
            pcm[i] = (short)(Mathf.Clamp(avg, -1f, 1f) * 32767f);
        }

        int sampleRate   = clip.frequency;
        int dataSize     = pcm.Length * 2;

        using var ms     = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write(new[] { 'R', 'I', 'F', 'F' });
        writer.Write(36 + dataSize);
        writer.Write(new[] { 'W', 'A', 'V', 'E' });
        writer.Write(new[] { 'f', 'm', 't', ' ' });
        writer.Write(16);
        writer.Write((short)1);           // PCM
        writer.Write((short)1);           // mono
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2);     // byte rate
        writer.Write((short)2);           // block align
        writer.Write((short)16);          // bits per sample
        writer.Write(new[] { 'd', 'a', 't', 'a' });
        writer.Write(dataSize);
        foreach (var s in pcm) writer.Write(s);

        return ms.ToArray();
    }
}
