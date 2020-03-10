using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TableTennis.DataAccess;
using TableTennis.DataAccess.DBAccess;

namespace TableTennis.DataExtractor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false);

            var configuration = builder.Build();

            var dec = new DataExtractorConfiguration();
            configuration.Bind(nameof(DataExtractorConfiguration), dec);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(dec)
                .AddSingleton<DataExtractor>()
                .AddSingleton<IBetsApiClient, BetsApiClient>(isp => new BetsApiClient(dec.BetsApiUrl))
                .AddDbContext<PostgreSqlDbContext>(db => db.UseNpgsql(dec.PostgreSqlConnectionString))
                .BuildServiceProvider();
            
            await serviceProvider.GetService<PostgreSqlDbContext>().Database.EnsureCreatedAsync();
            await serviceProvider.GetService<DataExtractor>().ScanAllPagesAsync();
            Console.WriteLine("Done");
        }
    }
}