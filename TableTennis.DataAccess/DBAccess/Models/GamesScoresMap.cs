namespace TableTennis.DataAccess.DBAccess.Models
{
    public class GamesScoresMap
    {
        public int GameId { get; set; }

        public int ScoreId { get; set; }

        public Game Game { get; set; }
        public Score Score { get; set; }
    }
}