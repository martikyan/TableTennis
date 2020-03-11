using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableTennis.DataAccess;
using TableTennis.DataAccess.DBAccess;
using TableTennis.DataAccess.Models;
using TableTennis.DataAccess.Telegram;
using TableTennis.RR.Models;

namespace TableTennis.RR
{
    public class RealTimeRetriever
    {
        private readonly IBetsApiClient _betsApiClient;
        private readonly RealTimeRetrieverConfiguration _configuration;
        private readonly Func<PostgreSqlDbContext> _dbContextAccessor;
        private readonly IEventsRepository _eventsRepository;

        public RealTimeRetriever(IBetsApiClient betsApiClient, IEventsRepository eventsRepository,
            RealTimeRetrieverConfiguration configuration,
            Func<PostgreSqlDbContext> dbContextAccessor)
        {
            _betsApiClient = betsApiClient ?? throw new ArgumentNullException(nameof(betsApiClient));
            _eventsRepository = eventsRepository ?? throw new ArgumentNullException(nameof(eventsRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dbContextAccessor = dbContextAccessor ?? throw new ArgumentNullException(nameof(dbContextAccessor));
        }

        public event UnbalancedOddsHandler OnUnbalancedOddsFound = delegate { };
        public event GoodBigScorePercentageHandler OnGoodBigScorePercentageFound = delegate { };

        public async void Start()
        {
            while (true)
            {
                var inPlayGameIds = await GetInplayGameIds();
                foreach (var id in inPlayGameIds)
                {
                    var oddsResult =
                        await _betsApiClient.GetSingleEventOddsSummaryAsync(_configuration.BetsApiAccessToken, id);

                    var odds = ExtractOdds(oddsResult);
                    if (odds != null)
                    {
                        var singleResult =
                            await _betsApiClient.GetSingleEventAsync(_configuration.BetsApiAccessToken, id);
                        var oddsDiff = Math.Abs(odds.Item1 - odds.Item2);

                        if (oddsDiff > _configuration.MinimalOddsDifference)
                            OnUnbalancedOddsFound(odds.Item1, odds.Item2, singleResult.Results[0].Home.Name,
                                singleResult.Results[0].Away.Name);
                    }

                    var historyResult = await GetHistoryOfGamesAsync(id);
                    AnalyzeScores(historyResult);
                }

                await Task.Delay(_configuration.ScanThresholdSeconds * 1000);
            }
        }

        private static Tuple<double, double> ExtractOdds(SingleEventOddsSummary oddsResult)
        {
            try
            {
                var odd1 = double.Parse(oddsResult.Results.Bet365.Odds.Start.The92_1.HomeOd);
                var odd2 = double.Parse(oddsResult.Results.Bet365.Odds.Start.The92_1.AwayOd);

                return Tuple.Create(odd1, odd2);
            }
            catch
            {
                return null;
            }
        }

        private void AnalyzeScores(List<Tuple<Score, Score>> scores)
        {
            if (scores.Count < _configuration.MinimalHistoryCount) return;

            var sums = scores.Select(score => score.Item1.ActualScore + score.Item2.ActualScore).ToList();
            var totalScoresCount = sums.Count;
            var bigScoresCount = sums.Count(s => s >= 22);
            var bigScoresPercentage = bigScoresCount * 100.0 / totalScoresCount;
            if (bigScoresPercentage <= _configuration.MaximalBigScoresPercentage)
                OnGoodBigScorePercentageFound(totalScoresCount, bigScoresCount, scores[0].Item1.PlayerName,
                    scores[0].Item2.PlayerName);
        }

        private async Task<List<Tuple<Score, Score>>> GetHistoryOfGamesAsync(int eventId)
        {
            var result = new List<Tuple<Score, Score>>();
            var singleEvent = await _betsApiClient.GetSingleEventAsync(_configuration.BetsApiAccessToken, eventId);
            var name1 = singleEvent.Results[0].Home.Name;
            var name2 = singleEvent.Results[0].Away.Name;

            using (var db = _dbContextAccessor())
            {
                var games = await db.Games
                    .Include(g => g.Player1)
                    .Include(g => g.Player2)
                    .Include(g => g.Scores)
                    .ThenInclude(sc => sc.Score)
                    .AsNoTracking().Where(g =>
                        g.Player1.Name == name1 && g.Player2.Name == name2 ||
                        g.Player1.Name == name2 && g.Player2.Name == name1)
                    .ToListAsync();

                foreach (var gameScore in games.SelectMany(g => g.Scores))
                {
                    var score1 = new Score
                    {
                        ActualScore = gameScore.Score.Score1,
                        PlayerName = name1
                    };

                    var score2 = new Score
                    {
                        ActualScore = gameScore.Score.Score2,
                        PlayerName = name2
                    };

                    result.Add(new Tuple<Score, Score>(score1, score2));
                }
            }

            return result.ToList();
        }

        private async Task<List<int>> GetInplayGameIds()
        {
            var result = new List<int>();
            var response =
                await _betsApiClient.GetInplayEventsAsync((int) SportId.TableTennis, _configuration.BetsApiAccessToken);
            if (response?.Results == null) return result;

            foreach (var e in response.Results)
            {
                var wasRetrieved = await _eventsRepository.ExistsAsync(e.Id);
                if (wasRetrieved) continue;

                result.Add(e.Id);
                await _eventsRepository.AddAsync(e.Id);
            }

            return result;
        }
    }
}