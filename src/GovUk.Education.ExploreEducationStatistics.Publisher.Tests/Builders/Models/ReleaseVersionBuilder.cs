using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class ReleaseVersionBuilder(Guid? releaseVersionId = null)
{
    private readonly ReleaseBuilder _releaseBuilder = new();
    private Guid _publicationId; // remove this line when EES-5818 removes ReleaseVersion.PublicationId
    private Release? _release;

    public ReleaseVersion Build()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId ?? Guid.NewGuid(),
            Release = _release ?? _releaseBuilder.Build(),
            PublicationId = _publicationId // remove this line when EES-5818 removes ReleaseVersion.PublicationId
        };
        return releaseVersion;
    }

    public ReleaseVersionBuilder WithPublicationId(Guid publicationId)
    {
        _releaseBuilder.WithPublicationId(publicationId);
        _publicationId = publicationId; // remove this line when EES-5818 removes ReleaseVersion.PublicationId
        return this;
    }

    public ReleaseVersionBuilder ForRelease(Func<ReleaseBuilder, ReleaseBuilder> modifyRelease)
    {
        modifyRelease(_releaseBuilder);
        return this;
    }

    public ReleaseVersionBuilder ForRelease(Release release)
    {
        _release = release;
        _publicationId = release.PublicationId;
        return this;
    }
}
