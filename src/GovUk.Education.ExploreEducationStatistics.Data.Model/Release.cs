#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model;
    
public class Release
{
    public Guid Id { get; set; }
    public Guid PublicationId { get; set; }

    public Release CreateReleaseAmendment(Guid contentAmendmentId) => new()
    {
        Id = contentAmendmentId,
        PublicationId = PublicationId
    };
}
