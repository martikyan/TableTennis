namespace TableTennis.RR.Models
{
    public delegate void GoodBigScorePercentageHandler(int totalGamesCount, int totalBigScoresCount, string player1Name,
        string player2Name);

    public delegate void UnbalancedOddsHandler(double odds1, double odds2, string player1Name, string player2Name);
}