using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableTennis.DataAccess;
using TableTennis.DataAccess.Models;
using TableTennis.RR.Models;

namespace TableTennis.RR
{
    public class RealTimeRetriever
    {
        private readonly IBetsApiClient _betsApiClient;
        private readonly RealTimeRetrieverConfiguration _configuration;

        public RealTimeRetriever(IBetsApiClient betsApiClient, RealTimeRetrieverConfiguration configuration)
        {
            _betsApiClient = betsApiClient ?? throw new ArgumentNullException(nameof(betsApiClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public event GoodBigScorePercentageHandler OnGoodBigScorePercentageFound = delegate { };

        public async void Start()
        {
            while (true)
            {
                var inPlayGameIds = await GetInplayGameIds();
                foreach (var id in inPlayGameIds)
                {
                    var results = await GetHistoryOfGamesAsync(id);
                    AnalyzeScores(results);
                }

                await Task.Delay(_configuration.ScanThresholdSeconds * 1000);
            }
        }

        private void AnalyzeScores(List<Tuple<Score, Score>> scores)
        {
            if (scores.Count < _configuration.MinimalHistoryCount) return;

            var sums = new List<int>();
            var oddsCount = 0;
            var evensCount = 0;
            foreach (var sum in scores.Select(score => score.Item1.ActualScore + score.Item2.ActualScore))
            {
                sums.Add(sum);
                if (sum % 2 == 0)
                    evensCount++;
                else
                    oddsCount++;
            }

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
            var history = await _betsApiClient.GetEventsHistoryAsync(_configuration.BetsApiAccessToken, eventId);
            var oldEventIds = history.Results.Away.Select(e => e.Id).ToList();
            oldEventIds.AddRange(history.Results.Home.Select(e => e.Id));
            oldEventIds = oldEventIds.Distinct().ToList();

            foreach (var oldEventId in oldEventIds)
            {
                var eventResults =
                    await _betsApiClient.GetSingleEventAsync(_configuration.BetsApiAccessToken, oldEventId);
                var singleResult = eventResults.Results.FirstOrDefault();
                if (singleResult?.Scores == null || singleResult.Scores.Count == 0) continue;

                var homeName = singleResult.Home.Name;
                var awayName = singleResult.Away.Name;
                foreach (var score in singleResult.Scores)
                {
                    if (score.Value.Home == null || score.Value.Away == null) continue;

                    var score1 = new Score
                    {
                        ActualScore = (int) score.Value.Home,
                        PlayerName = homeName
                    };

                    var score2 = new Score
                    {
                        ActualScore = (int) score.Value.Away,
                        PlayerName = awayName
                    };

                    result.Add(new Tuple<Score, Score>(score1, score2));
                }
            }

            return result;
        }

        private async Task<List<int>> GetInplayGameIds()
        {
            var response =
                await _betsApiClient.GetInplayEventsAsync((int) SportId.TableTennis, _configuration.BetsApiAccessToken);
            return response.Results.Select(r => r.Id).ToList();
        }
    }
}