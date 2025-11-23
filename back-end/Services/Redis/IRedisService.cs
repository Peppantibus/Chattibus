namespace Chat.Services.Redis
{
    public interface IRedisService
    {
        public Task SetValue(string key, string value, TimeSpan ttl);
        public Task<string?> GetValue(string key);
        public Task Remove(string key);
        public Task<double> Increment(string key, double value);
        public Task<bool> Expire(string key, TimeSpan ttl);
    }
}
