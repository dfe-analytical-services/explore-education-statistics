using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class ReleaseVersionBuilder(Guid releaseVersionId)
{
    private readonly ReleaseBuilder _releaseBuilder = new();
    private Guid _publicationId; // remove this line when EES-5818 removes ReleaseVersion.PublicationId

    public ReleaseVersion Build()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            Release = _releaseBuilder.Build(),
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
}
