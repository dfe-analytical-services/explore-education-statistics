using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class ReleaseBuilder()
{
    private Guid? _publicationId;
    private Guid? _releaseId;
    private string? _releaseSlug;

    public Release Build()
    {
        var release = new Release
        {
            Id = _releaseId ?? Guid.NewGuid(),
            PublicationId = _publicationId ?? Guid.NewGuid(),
            Slug = _releaseSlug ?? "release-slug",
            TimePeriodCoverage = TimeIdentifier.March,
            Year = 2025,
        };
        return release;
    }
    
    public ReleaseBuilder WithPublicationId(Guid publicationId)
    {
        _publicationId = publicationId;
        return this;
    }

    public ReleaseBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }
    
    public ReleaseBuilder WithReleaseSlug(string releaseSlug)
    {
        _releaseSlug = releaseSlug;
        return this;
    }
}
