using System.Threading.Tasks;

namespace TableTennis.DataAccess.Telegram
{
    public interface ISharedGamesRepository
    {
        Task AddAsync(string player1Name, string player2Name);

        Task<bool> ExistsAsync(string player1Name, string player2Name);
    }
}