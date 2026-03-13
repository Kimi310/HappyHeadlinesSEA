namespace ArticleQueue.Services;

public sealed class BrokerRuntimeState
{
    public bool IsRunning { get; private set; }

    public void MarkRunning() => IsRunning = true;
    public void MarkStopped() => IsRunning = false;
}

