using System;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using RestEase;
using TableTennis.DataAccess.Models;

namespace TableTennis.DataAccess
{
    public class BetsApiClient : IBetsApiClient
    {
        private const int MAX_SECONDS_TO_WAIT = 300;
        private readonly IBetsApiClient _betsApiClient;
        private readonly AsyncRetryPolicy _retryPolicy;

        public BetsApiClient(string betsApiUrl)
        {
            _betsApiClient = RestClient.For<IBetsApiClient>(betsApiUrl);
            _retryPolicy = Policy
                .Handle<ApiException>()
                .WaitAndRetryForeverAsync(retryAttempt =>
                {
                    var secondsToWait = Math.Pow(retryAttempt, 2);
                    secondsToWait = Math.Min(secondsToWait, MAX_SECONDS_TO_WAIT);
                    return TimeSpan.FromSeconds(secondsToWait);
                });
        }

        public async Task<EventHistoryResponse> GetEventsHistoryAsync(string token, int eventId)
        {
            var result =
                await _retryPolicy.ExecuteAndCaptureAsync(() => _betsApiClient.GetEventsHistoryAsync(token, eventId));
            return result.Result;
        }

        public async Task<InplayEventsResponse> GetInplayEventsAsync(int sportId, string token)
        {
            var result =
                await _retryPolicy.ExecuteAndCaptureAsync(() => _betsApiClient.GetInplayEventsAsync(sportId, token));
            return result.Result;
        }

        public async Task<SingleEventView> GetSingleEventAsync(string token, int eventId)
        {
            var result =
                await _retryPolicy.ExecuteAndCaptureAsync(() => _betsApiClient.GetSingleEventAsync(token, eventId));
            return result.Result;
        }

        public async Task<SingleEventOddsSummary> GetSingleEventOddsSummaryAsync(string token, int eventId)
        {
            var result =
                await _retryPolicy.ExecuteAndCaptureAsync(() =>
                    _betsApiClient.GetSingleEventOddsSummaryAsync(token, eventId));
            return result.Result;
        }

        public async Task<EventSearchResponse> SearchEventsForTeamsAsync(string token, int sportId, string home,
            string away, int time)
        {
            var result =
                await _retryPolicy.ExecuteAndCaptureAsync(() =>
                    _betsApiClient.SearchEventsForTeamsAsync(token, sportId, home, away, time));
            return result.Result;
        }

        public async Task<EndedEventsPage> GetEndedEventsPageAsync(int sportId, string token, int page)
        {
            var result =
                await _retryPolicy.ExecuteAndCaptureAsync(() =>
                    _betsApiClient.GetEndedEventsPageAsync(sportId, token, page));
            return result.Result;
        }

        public async Task<EndedEventsPage> GetEndedEventsDayAsync(int sportId, string token, int day, int page)
        {
            var result =
                await _retryPolicy.ExecuteAndCaptureAsync(() =>
                    _betsApiClient.GetEndedEventsDayAsync(sportId, token, day, page));
            return result.Result;
        }
    }
}