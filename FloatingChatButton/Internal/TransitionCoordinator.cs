namespace FloatingChatButton.Internal;

internal sealed class TransitionCoordinator
{
    private int _version;

    internal int Version => _version;

    internal bool IsTransitioning { get; private set; }

    internal int Begin()
    {
        IsTransitioning = true;
        return Interlocked.Increment(ref _version);
    }

    internal void Cancel()
    {
        IsTransitioning = false;
        Interlocked.Increment(ref _version);
    }

    internal bool IsCurrent(int version)
    {
        return version == _version;
    }

    internal bool CompleteIfCurrent(int version)
    {
        if (!IsCurrent(version))
        {
            return false;
        }

        IsTransitioning = false;
        return true;
    }
}
