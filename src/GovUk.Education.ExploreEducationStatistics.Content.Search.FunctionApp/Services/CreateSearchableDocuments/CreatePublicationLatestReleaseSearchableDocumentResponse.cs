﻿namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;

public record CreatePublicationLatestReleaseSearchableDocumentResponse
{
    public required string PublicationSlug { get; init; }
    public required Guid ReleaseId { get; init; }
    public required string ReleaseSlug { get; init; }
    public required Guid ReleaseVersionId { get; init; }
    public required string BlobName { get; init; }
}
