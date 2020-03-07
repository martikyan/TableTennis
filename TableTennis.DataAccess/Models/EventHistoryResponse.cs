using System.Collections.Generic;

namespace TableTennis.DataAccess.Models
{
    public class EventHistoryResponse
    {
        public int Success { get; set; }

        public ResultsModel Results { get; set; }

        public class ResultsModel
        {
            public List<AwayElement> H2H { get; set; }

            public List<AwayElement> Home { get; set; }

            public List<AwayElement> Away { get; set; }

            public class AwayElement
            {
                public int Id { get; set; }

                public int SportId { get; set; }

                public SidePlayerModel League { get; set; }

                public SidePlayerModel Home { get; set; }

                public SidePlayerModel Away { get; set; }
                public int Time { get; set; }

                public string Ss { get; set; }

                public int TimeStatus { get; set; }

                public class SidePlayerModel
                {
                    public int Id { get; set; }

                    public string Name { get; set; }

                    public int? ImageId { get; set; }
                }
            }
        }
    }
}