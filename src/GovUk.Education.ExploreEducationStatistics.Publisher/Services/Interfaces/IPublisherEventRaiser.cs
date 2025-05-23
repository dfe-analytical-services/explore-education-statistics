﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublisherEventRaiser
{
    Task OnPublicationArchived(
        Guid publicationId,
        string publicationSlug,
        Guid supersededByPublicationId);

    Task OnReleaseVersionsPublished(IReadOnlyList<PublishedPublicationInfo> publishedPublications);
}
