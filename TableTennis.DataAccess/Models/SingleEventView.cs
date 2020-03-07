using System.Collections.Generic;

namespace TableTennis.DataAccess.Models
{
    public class SingleEventView
    {
        public long Success { get; set; }
        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public long Id { get; set; }
        public long Time { get; set; }
        public long TimeStatus { get; set; }
        public Side League { get; set; }
        public Side Home { get; set; }
        public Side Away { get; set; }
        public TimerModel Timer { get; set; }
        public Dictionary<string, Score> Scores { get; set; }
        public List<Event> Events { get; set; }
        public ExtraModel Extra { get; set; }

        public class Side
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public long? ImageId { get; set; }
            public string Cc { get; set; }
        }

        public class Event
        {
            public long Id { get; set; }
            public string Text { get; set; }
        }

        public class ExtraModel
        {
            public AwayManagerModel AwayManager { get; set; }
            public AwayManagerModel HomeManager { get; set; }
            public long HomePos { get; set; }
            public long AwayPos { get; set; }
            public AwayManagerModel Referee { get; set; }
            public long Round { get; set; }
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
            public long? Home { get; set; }
            public long? Away { get; set; }
        }

        public class TimerModel
        {
            public long Tm { get; set; }
            public long Ts { get; set; }
            public long Tt { get; set; }
        }
    }
}