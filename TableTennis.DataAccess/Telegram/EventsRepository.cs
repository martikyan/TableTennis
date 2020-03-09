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

        public Task AddAsync(int eventId)
        {
            return _connectionMultiplexer.GetDatabase(2)
                .StringSetAsync(eventId.ToString(), true, TimeSpan.FromHours(1));
        }

        public Task<bool> ExistsAsync(int eventId)
        {
            return _connectionMultiplexer.GetDatabase(2).KeyExistsAsync(eventId.ToString());
        }
    }
}