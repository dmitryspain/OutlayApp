using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace OutlayApp.Infrastructure.Services.Extensions;

public static class DistributedCacheExtension
{
    public static void SetRecord<T>(this IMemoryCache cache, string recodeId, T data,
        TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpirationTime = null)
    {
        var options = new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromHours(1),
            SlidingExpiration = slidingExpirationTime
        };
        var jsonData = JsonConvert.SerializeObject(data);
        cache.Set(recodeId, jsonData, options);
    }

    public static T GetRecord<T>(this IMemoryCache cache, string recordId)
    {
        cache.TryGetValue(recordId, out string? jsonData);
        return jsonData is null
            ? default!
            : JsonConvert.DeserializeObject<T>(jsonData)!;
    }
}
