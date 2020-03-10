namespace TableTennis.DataExtractor
{
    public class DataExtractorConfiguration
    {
        public string PostgreSqlConnectionString { get; set; }
        public string BetsApiUrl { get; set; }
        public string BetsApiAccessToken { get; set; }
    }
}