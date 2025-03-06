using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class PublicationBuilder(Guid publicationId, string publicationSlug)
{
    private Guid? _supercededByPublicationId;

    public Publication Build()
    {
        var publication = new Publication
        {
            Id = publicationId,
            Slug = publicationSlug,
            SupercededById = _supercededByPublicationId
        };
        return publication;
    }

    public PublicationBuilder SupercededBy(Guid supercededByPublicationId)
    {
        _supercededByPublicationId = supercededByPublicationId;
        return this;
    }
}
