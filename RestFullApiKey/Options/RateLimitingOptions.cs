namespace RestFullApiKey.Options
{
    public sealed class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";
        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
        public int QueueLimit { get; set; } = 0;
    }
}
