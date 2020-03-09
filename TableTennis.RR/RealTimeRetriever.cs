using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableTennis.DataAccess;
using TableTennis.DataAccess.Models;
using TableTennis.DataAccess.Telegram;
using TableTennis.RR.Helpers;
using TableTennis.RR.Models;

namespace TableTennis.RR
{
    public class RealTimeRetriever
    {
        private readonly IBetsApiClient _betsApiClient;
        private readonly RealTimeRetrieverConfiguration _configuration;
        private readonly IEventsRepository _eventsRepository;

        public RealTimeRetriever(IBetsApiClient betsApiClient, IEventsRepository eventsRepository,
            RealTimeRetrieverConfiguration configuration)
        {
            _betsApiClient = betsApiClient ?? throw new ArgumentNullException(nameof(betsApiClient));
            _eventsRepository = eventsRepository ?? throw new ArgumentNullException(nameof(eventsRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
            var result = new ConcurrentBag<Tuple<Score, Score>>();
            var singleEvent = await _betsApiClient.GetSingleEventAsync(_configuration.BetsApiAccessToken, eventId);
            var name1 = singleEvent.Results[0].Home.Name;
            var name2 = singleEvent.Results[0].Away.Name;
            foreach (var date in DateTimeHelper.GetPast90DaysYYYYMMDD().AsParallel())
            {
                var daySearch1Task = _betsApiClient.SearchEventsForTeamsAsync(_configuration.BetsApiAccessToken,
                    (int) SportId.TableTennis, name1, name2, date);

                var daySearch2Task = _betsApiClient.SearchEventsForTeamsAsync(_configuration.BetsApiAccessToken,
                    (int) SportId.TableTennis, name2, name1, date);


                var scores = ExtractFinalScores(await daySearch1Task);
                scores.AddRange(ExtractFinalScores(await daySearch2Task));
                foreach (var tuple in from score in scores
                    let score1 = new Score {ActualScore = score.Item1, PlayerName = name1}
                    let score2 = new Score {ActualScore = score.Item2, PlayerName = name2}
                    select Tuple.Create(score1, score2))
                    result.Add(tuple);
            }

            return result.ToList();
        }

        private async Task<List<int>> GetInplayGameIds()
        {
            var result = new List<int>();
            var response =
                await _betsApiClient.GetInplayEventsAsync((int) SportId.TableTennis, _configuration.BetsApiAccessToken);
            foreach (var e in response.Results)
            {
                var wasRetrieved = await _eventsRepository.ExistsAsync(e.Id);
                if (wasRetrieved) continue;

                result.Add(e.Id);
                await _eventsRepository.AddAsync(e.Id);
            }

            return result;
        }

        private static List<Tuple<int, int>> ExtractFinalScores(EventSearchResponse response)
        {
            var result = new List<Tuple<int, int>>();
            try
            {
                foreach (var singleRes in response.Results)
                {
                    var scores = singleRes.Scores.Values;
                    foreach (var score in scores) result.Add(Tuple.Create(score.Home, score.Away));
                }
            }
            catch
            {
                // ignored
            }

            return result;
        }
    }
}