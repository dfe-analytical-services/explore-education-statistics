using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class PublicationBuilder(Guid publicationId, string publicationSlug)
{
    private Guid? _supersededByPublicationId;
    private Publication? _supersededByPublication;
    private bool _hasPublishedReleaseVersion;

    public Publication Build()
    {
        var publication = new Publication
        {
            Id = publicationId,
            Slug = publicationSlug,
            LatestPublishedReleaseVersionId = _hasPublishedReleaseVersion ? Guid.NewGuid() : null,
            SupersededById = _supersededByPublicationId,
            SupersededBy = _supersededByPublication
        };
        return publication;
    }

    public PublicationBuilder HasPublishedReleaseVersion()
    {
        _hasPublishedReleaseVersion = true;
        return this;
    }

    public PublicationBuilder SupersededBy(Guid supersededByPublicationId)
    {
        _supersededByPublicationId = supersededByPublicationId;
        return this;
    }

    public PublicationBuilder SupersededBy(Publication supersededByPublication)
    {
        _supersededByPublication = supersededByPublication;
        _supersededByPublicationId = supersededByPublication.Id;
        return this;
    }
}
