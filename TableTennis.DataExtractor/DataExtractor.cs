using System;
using System.Collections.Generic;
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

        public async Task ScanAllPagesAsync()
        {
            for (var i = 100; i >= 0; i--) await ScanSinglePage(i);
        }

        private async Task ScanSinglePage(int page)
        {
            var resp = await _betsApiClient.GetEndedEventsPageAsync((int) SportId.TableTennis,
                _configuration.BetsApiAccessToken,
                page);

            if (resp?.Results == null) return;

            foreach (var result in resp.Results)
            {
                var containsGame = await _dbContext.Games.AnyAsync(g => g.Id == result.Id);
                if (containsGame) continue;

                var game = await ExtractGameFromResult(result);
                if (game == null) continue;

                await _dbContext.AddAsync(game);
                await _dbContext.SaveChangesAsync();
            }

            Console.WriteLine($"Scanned page {page}");
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
            {
                game.Scores.Add(new GamesScoresMap
                {
                    Score = extractedScore,
                    Game = game
                });
            }

            return game;
        }
    }
}