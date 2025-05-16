using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class PublicationBuilder(Guid publicationId, string publicationSlug)
{
    private Guid? _latestPublishedReleaseVersionId;
    private Guid? _supersededByPublicationId;

    public Publication Build()
    {
        var publication = new Publication
        {
            Id = publicationId,
            Slug = publicationSlug,
            LatestPublishedReleaseVersionId = _latestPublishedReleaseVersionId,
            SupersededById = _supersededByPublicationId,
        };
        return publication;
    }

    public PublicationBuilder WithLatestPublishedReleaseVersionId(Guid? latestPublishedReleaseVersionId)
    {
        _latestPublishedReleaseVersionId = latestPublishedReleaseVersionId;
        return this;
    }

    public PublicationBuilder SupersededBy(Guid supersededByPublicationId)
    {
        _supersededByPublicationId = supersededByPublicationId;
        return this;
    }
}
