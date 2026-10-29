using MinOS.Core;
using MinOS.Gameplay;

namespace MinOS.Audio;

public sealed class AudioManager : IAudioManager
{
    public float SfxVolume { get; private set; } = 1f;
    public void PlayColor(ColorId color) { /* Engineer: map ColorId to nine SoundEffect assets; keep this non-authoritative. */ }
    public void SetSfxVolume(float volume) => SfxVolume = Math.Clamp(volume, 0f, 1f);
}
