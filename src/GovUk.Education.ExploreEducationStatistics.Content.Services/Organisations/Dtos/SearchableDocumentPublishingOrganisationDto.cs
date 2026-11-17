using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;

/// <summary>
/// A publishing organisation included in searchable document response types,
/// served to the Search Docs Function App.
/// Shared by <see cref="Releases.Dtos.ReleaseSearchableDocumentDto"/> and
/// <see cref="DataSets.Dtos.DataSetSearchableDocumentDto"/>.
/// </summary>
public record SearchableDocumentPublishingOrganisationDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public static SearchableDocumentPublishingOrganisationDto FromOrganisation(Organisation organisation) =>
        new() { Id = organisation.Id, Title = organisation.Title };
}
