using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TableTennis.DataAccess.DBAccess.Models
{
    public class Score
    {
        [Key] public int Id { get; set; }

        public int Score1 { get; set; }
        public int Score2 { get; set; }

        public ICollection<GamesScoresMap> Games { get; set; }
    }
}