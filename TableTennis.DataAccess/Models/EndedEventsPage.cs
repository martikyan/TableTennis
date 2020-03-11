using System.Collections.Generic;

namespace TableTennis.DataAccess.Models
{
    public class EndedEventsPage
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
            public Dictionary<string, ScoreModel> Scores { get; set; }
            public Side OAway { get; set; }
            public Side OHome { get; set; }

            public class ScoreModel
            {
                public int Home { get; set; }
                public int Away { get; set; }
            }

            public class Side
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public int? ImageId { get; set; }
                public string Cc { get; set; }
            }
        }
    }
}