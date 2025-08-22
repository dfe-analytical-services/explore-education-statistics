namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Errors;

public record ResourceNotFoundError;

public record PublicationNotFoundError(string PublicationSlug) : ResourceNotFoundError;

public record ReleaseNotFoundError(string PublicationSlug, string ReleaseSlug) : ResourceNotFoundError;
