using System.Collections.Generic;

namespace TableTennis.DataAccess.Models
{
    public class EventSearchResponse
    {
        public int Success { get; set; }
        public List<Result> Results { get; set; }

        public class Result
        {
            public int Id { get; set; }
            public int SportId { get; set; }
            public int Time { get; set; }
            public int TimeStatus { get; set; }
            public SideModel League { get; set; }
            public SideModel Home { get; set; }
            public SideModel Away { get; set; }
            public string Ss { get; set; }
            public Dictionary<string, Score> Scores { get; set; }

            public class SideModel
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public object ImageId { get; set; }
                public object Cc { get; set; }
            }

            public class Score
            {
                public int Home { get; set; }
                public int Away { get; set; }
            }
        }
    }
}