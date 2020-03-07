using System.Threading;
using System.Threading.Tasks;
using RestEase;
using TableTennis.DataAccess;

#pragma warning disable 1998

namespace TableTennis.RR
{
    public class Program
    {
        private const string BASE_URL = "https://api.betsapi.com/";
        private const string TOKEN = "37302-Nr7NXkN8D5qBID";

        public static async Task Main(string[] args)

        {
            var rr = new RealTimeRetriever(RestClient.For<IBetsApiClient>(BASE_URL), TOKEN);
            rr.Start();

            new ManualResetEvent(false).WaitOne();
        }
    }
}

#pragma warning restore 1998