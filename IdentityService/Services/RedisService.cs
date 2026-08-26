using IdentityService.Services.IService;
using StackExchange.Redis;
using System.Text.Json;

namespace IdentityService.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;

        public RedisService(IConnectionMultiplexer redisConnection)
        {
            // Lấy Database mặc định (Index 0) hoặc chỉ định số index cụ thể (ví dụ: GetDatabase(1))
            _database = redisConnection.GetDatabase();
        }

        // 1. Lưu giá trị kèm thời gian sống (TTL)
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            string jsonString = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, jsonString, (Expiration)expiry);
        }

        // 2. Lấy giá trị và tự động Parse về kiểu dữ liệu ban đầu
        public async Task<T?> GetAsync<T>(string key)
        {
            string? jsonString = await _database.StringGetAsync(key);
            if (string.IsNullOrEmpty(jsonString))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        // 3. Xóa một Key khỏi Redis
        public async Task<bool> DeleteAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        // 4. Kiểm tra Key có tồn tại không (Rất hữu ích để check Blacklist Token jti)
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }
    }
}