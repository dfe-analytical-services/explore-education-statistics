﻿namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public record RemovePublicationSearchableDocumentsResponse(IReadOnlyDictionary<Guid, bool> ReleaseIdToDeletionResult);
