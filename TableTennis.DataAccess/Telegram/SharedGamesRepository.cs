using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TableTennis.DataAccess.Telegram
{
    public class SharedGamesRepository : ISharedGamesRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public SharedGamesRepository(string connectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        }

        public Task AddAsync(string player1Name, string player2Name)
        {
            var gameId = GetGameId(player1Name, player2Name);

            // Move TTL seconds to configuration.
            return _connectionMultiplexer.GetDatabase(2)
                .StringSetAsync(gameId.ToString(), true, TimeSpan.FromMinutes(35));
        }

        public async Task<bool> ExistsAsync(string player1Name, string player2Name)
        {
            var gameId = GetGameId(player1Name, player2Name);
            var value = await _connectionMultiplexer.GetDatabase(2).StringGetAsync(gameId.ToString());

            return value == default || (bool) value == false;
        }

        private int GetGameId(string player1Name, string player2Name)
        {
            var gameId = player1Name.GetHashCode() ^ player2Name.GetHashCode();
            return gameId;
        }
    }
}