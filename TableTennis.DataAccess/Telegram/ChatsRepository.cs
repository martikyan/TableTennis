using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TableTennis.DataAccess.Telegram
{
    public class ChatsRepository : IChatsRepository
    {
        private const string CHATS_KEY = "ChatIds";
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public ChatsRepository(string connectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        }

        public Task AddChatAsync(long chatId)
        {
            return _connectionMultiplexer.GetDatabase(1).SetAddAsync(CHATS_KEY, chatId);
        }

        public async Task<List<long>> GetAllChatsAsync()
        {
            var result = await _connectionMultiplexer.GetDatabase(1).SetMembersAsync(CHATS_KEY);
            return result.Select(r => long.Parse(r)).ToList();
        }

        public Task<bool> ExistsAsync(long chatId)
        {
            return _connectionMultiplexer.GetDatabase(1).SetContainsAsync(CHATS_KEY, chatId.ToString());
        }
    }
}