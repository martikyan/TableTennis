using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableTennis.DataAccess.DBAccess;
using TableTennis.DataAccess.DBAccess.Models;

namespace TableTennis.DataAccess.Telegram
{
    public class AccessTokenRepository : IAccessTokenRepository
    {
        private readonly Func<PostgreSqlDbContext> _dbContextAccessor;

        public AccessTokenRepository(Func<PostgreSqlDbContext> dbContextAccessor)
        {
            _dbContextAccessor = dbContextAccessor ?? throw new ArgumentNullException(nameof(dbContextAccessor));
        }

        public async Task<bool> ExistsAsync(string accessToken)
        {
            using (var context = _dbContextAccessor())
            {
                return await context.AuthCodes.ContainsAsync(new AuthCode {AuthCodeId = accessToken});
            }
        }

        public async Task AddAsync(string accessToken)
        {
            using (var context = _dbContextAccessor())
            {
                await context.AuthCodes.AddAsync(new AuthCode {AuthCodeId = accessToken});
                await context.SaveChangesAsync();
            }
        }

        public async Task MakeUsedAsync(string accessToken)
        {
            using (var context = _dbContextAccessor())
            {
                var token = await context.AuthCodes.FirstAsync(ac => ac.AuthCodeId == accessToken);
                token.IsUsed = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUsedAsync(string accessToken)
        {
            using (var context = _dbContextAccessor())
            {
                var token = await context.AuthCodes.FirstOrDefaultAsync(ac => ac.AuthCodeId == accessToken);
                return token?.IsUsed ?? true;
            }
        }
    }
}