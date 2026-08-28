using IdentityService.Services.IService;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace IdentityService.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redisConnection, ILogger<RedisService> logger)
        {
            _redisConnection = redisConnection;
            _logger = logger;
        }

        // Helper lấy database an toàn
        private IDatabase? GetDatabase()
        {
            try
            {
                // Kiểm tra kết nối
                if (!_redisConnection.IsConnected)
                {
                    _logger.LogWarning("[RedisService] Redis is not connected.");
                    return null;
                }
                return _redisConnection.GetDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RedisService] Error getting Redis Database.");
                return null;
            }
        }

        // 1. Lưu giá trị kèm thời gian sống (TTL)
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return;

                string jsonString = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, jsonString, (Expiration)expiry);
            }
            catch (Exception ex)
            {
                // Ghi log cảnh báo khi không ghi được vào Redis, không throw lỗi làm chết luồng chính
                _logger.LogError(ex, "[RedisService] Failed to set key '{Key}' to Redis.", key);
            }
        }

        // 2. Lấy giá trị và tự động Parse về kiểu dữ liệu ban đầu
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return default;

                string? jsonString = await db.StringGetAsync(key);
                if (string.IsNullOrEmpty(jsonString))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RedisService] Failed to get key '{Key}' from Redis.", key);
                return default;
            }
        }

        // 3. Xóa một Key khỏi Redis
        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return false;

                return await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RedisService] Failed to delete key '{Key}' from Redis.", key);
                return false;
            }
        }

        // 4. Kiểm tra Key có tồn tại không (Rất quan trọng cho Blacklist JTI)
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var db = GetDatabase();
                // Nếu Redis sập (db == null) -> Trả về false (Fail-Open) để cho phép request đi tiếp
                if (db == null) return false;

                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                // Khi Redis bị timeout/mất kết nối đột ngột: Ghi log cảnh báo và trả về false
                _logger.LogWarning(ex, "[RedisService] Redis connection error while checking key '{Key}'. Falling back to FALSE (Fail-Open).", key);
                return true;
            }
        }
    }
}