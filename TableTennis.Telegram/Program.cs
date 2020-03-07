using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TableTennis.DataAccess.Telegram;

namespace TableTennis.Telegram
{
    public class Program
    {
        public static void Main()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false);

            var configuration = builder.Build();

            var redisConnectionString = configuration.GetConnectionString("Redis");
            var tbc = new TelegramBotConfiguration();
            configuration.Bind(nameof(TelegramBotConfiguration), tbc);


            var serviceProvider = new ServiceCollection()
                .AddSingleton(tbc)
                .AddSingleton<IAccessTokenRepository, AccessTokenRepository>(isp =>
                    new AccessTokenRepository(redisConnectionString))
                .AddSingleton<IChatsRepository, ChatsRepository>(isp => new ChatsRepository(redisConnectionString))
                .AddSingleton<TableTennisBot>()
                .BuildServiceProvider();


            serviceProvider.GetService<TableTennisBot>();

            new ManualResetEvent(false).WaitOne();
        }
    }
}