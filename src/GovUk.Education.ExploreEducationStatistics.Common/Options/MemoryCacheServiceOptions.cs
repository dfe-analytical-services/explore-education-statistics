namespace GovUk.Education.ExploreEducationStatistics.Common.Options;

public class MemoryCacheServiceOptions
{
    public const string Section = "MemoryCache";

    public bool Enabled { get; set; }

    public int MaxCacheSizeMb { get; set; }

    public int ExpirationScanFrequencySeconds { get; set; }

    public Overrides? Overrides { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Overrides
{
    public int DurationInSeconds { get; set; }
}
