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
        private readonly string _token;
        private readonly IBetsApiClient _betsApiClient;
        
        public RealTimeRetriever(IBetsApiClient betsApiClient, string token)
        {
            _betsApiClient = betsApiClient ?? throw new ArgumentNullException(nameof(betsApiClient));
            _token = token ?? throw new ArgumentNullException(nameof(token));
        }
        
        public async void Start()
        {
            while (true)
            {
                var inPlayGameIds = await GetInPlayGameIds();
                foreach (var id in inPlayGameIds)
                {
                    var results = await GetHistoryOfGamesAsync(id);
                    Console.WriteLine($"{results[0].Item1.PlayerName} - {results[0].Item2.PlayerName}");
                    foreach (var res in results)
                    {
                        var score1 = res.Item1.PlayerName == results[0].Item1.PlayerName
                            ? results[0].Item1.ActualScore
                            : results[0].Item2.ActualScore;
                        
                        var score2 = res.Item2.PlayerName == results[0].Item2.PlayerName
                            ? results[0].Item2.ActualScore
                            : results[0].Item1.ActualScore;

                        Console.WriteLine($"{score1}-{score2}");
                    }
                    
                    Console.WriteLine($"");
                }

                Console.WriteLine("Done printing, sleeping for 10 secs");
                await Task.Delay(10000);
            }
        }

        private async Task<List<Tuple<Score, Score>>> GetHistoryOfGamesAsync(int eventId)
        {
            var result = new List<Tuple<Score, Score>>();
            var history = await _betsApiClient.GetEventsHistoryAsync(_token, eventId);
            foreach (var game in history.Results.Away)
            {
                var res = game.Ss.Split('-');
                var score1 = new Score
                {
                    PlayerName = game.Home.Name,
                    ActualScore = int.Parse(res[0]),
                };
                
                var score2 = new Score
                {
                    PlayerName = game.Away.Name,
                    ActualScore = int.Parse(res[1]),
                };
                
                result.Add(Tuple.Create(score1, score2));
            }

            return result;
        } 

        private async Task<List<int>> GetInPlayGameIds()
        {
            var response = await _betsApiClient.GetInplayEventsAsync((int)SportId.TableTennis, _token);
            return response.Results.Select(r => r.Id).ToList();
        }
    }
}