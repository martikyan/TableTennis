using System.Threading.Tasks;
using RestEase;
using TableTennis.DataAccess.Models;

namespace TableTennis.DataAccess
{
    public interface IBetsApiClient
    {
        // https://api.betsapi.com/v1/event/history?token=YOUR_TOKEN&event_id=219465
        [Get("v1/event/history")]
        Task<EventHistoryResponse> GetEventsHistoryAsync([Query] string token, [Query("event_id")] int eventId);

        // https://api.betsapi.com/v1/events/inplay?sport_id=1&token=YOUR-TOKEN
        [Get("v1/events/inplay")]
        Task<InplayEventsResponse> GetInplayEventsAsync([Query("sport_id")] int sportId, [Query] string token);

        // https://api.betsapi.com/v1/event/view?token=YOUR_TOKEN&event_id=92149
        [Get("v1/event/view")]
        Task<SingleEventView> GetSingleEventAsync([Query] string token, [Query("event_id")] int eventId);

        // https://api.betsapi.com/v2/event/odds/summary?token=YOUR_TOKEN&event_id=232751
        [Get("v2/event/odds/summary")]
        Task<SingleEventOddsSummary> GetSingleEventOddsSummaryAsync([Query] string token,
            [Query("event_id")] int eventId);

        // https://api.betsapi.com/v1/events/search?token=YOUR_TOKEN&sport_id=1&home=Man%20City&away=Barcelona&time=1478029500
        [Get("v1/events/search")]
        Task<EventSearchResponse> SearchEventsForTeamsAsync([Query] string token, [Query("sport_id")] int sportId,
            [Query] string home, [Query] string away, [Query] int time);
        
        // https://api.betsapi.com/v2/events/ended?sport_id=1&token=YOUR-TOKEN
        [Get("v2/events/ended")]
        Task<EndedEventsPage> GetEndedEventsPageAsync([Query("sport_id")] int sportId, string token, int page);
        
        // https://api.betsapi.com/v2/events/ended?sport_id=1&token=YOUR-TOKEN
        [Get("v2/events/ended")]
        Task<EndedEventsPage> GetEndedEventsDayAsync([Query("sport_id")] int sportId, string token, int day, int page);
    }
}