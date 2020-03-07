using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase;
using TableTennis.DataAccess;
using TableTennis.DataAccess.Telegram;
using TableTennis.RR;

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
            var rtrc = new RealTimeRetrieverConfiguration();
            configuration.Bind(nameof(TelegramBotConfiguration), tbc);
            configuration.Bind(nameof(RealTimeRetrieverConfiguration), rtrc);


            var serviceProvider = new ServiceCollection()
                .AddSingleton(tbc)
                .AddSingleton(rtrc)
                .AddSingleton<RealTimeRetriever>()
                .AddSingleton(isp => RestClient.For<IBetsApiClient>(rtrc.BetsApiUrl))
                .AddSingleton<IAccessTokenRepository, AccessTokenRepository>(isp =>
                    new AccessTokenRepository(redisConnectionString))
                .AddSingleton<IChatsRepository, ChatsRepository>(isp => new ChatsRepository(redisConnectionString))
                .AddSingleton<TableTennisBot>()
                .BuildServiceProvider();


            serviceProvider.GetService<RealTimeRetriever>().Start();
            serviceProvider.GetService<TableTennisBot>();

            new ManualResetEvent(false).WaitOne();
        }
    }
}