using System.Threading.Tasks;

namespace TableTennis.DataAccess.Telegram
{
    public interface IEventsRepository
    {
        Task AddAsync(int eventId);

        Task<bool> ExistsAsync(int eventId);
    }
}