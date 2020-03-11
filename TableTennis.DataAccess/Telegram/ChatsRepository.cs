using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableTennis.DataAccess.DBAccess;
using TableTennis.DataAccess.DBAccess.Models;

namespace TableTennis.DataAccess.Telegram
{
    public class ChatsRepository : IChatsRepository
    {
        private readonly Func<PostgreSqlDbContext> _dbContextAccessor;

        public ChatsRepository(Func<PostgreSqlDbContext> dbContextAccessor)
        {
            _dbContextAccessor = dbContextAccessor ?? throw new ArgumentNullException(nameof(dbContextAccessor));
        }


        public async Task AddChatAsync(long chatId)
        {
            using (var context = _dbContextAccessor())
            {
                await context.Chats.AddAsync(new Chat {ChatId = chatId});
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<long>> GetAllChatsAsync()
        {
            using (var context = _dbContextAccessor())
            {
                return await context.Chats.AsNoTracking().Select(c => c.ChatId).ToListAsync();
            }
        }

        public async Task<bool> ExistsAsync(long chatId)
        {
            using (var context = _dbContextAccessor())
            {
                return await context.Chats.AsNoTracking().ContainsAsync(new Chat {ChatId = chatId});
            }
        }
    }
}