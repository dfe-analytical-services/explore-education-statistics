#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class PreReleaseAccessOptions
{
    public const string Section = "PreReleaseAccess";

    public AccessWindowOptions AccessWindow { get; set; } = null!;
}

public class AccessWindowOptions
{
    public int MinutesBeforeReleaseTimeStart { get; set; }
}
