namespace GovUk.Education.ExploreEducationStatistics.Common.Options;

public class FeatureFlags
{
    /// <summary>
    /// Whilst EES-5779 is being developed, this prevents
    /// code that is not ready to run in all environments.
    /// </summary>
    public const string Section = "FeatureFlags";
    public bool EnableReplacementOfPublicApiDataSets { get; set; } = false;
}
