using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Chat.Services.Redis;

public class RedisService : IRedisService
{
    private readonly IDatabase _redisDb;

    public RedisService(IConnectionMultiplexer mux)
    {
        _redisDb = mux.GetDatabase();
    }

    public async Task SetValue(string key, string value, TimeSpan ttl)
    {
        //salvo la chiave su redis
        await _redisDb.StringSetAsync(key, value, ttl);
    }

    public async Task<string?> GetValue(string key)
    {
        //recupero dati da redis
        return await _redisDb.StringGetAsync(key);
    }

    public async Task Remove(string key) 
    {
        //elimino chiave provvisoria in redis
        await _redisDb.KeyDeleteAsync(key);
    }

    public async Task<double> Increment(string key, double value)
    {
        //incrementa il valore della chiave immagino di 1
        var result = await _redisDb.StringIncrementAsync(key, value);
        return result;
    }

    public async Task<bool> Expire(string key, TimeSpan ttl)
    {
        //operazione di impostare il TTL ha avuto successo.
        return await _redisDb.KeyExpireAsync(key, ttl);   
    }
    
}
