using Microsoft.Xna.Framework.Audio;
using MinOS.Core;
using MinOS.Gameplay;

namespace MinOS.Audio;

public sealed class AudioManager : IAudioManager
{
    private readonly SoundEffect[] tones;
    public float SfxVolume { get; private set; } = 1f;
    public AudioManager()
    {
        // QA sprint 2: temporary tones keep the nine color cues testable until the
        // product sound files arrive. Replacing this constructor with Content.Load
        // calls will not change gameplay or the IAudioManager contract.
        tones = Enumerable.Range(0, 9).Select(CreatePlaceholderTone).ToArray();
    }

    public void PlayColor(ColorId color)
    {
        if (SfxVolume <= 0f) return;
        tones[(int)color].Play(SfxVolume, 0f, 0f);
    }
    public void SetSfxVolume(float volume) => SfxVolume = Math.Clamp(volume, 0f, 1f);

    private static SoundEffect CreatePlaceholderTone(int index)
    {
        const int sampleRate = 22050;
        const int samples = sampleRate / 12;
        var pcm = new byte[samples * 2];
        var frequency = 220 + index * 45;
        for (var i = 0; i < samples; i++)
        {
            var value = (short)(Math.Sin(2 * Math.PI * frequency * i / sampleRate) * short.MaxValue * .18);
            pcm[i * 2] = (byte)value; pcm[i * 2 + 1] = (byte)(value >> 8);
        }
        return new SoundEffect(pcm, sampleRate, AudioChannels.Mono);
    }
}
