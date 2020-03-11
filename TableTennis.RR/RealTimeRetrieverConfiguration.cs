namespace TableTennis.RR
{
    public class RealTimeRetrieverConfiguration
    {
        public string BetsApiUrl { get; set; }
        public string BetsApiAccessToken { get; set; }
        public int MinimalHistoryCount { get; set; }
        public double MinimalOddsDifference { get; set; }
        public int MaximalBigScoresPercentage { get; set; }
        public int ScanThresholdSeconds { get; set; }
        public int MaxGamesToRetrieve { get; set; }
    }
}