using MinOS.Core;

namespace MinOS.Audio;

/// <summary>Deterministic presentation-only sequence used for memory audio and tile highlighting.</summary>
public sealed class MemorySequencePlayer
{
    private IReadOnlyList<ColorId> sequence = [];
    private double interval;
    private double elapsed;
    private int next;
    public ColorId? Current { get; private set; }
    public int CurrentIndex { get; private set; } = -1;
    public bool IsPlaying => next < sequence.Count;

    public void Start(IEnumerable<ColorId> colors, TimeSpan totalDuration)
    {
        sequence = colors.ToArray(); next = 0; elapsed = 0; CurrentIndex = sequence.Count > 0 ? 0 : -1; Current = sequence.Count > 0 ? sequence[0] : null;
        interval = sequence.Count == 0 ? 0 : Math.Max(.01, totalDuration.TotalSeconds / sequence.Count);
    }

    public IReadOnlyList<ColorId> Update(TimeSpan elapsedTime)
    {
        if (!IsPlaying || interval <= 0) return [];
        elapsed += elapsedTime.TotalSeconds;
        var emitted = new List<ColorId>();
        while (elapsed >= interval && next < sequence.Count)
        {
            elapsed -= interval;
            emitted.Add(sequence[next]);
            next++;
            CurrentIndex = next < sequence.Count ? next : -1;
            Current = next < sequence.Count ? sequence[next] : null;
        }
        return emitted;
    }
}
