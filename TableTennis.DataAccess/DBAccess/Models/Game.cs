using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TableTennis.DataAccess.DBAccess.Models
{
    public class Game
    {
        [Key] public int Id { get; set; }

        public int Player1Id { get; set; }
        public int Player2Id { get; set; }

        public PlayerInfo Player1 { get; set; }
        public PlayerInfo Player2 { get; set; }

        public ICollection<GamesScoresMap> Scores { get; set; }
    }
}