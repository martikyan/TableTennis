using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableTennis.DataAccess;
using TableTennis.DataAccess.DBAccess;
using TableTennis.DataAccess.DBAccess.Models;
using TableTennis.DataAccess.Models;

namespace TableTennis.DataExtractor
{
    public class DataExtractor
    {
        private readonly IBetsApiClient _betsApiClient;

        private readonly DataExtractorConfiguration _configuration;
        private readonly PostgreSqlDbContext _dbContext;

        public DataExtractor(DataExtractorConfiguration configuration, PostgreSqlDbContext dbContext,
            IBetsApiClient betsApiClient)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _betsApiClient = betsApiClient ?? throw new ArgumentNullException(nameof(betsApiClient));
        }

        public async Task ScanPassedGamesAsync()
        {
            var earliestDate = DateTimeOffset.UtcNow;
            Console.WriteLine($"Starting from {earliestDate}");
            foreach (var date in GetYYYYMMDDs(earliestDate.DateTime, _configuration.DaysToScanBefore))
            {
                Console.WriteLine($"Current date {date}");
                EndedEventsPage firstPage;
                var attempt = 0;
                do
                {
                    firstPage = await _betsApiClient.GetEndedEventsDayAsync((int) SportId.TableTennis,
                        _configuration.BetsApiAccessToken,
                        date, 0);

                    if (firstPage?.Results == null)
                    {
                        attempt++;
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                } while (firstPage?.Results == null && attempt < 10);

                if (firstPage?.Results == null) break;

                await RegisterPageResults(firstPage);

                var totalPages = 6;
                if (firstPage.Pager.PerPage != 0)
                    totalPages = 1 + firstPage.Pager.Total / firstPage.Pager.PerPage;

                Console.WriteLine($"Total pages in this date {totalPages}");
                for (var i = 2; i <= totalPages + 4; i++)
                {
                    Console.WriteLine($"Scanning page N{i}");
                    await ScanSinglePage(i, date);
                    Console.WriteLine($"Done scanning page N{i}");
                }
            }
        }

        private IEnumerable<int> GetYYYYMMDDs(DateTime from, int days)
        {
            var iDate = from;
            for (var i = 0; i < days; i++)
            {
                yield return int.Parse(
                    iDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                iDate = iDate.Subtract(TimeSpan.FromDays(1));
            }
        }

        private async Task ScanSinglePage(int page, int date)
        {
            var resp = await _betsApiClient.GetEndedEventsDayAsync((int) SportId.TableTennis,
                _configuration.BetsApiAccessToken,
                date,
                page);


            await RegisterPageResults(resp);
        }

        private async Task RegisterPageResults(EndedEventsPage eep)
        {
            if (eep?.Results == null) return;

            foreach (var result in eep.Results)
            {
                var containsGame = await _dbContext.Games.AnyAsync(g => g.Id == result.Id);
                if (containsGame) continue;

                var game = await ExtractGameFromResult(result);
                if (game == null) continue;

                Console.WriteLine($"Adding game with Id: {game.Id}");
                await _dbContext.AddAsync(game);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task<Game> ExtractGameFromResult(EndedEventsPage.Result result)
        {
            var game = new Game
            {
                Id = result.Id,
                Scores = new List<GamesScoresMap>()
            };

            var player1 = new PlayerInfo();
            var player2 = new PlayerInfo();
            player1.Name = result.Home.Name;
            player2.Name = result.Away.Name;

            game.Player1 =
                await _dbContext.PlayerInfos.FirstOrDefaultAsync(pi => pi.Name == player1.Name) ??
                player1;
            game.Player2 =
                await _dbContext.PlayerInfos.FirstOrDefaultAsync(pi => pi.Name == player2.Name) ??
                player2;

            if (result.Scores?.Values == null) return null;

            foreach (var extractedScore in result.Scores.Values.Select(score => new Score
            {
                Score1 = score.Home,
                Score2 = score.Away
            }))
                game.Scores.Add(new GamesScoresMap
                {
                    Score = extractedScore,
                    Game = game
                });

            return game;
        }
    }
}