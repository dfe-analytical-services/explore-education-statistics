using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class PublicationExtensions
{
    public static bool IsArchived(this Publication publication) =>
        !publication.SupersededById.IsBlank()
        && (publication.SupersededBy?.LatestPublishedReleaseVersionId.HasValue
            ?? throw new ArgumentException(
                $"{nameof(Publication)}.{nameof(publication.SupersededBy)} has not been populated. Ensure it is Included in the entity in the query."));
}
