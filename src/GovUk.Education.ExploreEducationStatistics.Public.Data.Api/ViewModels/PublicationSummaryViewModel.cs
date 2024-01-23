namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public record PublicationSummaryViewModel(Guid Id, string Title, string Slug, string Summary, DateTimeOffset LastPublished);
