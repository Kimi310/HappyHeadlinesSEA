namespace ArticleCacheService.Service;

public sealed class CacheRuntimeState
{
    private volatile bool _enabled;

    public CacheRuntimeState(bool enabled)
    {
        _enabled = enabled;
    }

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }
}
