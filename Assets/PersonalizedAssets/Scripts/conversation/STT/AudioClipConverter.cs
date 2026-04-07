using UnityEngine;
using System.IO;

public static class AudioClipConverter
{
    public static byte[] ToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        int sampleCount = samples.Length;
        int byteCount = sampleCount * 2; // 16 bits por sample

        // Header WAV
        writer.Write(new char[] { 'R', 'I', 'F', 'F' });
        writer.Write(36 + byteCount);
        writer.Write(new char[] { 'W', 'A', 'V', 'E' });
        writer.Write(new char[] { 'f', 'm', 't', ' ' });
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)clip.channels);
        writer.Write(clip.frequency);
        writer.Write(clip.frequency * clip.channels * 2);
        writer.Write((short)(clip.channels * 2));
        writer.Write((short)16);
        writer.Write(new char[] { 'd', 'a', 't', 'a' });
        writer.Write(byteCount);

        // Audio data
        foreach (float sample in samples)
            writer.Write((short)(sample * 32767));

        return stream.ToArray();
    }
}