using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class ReleaseBuilder()
{
    private Guid? _publicationId;

    public Release Build()
    {
        _publicationId ??= Guid.NewGuid();
        
        var release = new Release
        {
            PublicationId = _publicationId.Value,
            Slug = "release-slug",
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
}
