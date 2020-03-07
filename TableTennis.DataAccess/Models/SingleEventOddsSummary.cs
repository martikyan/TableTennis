using Newtonsoft.Json;

namespace TableTennis.DataAccess.Models
{
    public class SingleEventOddsSummary
    {
        public long Success { get; set; }
        public ResultsModel Results { get; set; }

        public class ResultsModel
        {
            public Bet365Model Bet365 { get; set; }

            public class Bet365Model
            {
                public long MatchingDir { get; set; }
                public Bet365OddsModel Odds { get; set; }

                public class Bet365OddsModel
                {
                    public OddsOnStartEnd Start { get; set; }
                    public OddsOnStartEnd Kickoff { get; set; }
                    public OddsOnStartEnd End { get; set; }

                    public class OddsOnStartEnd
                    {
                        [JsonProperty("92_1")] public HomeAwayOddsModel The92_1 { get; set; }

                        public class HomeAwayOddsModel
                        {
                            public long Id { get; set; }

                            [JsonProperty("home_od")] public string HomeOd { get; set; }

                            [JsonProperty("away_od")] public string AwayOd { get; set; }

                            public string Ss { get; set; }
                            public long AddTime { get; set; }
                        }
                    }
                }
            }
        }
    }
}