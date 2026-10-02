using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public abstract record ZipDelivery
{
    public sealed record Redirect(string Path) : ZipDelivery;

    public sealed record Stream(ReleaseVersion ReleaseVersion) : ZipDelivery;
}
