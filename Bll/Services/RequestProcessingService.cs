using System.Collections.Concurrent;
using Bll.Interfaces;

namespace Bll.Services;

public class RequestProcessingService : IRequestProcessingService
{
    private readonly ConcurrentDictionary<string, bool> _processingRequests = new();

    public bool StartProcessing(string key)
    {
        return _processingRequests.TryAdd(key, true);
    }

    public void FinishProcessing(string key)
    {
        _processingRequests.TryRemove(key, out _);
    }
}
