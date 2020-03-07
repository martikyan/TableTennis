using System.Collections.Generic;
using System.Threading.Tasks;

namespace TableTennis.DataAccess.Telegram
{
    public interface IChatsRepository
    {
        Task AddChatAsync(long chatId);
        Task<List<long>> GetAllChatsAsync();

        Task<bool> ExistsAsync(long chatId);
    }
}