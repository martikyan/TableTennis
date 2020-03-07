using System;
using System.Security.Principal;
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
    }
}