using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableTennis.DataAccess.DBAccess;
using TableTennis.DataAccess.DBAccess.Models;

namespace TableTennis.DataAccess.Telegram
{
    public class EventsRepository : IEventsRepository
    {
        private readonly Func<PostgreSqlDbContext> _dbContextAccessor;

        public EventsRepository(Func<PostgreSqlDbContext> dbContextAccessor)
        {
            _dbContextAccessor = dbContextAccessor ?? throw new ArgumentNullException(nameof(dbContextAccessor));
        }


        public async Task AddAsync(int eventId)
        {
            using (var context = _dbContextAccessor())
            {
                await context.Events.AddAsync(new Event {EventId = eventId});
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int eventId)
        {
            using (var context = _dbContextAccessor())
            {
                return await context.Events.ContainsAsync(new Event {EventId = eventId});
            }
        }
    }
}