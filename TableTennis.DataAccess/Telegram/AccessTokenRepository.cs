using System.Threading.Tasks;
using StackExchange.Redis;

namespace TableTennis.DataAccess.Telegram
{
    public class AccessTokenRepository : IAccessTokenRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public AccessTokenRepository(string connectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        }

        public Task<bool> ExistsAsync(string accessToken)
        {
            return _connectionMultiplexer.GetDatabase(0).KeyExistsAsync(accessToken.ToUpper());
        }

        public Task AddAsync(string accessToken)
        {
            return _connectionMultiplexer.GetDatabase(0).StringSetAsync(accessToken.ToUpper(), false);
        }

        public Task MakeUsedAsync(string accessToken)
        {
            return _connectionMultiplexer.GetDatabase(0).StringSetAsync(accessToken.ToUpper(), true);
        }

        public async Task<bool> IsUsedAsync(string accessToken)
        {
            if (await ExistsAsync(accessToken))
            {
                var result = await _connectionMultiplexer.GetDatabase(0).StringGetAsync(accessToken.ToUpper());
                return (bool) result;
            }

            return false;
        }

        public Task RemoveAsync(string accessToken)
        {
            return _connectionMultiplexer.GetDatabase(0).KeyDeleteAsync(accessToken.ToUpper());
        }
    }
}