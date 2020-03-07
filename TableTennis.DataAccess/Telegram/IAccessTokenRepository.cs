using System.Threading.Tasks;

namespace TableTennis.DataAccess.Telegram
{
    public interface IAccessTokenRepository
    {
        Task<bool> ExistsAsync(string accessToken);

        Task AddAsync(string accessToken);

        Task MakeUsedAsync(string accessToken);

        Task<bool> IsUsedAsync(string accessToken);

        Task RemoveAsync(string accessToken);
    }
}