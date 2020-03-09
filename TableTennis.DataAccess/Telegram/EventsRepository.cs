using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TableTennis.DataAccess.Telegram
{
    public class EventsRepository : IEventsRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public EventsRepository(string connectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        }

        public Task AddAsync(string player1Name, string player2Name)
        {
            var gameId = GetGameId(player1Name, player2Name);

            // Move TTL seconds to configuration.
            return _connectionMultiplexer.GetDatabase(2)
                .StringSetAsync(gameId, true, TimeSpan.FromHours(1));
        }

        public async Task<bool> ExistsAsync(string player1Name, string player2Name)
        {
            var gameId = GetGameId(player1Name, player2Name);
            var exists = await _connectionMultiplexer.GetDatabase(2).KeyExistsAsync(gameId);

            return exists;
        }

        private static string GetGameId(string player1Name, string player2Name)
        {
            var args = new[] {player1Name, player2Name};
            Array.Sort(args);
            return args[0] + args[1];
        }
    }
}