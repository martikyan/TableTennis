using System.Threading.Tasks;
using StackExchange.Redis;

namespace TableTennis.DataAccess
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
            return _connectionMultiplexer.GetDatabase(0).KeyExistsAsync(accessToken);
        }

        public Task AddAsync(string accessToken)
        {
            return _connectionMultiplexer.GetDatabase(0).StringSetAsync(accessToken, true);
        }

        public Task RemoveAsync(string accessToken)
        {
            return _connectionMultiplexer.GetDatabase(0).KeyDeleteAsync(accessToken);
        }
    }
}