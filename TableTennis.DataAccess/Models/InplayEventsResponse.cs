using System.Collections.Generic;

namespace TableTennis.DataAccess.Models
{
    public class InplayEventsResponse
    {
        public int Success { get; set; }
        public PagerModel Pager { get; set; }
        public List<Result> Results { get; set; }

        public class PagerModel
        {
            public int Page { get; set; }
            public int PerPage { get; set; }
            public int Total { get; set; }
        }

        public class Result
        {
            public int Id { get; set; }
            public int SportId { get; set; }
            public int Time { get; set; }
            public int TimeStatus { get; set; }
            public Side League { get; set; }
            public Side Home { get; set; }
            public Side Away { get; set; }
            public string Ss { get; set; }
            public TimerModel Timer { get; set; }
            public ScoresModel Scores { get; set; }
            public ExtraData Extra { get; set; }
            public int HasLineup { get; set; }
            public int Bet365Id { get; set; }
            public Side OHome { get; set; }

            public class TimerModel
            {
                public int Tm { get; set; }
                public int Ts { get; set; }
                public int Tt { get; set; }
                public int Ta { get; set; }
                public int Md { get; set; }
            }

            public class ScoresModel
            {
                public Both The2 { get; set; }

                public class Both
                {
                    public int Home { get; set; }
                    public int Away { get; set; }
                }
            }

            public class Side
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public int? ImageId { get; set; }
                public string Cc { get; set; }
            }

            public class ExtraData
            {
                public enum PitchEnum
                {
                    ArtificialTurf,
                    Excellent,
                    Good,
                    Regular
                }

                public enum WeatherEnum
                {
                    Cloudy,
                    Cold,
                    Good,
                    Sunny
                }

                public int? Length { get; set; }
                public PitchEnum? Pitch { get; set; }
                public WeatherEnum? Weather { get; set; }
                public Side AwayManager { get; set; }
                public Side HomeManager { get; set; }
                public Side Referee { get; set; }
                public StadiumData StadiumData { get; set; }
                public int? Round { get; set; }
                public int? HomePos { get; set; }
                public int? AwayPos { get; set; }
            }

            public class StadiumData
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string City { get; set; }
                public string Country { get; set; }
                public int Capacity { get; set; }
                public string Googlecoords { get; set; }
            }
        }
    }
}