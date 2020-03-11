using System.Collections.Generic;

namespace TableTennis.DataAccess.Models
{
    public class SingleEventView
    {
        public int Success { get; set; }
        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public int Id { get; set; }
        public int Time { get; set; }
        public int TimeStatus { get; set; }
        public Side League { get; set; }
        public Side Home { get; set; }
        public Side Away { get; set; }
        public TimerModel Timer { get; set; }
        public Dictionary<string, Score> Scores { get; set; }
        public List<Event> Events { get; set; }
        public ExtraModel Extra { get; set; }

        public class Side
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? ImageId { get; set; }
            public string Cc { get; set; }
        }

        public class Event
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        public class ExtraModel
        {
            public AwayManagerModel AwayManager { get; set; }
            public AwayManagerModel HomeManager { get; set; }
            public int HomePos { get; set; }
            public int AwayPos { get; set; }
            public AwayManagerModel Referee { get; set; }
            public int Round { get; set; }
            public string Pitch { get; set; }
            public string Stadium { get; set; }
            public string Weather { get; set; }

            public class AwayManagerModel
            {
                public string Name { get; set; }
                public string Cc { get; set; }
            }
        }

        public class Score
        {
            public int? Home { get; set; }
            public int? Away { get; set; }
        }

        public class TimerModel
        {
            public int Tm { get; set; }
            public int Ts { get; set; }
            public int Tt { get; set; }
        }
    }
}