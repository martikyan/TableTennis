using System;
using System.IO;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TableTennis.DataAccess;
using TableTennis.DataAccess.DBAccess;
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
                .AddSingleton<IBetsApiClient, BetsApiClient>(isp => new BetsApiClient(rtrc.BetsApiUrl))
                .AddSingleton<IAccessTokenRepository, AccessTokenRepository>(isp =>
                    new AccessTokenRepository(redisConnectionString))
                .AddSingleton<IChatsRepository, ChatsRepository>(isp => new ChatsRepository(redisConnectionString))
                .AddSingleton<IEventsRepository, EventsRepository>(isp =>
                    new EventsRepository(redisConnectionString))
                .AddSingleton<TableTennisBot>()
                .AddDbContext<PostgreSqlDbContext>(db => db.UseNpgsql(configuration.GetConnectionString("PostgreSql")))
                .AddDbContextPool<PostgreSqlDbContext>(db =>
                    db.UseNpgsql(configuration.GetConnectionString("PostgreSql")))
                .AddSingleton<Func<PostgreSqlDbContext>>(isp => isp.GetService<PostgreSqlDbContext>)
                .BuildServiceProvider();


            serviceProvider.GetService<TableTennisBot>();
            serviceProvider.GetService<RealTimeRetriever>().StartAsync().Wait();

            new ManualResetEvent(false).WaitOne();
        }
    }
}