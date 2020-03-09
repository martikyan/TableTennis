using System.Threading.Tasks;

namespace TableTennis.DataAccess.Telegram
{
    public interface IEventsRepository
    {
        Task AddAsync(string player1Name, string player2Name);

        Task<bool> ExistsAsync(string player1Name, string player2Name);
    }
}