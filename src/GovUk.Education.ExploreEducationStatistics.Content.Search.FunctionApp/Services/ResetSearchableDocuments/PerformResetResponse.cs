﻿using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;

public class PerformResetResponse
{
    public required PublicationInfo[] AllPublications { get; init; }
}
