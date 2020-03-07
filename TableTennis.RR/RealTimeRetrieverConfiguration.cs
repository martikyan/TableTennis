namespace TableTennis.RR
{
    public class RealTimeRetrieverConfiguration
    {
        public string BetsApiAccessToken { get; set; }
        public int MinimalHistoryCount { get; set; }
        public int MaximalBigScoresPercentage { get; set; }
        public int ScanThresholdSeconds { get; set; }
    }
}